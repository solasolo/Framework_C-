using SoulFab.Core.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoulFab.Core.Data
{
    public abstract class DbConfig : IDbConfig
    {
        protected string _Host;
        protected int _Port;
        protected string _Database;
        protected string _User;
        protected string _Password;

        public DbConfig(IConfig conf)
        {
            this._Host = conf["Host"];
            this._Port = conf.GetInt("Port");
            this._Database = conf["Database"];
            this._User = conf["User"];
            this._Password = conf["Password"];
        }

        public string Host
        {
            get 
            {
                return _Host;
            }
        }

        public abstract string ConnectionString { get; }
    }


    public class ConstantDbConfig : IDbConfig
    {
        protected string _ConnectionString;

        public ConstantDbConfig(string conn_str)
        {
            this._ConnectionString = conn_str;        
        }

        public string ConnectionString
        {
            get
            {
                return this._ConnectionString;
            }
        }
    }
}
