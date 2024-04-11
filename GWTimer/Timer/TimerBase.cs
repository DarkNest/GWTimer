using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GWTimer
{
    public abstract class TimerBase : IDisposable
    {
        public Action<string> LogAction;
        public Action<string> WarnAction;
        public Action<string> ErrorAction;

        protected ConcurrentDictionary<int, TimerTaskBase> taskDic = new ConcurrentDictionary<int, TimerTaskBase>();
        protected ConcurrentQueue<TimerTaskBase> invokeQueue = new ConcurrentQueue<TimerTaskBase>();

        private object lockObj = new object();
        private int curTaskID = 0;
        
        private bool handleInvoke = false;

        #region 对外接口
        /// <summary>
        /// 添加定时器任务
        /// </summary>
        /// <param name="delay">延时时间(ms)</param>
        /// <param name="onInvoke">执行任务回调</param>
        /// <param name="onCancel">取消任务回调</param>
        /// <param name="count">执行次数</param>
        /// <returns>任务id</returns>
        public int AddTask(int delay, Action<int> onInvoke, Action<int> onCancel = null, int count = 1) 
        {
            TimerTaskBase task = DoCreatTimerTask(delay, onInvoke, onCancel, count);
            if (!taskDic.TryAdd(task.id, task))
            {
                ErrorAction?.Invoke($"添加 task id 已存在 {task.id}");
                return -1;
            }
            return task.id;
        }

        /// <summary>
        /// 删除指定任务
        /// </summary>
        /// <param name="id">任务id</param>
        public bool RemoveTask(int id)
        {
            if (taskDic.TryRemove(id, out TimerTaskBase task))
            {
                //移除任务
                DoRemoveTask(task);
            }
            else
            {
                WarnAction?.Invoke($"task id {id} 移除失败");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 重置指定任务
        /// </summary>
        /// <param name="id">任务id</param>
        public void Reset(int id)
        {
            if (taskDic.ContainsKey(id))
            {
                TimerTaskBase task = taskDic[id];
                DoResetTask(task);
            }
            else
            {
                WarnAction?.Invoke($"找不到需重置的任务：{id}");
            }
        }

        /// <summary>
        /// 重置全部任务
        /// </summary>
        public void Reset()
        {
            foreach (var ky in taskDic)
            {
                TimerTaskBase task = ky.Value;
                DoResetTask(task);
            }
        }

        /// <summary>
        /// 清空任务
        /// </summary>
        public void Clear()
        {
            taskDic.Clear();
            //not surpport
            //invokeQueue.Clear();
            for (int i = 0; i < invokeQueue.Count; i++)
            {
                invokeQueue.TryDequeue(out var rst);
            }
        }

        /// <summary>
        /// 外部处理定时任务
        /// </summary>
        public void HandleInvoke()
        {
            for (int i = 0; i < invokeQueue.Count; i++)
            {
                if (invokeQueue.TryDequeue(out TimerTaskBase task))
                    task.OnInvoke();
                else
                    ErrorAction?.Invoke("定时器回调队列dequeue失败");
            }
        }
        #endregion

        #region 抽象方法
        /// <summary>
        /// 创建定时任务
        /// </summary>
        /// <returns>定时任务</returns>
        protected abstract TimerTaskBase DoCreatTimerTask(int delay, Action<int> onInvoke, Action<int> onCancel, int count);

        /// <summary>
        /// 处理被移除的任务
        /// </summary>
        /// <param name="task">被移除的定时任务</param>
        protected abstract void DoRemoveTask(TimerTaskBase task);

        /// <summary>
        /// 重置任务
        /// </summary>
        /// <param name="task">被重置的任务</param>
        protected abstract void DoResetTask(TimerTaskBase task);

        /// <summary>
        /// 资源释放
        /// </summary>
        public abstract void Dispose();
        #endregion

        #region 内部方法
        /// <summary>
        /// 设置是否手动调用Invoke
        /// </summary>
        protected void SetHandleState(bool handleInvoke)
        {
            this.handleInvoke = handleInvoke;
        }

        /// <summary>
        /// 调用Invoke
        /// </summary>
        protected void ProcessTaskInvoke(TimerTaskBase task)
        {
            if (!handleInvoke)
                task.OnInvoke();
            else
                invokeQueue.Enqueue(task);
        }

        /// <summary>
        /// 生成任务id
        /// </summary>
        /// <returns> 任务id </returns>
        protected int GenTaskID()
        {
            lock (lockObj)
            {
                while (true)
                {
                    curTaskID++;
                    if (curTaskID == int.MaxValue)
                        curTaskID = 1;
                    if (!taskDic.ContainsKey(curTaskID))
                        break;
                }
                return curTaskID;
            }
        }
        #endregion
    }
}
