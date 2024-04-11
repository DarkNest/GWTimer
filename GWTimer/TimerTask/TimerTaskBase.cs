using System;

namespace GWTimer
{
    public abstract class TimerTaskBase
    {
        public int id;                     //任务ID
        public int delay;                  //任务延时
        public int count;                  //任务执行次数
        public Action<int> onInvoke;       //任务回调
        public Action<int> onCancel;       //任务取消回调

        protected bool isTaskOver;    //任务结束  
        public bool IsTaskOver { get { return isTaskOver; } }


        public void OnInvoke()
        {
            onInvoke?.Invoke(id);
        }

        public void OnCancel()
        {
            onCancel?.Invoke(id);
        }
    }
}
