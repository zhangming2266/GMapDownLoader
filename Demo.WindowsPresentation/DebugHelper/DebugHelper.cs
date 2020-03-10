using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetroPro.DebugHelper
{
    public class DebugHelper
    {
        public static bool DebugOn = true;
        public static DebugHelper insetence = new DebugHelper();
        public Dictionary<string, DebugTimer> timers = new Dictionary<string, DebugTimer>();
        public DebugTimer this[string key] 
        {
            get 
            {
                if (!timers.ContainsKey(key))
                {
                    var temp = new DebugTimer();
                    temp.key = key;
                    timers.Add(key, temp);
                }
                return timers[key];
            }
        }
    }
}
