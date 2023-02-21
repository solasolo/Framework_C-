using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SoulFab.Core.Base
{
    public class NoneJsonDecodeNaming : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name;
        }
    }

    public class DefaultJsonDecodeNaming : JsonNamingPolicy
    {
        private INamingRule Rule = new UnderscoreNaming();

        public override string ConvertName(string name)
        {
            return Rule.From(name);
        }
    }

    public class DefaultJsonEncodeNaming : JsonNamingPolicy
    {
        private INamingRule Rule = new UnderscoreNaming();

        public override string ConvertName(string name)
        {
            return Rule.To(name);
        }
    }

    public class UnderscoreNaming : INamingRule
    {
        public string From(string name)
        {
            string ret = "";

            foreach (string part in name.Split('_'))
            {
                ret += Char.ToUpper(part[0]) + part.Substring(1);
            }

            return ret;
        }

        public string To(string name)
        {
            string ret = name;


            return ret;
        }
    }
}
