
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWTimer
{
    public class TimerTool
    {
        private static DateTime originTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// 获取当前毫秒
        /// </summary>
        public static double GetCurMillisecond()
        {
            var delta = DateTime.UtcNow - originTime;
            return delta.TotalMilliseconds;
        }
    }
}
