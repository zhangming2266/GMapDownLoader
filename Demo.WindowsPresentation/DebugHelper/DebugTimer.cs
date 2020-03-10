using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetroPro.DebugHelper
{
    public class DebugTimer
    {
        public TimeSpan tick = new TimeSpan();
        public string key = "";
        public int count = 0;

        static  System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public DebugTimer()
        {
            sw.Start();
        }

        public void start()
        {
            tick = sw.Elapsed;
        }
        public void end()
        {
            tick = sw.Elapsed - tick;
            System.Diagnostics.Trace.WriteLine(key + ":" + tick.Seconds.ToString()+"秒" + tick.TotalMilliseconds.ToString());
        }
        public string endString
        {
            get
            {
                tick = sw.Elapsed - tick;
                return key + ":" + tick.Seconds.ToString() + "秒" + tick.TotalMilliseconds.ToString();
            }
        }
    }
}
