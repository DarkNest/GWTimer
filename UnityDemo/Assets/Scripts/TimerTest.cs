using UnityEngine;
using GWTimer;


public class TimerTest : MonoBehaviour
{
    TimerBase timer;

    private void Awake()
    {
        Debug.Log("启动定时器");
        timer = new TickTimer()
        //timer = new TickTimer(0)
        //timer = new TickTimer(0, false)
        //timer = new AsyncTimer()
        //timer = new AsyncTimer(false)
        {
            LogAction = str => Debug.Log(str),
            WarnAction = str => Debug.LogWarning(str),
            ErrorAction = str => Debug.LogError(str),
        };

        double startTime = TimerTool.GetCurMillisecond();
        timer.AddTask(1000,
            (id) =>
            {
                double curTime = TimerTool.GetCurMillisecond();
                Debug.Log($"任务id：{id} 时间 {(int)(curTime - startTime)} ms");
            }, null, 10);
    }

    //private void Update()
    //{
    //    TickTimer tickTimer = timer as TickTimer;
    //    if (tickTimer != null)
    //    {
    //        //帧驱动
    //        tickTimer.HandleUpdateTimer();
    //    }
    //    //外部执行Invoke回调
    //    timer.HandleInvoke();
    //}

    private void OnDestroy()
    {
        timer.Dispose();
    }
}
