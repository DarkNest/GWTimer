using System;
using System.Threading.Tasks;
using System.Threading;
using GWTimer;

internal class Program
{
    private static TimerBase timer;

    public static void Main()
    {
        Thread.Sleep(2000);

        Console.WriteLine("====Test====");

        int timeStep = 50;
        timer = new TickTimer(timeStep, true)
        {
            LogAction = Console.WriteLine,
            WarnAction = Console.WriteLine,
            ErrorAction = Console.WriteLine,
        };

        //timer = new AsyncTimer()
        //{
        //    LogAction = Console.WriteLine,
        //    WarnAction = Console.WriteLine,
        //    ErrorAction = Console.WriteLine,
        //};

        int taskID;
        //taskID = AddTask(1000);
        taskID = AddTask(2000);
        taskID = AddTask(3000);

        ////外部驱动任务
        //Task.Run(() =>
        //{
        //    while (true)
        //    {
        //        Thread.Sleep(10);
        //        //timer.HandleUpdateTimer();
        //        ////外部调用
        //        timer.HandleInvoke();
        //    }
        //});


        Task.Run(() =>
        {
            Thread.Sleep(7000);
            Console.WriteLine("移除任务");
            timer.RemoveTask(taskID);
            //Console.WriteLine("重置任务");
            //timer.Reset(taskID);
        });

        Console.ReadKey();
    }

    private static int AddTask(int delay, int cnt = 0)
    {
        double lastTime = TimerTool.GetCurMillisecond();
        double startTime = lastTime;
        int taskID = timer.AddTask(delay,
            (id) =>
            {
                double curTime = TimerTool.GetCurMillisecond();
                double delta = curTime - lastTime;
                double runTime = curTime - startTime;
                Console.WriteLine($"================= 当前时间 {(int)runTime}");
                Console.WriteLine($"时间差：{(int)delta} (ms)");
                Console.WriteLine($"偏差：{(int)(delta - delay)} (ms)");
                Console.WriteLine($"{id} 执行Invoke");
                lastTime = curTime;
            },
            (id) =>
            {
                Console.WriteLine($"{id} 执行Cancel");
            }, cnt);
        return taskID;
    }
}