using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Config
{
    public class MemoryConfig : BaseConfig
    {
        public override IConfig GetConfig(string key)
        {
            throw new NotImplementedException();
        }

        public void SetItem(string key, string value)
        {
            this.ConfigCache[key] = value;
        }

        public void SetItem(string key, int value)
        {
            this.ConfigCache[key] = value.ToString();
        }

        protected override string GetItem(string key)
        {
            return this.ConfigCache[key];
        }
    }
}
