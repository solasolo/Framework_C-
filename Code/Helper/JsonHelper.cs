using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SoulFab.Core.Helper
{
    public class JsonHelper
    {
        public static T Load<T>(string file)
        {
            using var f = new FileStream(file, FileMode.Open, FileAccess.Read);

            return JsonSerializer.Deserialize<T>(f);
        }
    }
}
