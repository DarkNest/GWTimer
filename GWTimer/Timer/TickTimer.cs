using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GWTimer
{
    public partial class TickTimer : TimerBase
    {
        //Tick精度(ms)
        private readonly int stepTime;

        // 定时线程相关
        private Task timerTask;
        private CancellationTokenSource cancelTokenSource;
        private CancellationToken cancelToken;

        /// <summary>
        /// TickTimer 构造函数
        /// </summary>
        /// <param name="stepTime">定时器步进时间(为0时由外部驱动)</param>
        /// <param name="autoTask">自动执行Task</param>
        public TickTimer(int stepTime = 50, bool autoTask = true)
        {
            this.stepTime = stepTime;
            SetHandleState(!autoTask);
            if (stepTime > 0)
                RunTimerTask();
        }

        ~TickTimer() 
        {
            Dispose();
        }

        #region 驱动线程相关

        /// <summary>
        /// 启用多线程运行Timer
        /// </summary>
        private void RunTimerTask()
        {
            if (timerTask != null)
            {
                ErrorAction?.Invoke("Tick定时器线程重复启动");
                return;
            }
            cancelTokenSource = new CancellationTokenSource();
            cancelToken = cancelTokenSource.Token;
            timerTask = new Task(TimerTask);
            timerTask.Start();
        }

        /// <summary>
        /// 异步定时器任务
        /// </summary>
        private async void TimerTask()
        {
            long loopCnt = 0;
            int nextStepTime = stepTime;
            double startTime = TimerTool.GetCurMillisecond();
            while (true)
            {
                //延时
                await Task.Delay(nextStepTime, cancelToken).ContinueWith(_ => { });

                //退出线程
                if (cancelTokenSource.IsCancellationRequested)
                    return;

                //更新定时任务
                UpdateTimerTask();

                //延时补偿
                loopCnt++;
                double curTime = TimerTool.GetCurMillisecond();
                double totalRunTime = curTime - startTime;
                double totalStepTime = loopCnt * stepTime;
                double fixTime = totalStepTime - totalRunTime;
                nextStepTime = stepTime + (int)fixTime;
                nextStepTime = Math.Max(nextStepTime, 1);
            }
        }

        /// <summary>
        /// 更新定时器
        /// </summary>
        private void UpdateTimerTask()
        {
            foreach (var ky in taskDic)
            {
                TickTimerTask task = ky.Value as TickTimerTask;
                //更新任务状态
                task.UpdateTaskState();
                //执行任务
                if (task.CanInvoke)
                {
                    //防止执行任务异常
                    try
                    {
                        //执行任务
                        ProcessTaskInvoke(task);
                    }
                    catch (Exception ex)
                    {
                        ErrorAction?.Invoke(ex.Message);
                    }                    
                }
                //任务结束，移除任务
                if (task.IsTaskOver)
                    RemoveTask(task.id);
            }
        }

        /// <summary>
        /// 终止线程
        /// </summary>
        private void StopTimerThread()
        {
            if (timerTask != null)
            {
                LogAction?.Invoke("Tick定时器终止线程");
                cancelTokenSource.Cancel();
                timerTask = null;
            }
        }
        #endregion

        #region 抽象方法实现

        protected override TimerTaskBase DoCreatTimerTask(int delay, Action<int> onInvoke, Action<int> onCancel , int count)
        {
            TickTimerTask task = new TickTimerTask()
            {
                id = GenTaskID(),
                delay = delay,
                onInvoke = onInvoke,
                onCancel = onCancel,
                count = count
            };
            return task;
        }

        protected override void DoRemoveTask(TimerTaskBase task)
        {
            TickTimerTask tickTask = task as TickTimerTask;
            //任务取消回调
            if (!tickTask.IsTaskOver)
                task.OnCancel();
        }

        protected override void DoResetTask(TimerTaskBase task)
        {
            TickTimerTask tickTask = task as TickTimerTask;
            tickTask.ResetTask();
        }

        public override void Dispose()
        {
            StopTimerThread();
            Clear();
        }
        #endregion

        #region 外部接口
        /// <summary>
        /// 外部更新定时器
        /// </summary>
        public void HandleUpdateTimer()
        {
            if (stepTime <= 0)
                UpdateTimerTask();
        }
        #endregion
    }
}
