using System;
using System.Collections.Generic;

namespace SoulFab.Core.Config
{
    public abstract class BaseConfig : IConfig
    {
        protected Dictionary<string, string> ConfigCache;

        protected abstract string GetItem(string key);
        public abstract IConfig GetConfig(string key);

        public BaseConfig()
        {
            this.ConfigCache = new Dictionary<string, string>();
        }

        public string this[string key]
        {
            get
            {
                string ret = null;

                if (this.ConfigCache.ContainsKey(key))
                {
                    ret = this.ConfigCache[key];
                }
                else
                {
                    ret = this.GetItem(key);

                    if (!String.IsNullOrEmpty(ret))
                    {
                        this.ConfigCache[key] = ret;
                    }
                }

                return ret;
            }
        }

        public int GetInt(string key)
        {
            int ret = 0;

            string value = this[key];
            if (value != null)
            {
                Int32.TryParse(value, out ret);
            }

            return ret;
        }

        public double GetFloat(string key)
        {
            double ret = 0;

            string value = this[key];
            if (value != null)
            {
                Double.TryParse(value, out ret);
            }

            return ret;
        }
    }
}
