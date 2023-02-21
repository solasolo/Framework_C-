using System;
using System.Collections.Generic;
using System.Text;

namespace SoulFab.Core.Config
{
    public interface IConfig
    {
        string this[string key] { get;  }
        
        int GetInt(string key);
        double GetFloat(string key);

        IConfig GetConfig(string key);
    }
}
