using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Helper
{
    public static class StringHelper
    {
        public static bool IsInteger(this String s)
        {
            bool ret = true;

            foreach (var c in s)
            {
                if (!Char.IsNumber(c))
                { 
                    ret = false;
                    break;
                }
            }

            return ret;
        }
    }
}
