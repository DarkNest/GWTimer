using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GWTimer
{
    public partial class AsyncTimer : TimerBase
    {
        public AsyncTimer(bool autoTask = true) 
        { 
            SetHandleState(!autoTask);
        }

        ~AsyncTimer() 
        {
            Dispose();
        }

        #region 驱动线程相关
        private void RunTask(AsyncTimerTask task)
        {
            task.timerTask = new Task(async () =>
            {
                int nextStepTime = task.delay;
                double startTime = TimerTool.GetCurMillisecond();
                while (!task.IsTaskOver)
                {
                    //延时
                    await Task.Delay(nextStepTime, task.cancelToken).ContinueWith(_ => { });

                    //退出线程
                    if (task.cancelTokenSource.IsCancellationRequested)
                        return;

                    //更新任务状态
                    task.UpdateTaskState();

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
                    
                    //任务结束，移除任务
                    if (task.IsTaskOver)
                        RemoveTask(task.id);

                    //延时补偿                
                    double curTime = TimerTool.GetCurMillisecond();
                    double totalRunTime = curTime - startTime;
                    double totalStepTime = task.loopCnt * task.delay;
                    double fixTime = totalStepTime - totalRunTime;
                    nextStepTime = task.delay + (int)fixTime;
                    nextStepTime = Math.Max(nextStepTime, 1);
                }
            });
            task.timerTask.Start();
        }
        #endregion

        #region 抽象方法实现
        protected override TimerTaskBase DoCreatTimerTask(int delay, Action<int> onInvoke, Action<int> onCancel, int count)
        {
            AsyncTimerTask task = new AsyncTimerTask()
            {
                id = GenTaskID(),
                delay = delay,
                onInvoke = onInvoke,
                onCancel = onCancel,
                count = count
            };
            RunTask(task);
            return task;
        }

        protected override void DoRemoveTask(TimerTaskBase task)
        {
            AsyncTimerTask asyncTask = task as AsyncTimerTask;
            //任务取消回调
            if (!asyncTask.IsTaskOver)
                asyncTask.OnCancel();
            //取消任务进程
            asyncTask.CancelTask();
        }

        protected override void DoResetTask(TimerTaskBase task)
        {
            AsyncTimerTask asyncTask = task as AsyncTimerTask;
            asyncTask.ResetTask();
        }

        public override void Dispose()
        {
            //终止全部进程
            foreach(var kv in taskDic)
            {
                AsyncTimerTask asyncTask = kv.Value as AsyncTimerTask;
                asyncTask.CancelTask();
            }
            Clear();
        }
        #endregion
    }
}
