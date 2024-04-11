using System;
using System.Threading;
using System.Threading.Tasks;

namespace GWTimer
{
    public partial class AsyncTimer
    {
        private class AsyncTimerTask : TimerTaskBase
        {
            public CancellationTokenSource cancelTokenSource;
            public CancellationToken cancelToken;
            public Task timerTask;
            public long loopCnt = 0;

            public AsyncTimerTask()
            {
                cancelTokenSource = new CancellationTokenSource();
                cancelToken = cancelTokenSource.Token;
                ResetTask();
            }

            public void UpdateTaskState()
            {
                loopCnt++;
                if (loopCnt >= count && count != 0)
                    isTaskOver = true;
            }

            /// <summary>
            /// 重置任务
            /// </summary>
            public void ResetTask()
            {
                loopCnt = 0;
                isTaskOver = false;
            }

            public void CancelTask()
            {
                cancelTokenSource.Cancel();
            }
        }
    }
}
