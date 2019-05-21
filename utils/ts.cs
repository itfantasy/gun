using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace itfantasy.gun.utils
{
    public class ts
    {
        public static long MS()
        {
            return MS(DateTime.Now.Ticks);
        }

        public static long MS(long tick)
        {
            DateTime timeStamp = new DateTime(1970, 1, 1, 8, 0, 0);
            return (tick - timeStamp.Ticks) / 10000;
        }
    }
}
