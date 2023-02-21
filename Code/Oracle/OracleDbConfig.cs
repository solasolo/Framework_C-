using SoulFab.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Data
{
    public class OracleDbConfig : DbConfig
    {
        public OracleDbConfig(IConfig conf)
            : base(conf)
        {
        }

        public override string ConnectionString
        {
            get
            {
                return $"User Id={this._User};Password={this._Password};Data Source={this._Host}:{this._Port}/{this._Database}";
            }
        }
    }
}
