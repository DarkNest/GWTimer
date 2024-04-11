using System;
using System.Threading.Tasks;

namespace GWTimer
{
    public partial class TickTimer 
    {
        private class TickTimerTask : TimerTaskBase
        {
            private double startTime;       //起始时间
            private int loopCnt;            //循环次数
            private bool canInvoke;         //是否可调用
            public bool CanInvoke
            {
                get
                {
                    bool flag = canInvoke;
                    canInvoke = false;
                    return flag;
                }
            }

            public TickTimerTask()
            {
                ResetTask();
            }

            public void UpdateTaskState()
            {
                double curTime = TimerTool.GetCurMillisecond();
                double runTime = curTime - startTime;
                if (runTime > delay * (loopCnt + 1))
                {
                    loopCnt++;
                    //定时调用标记
                    canInvoke = true;
                    //任务结束标记
                    if (loopCnt >= count && count > 0)
                        isTaskOver = true;
                }
            }

            /// <summary>
            /// 重置任务
            /// </summary>
            public void ResetTask()
            {
                startTime = TimerTool.GetCurMillisecond();
                loopCnt = 0;
                isTaskOver = false;
                canInvoke = false;
            }
        }
    }
}
