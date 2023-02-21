using SoulFab.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Data
{
    public class PgDbConfig : DbConfig
    {
        public PgDbConfig(IConfig conf)
            : base(conf)
        {
        }

        public override string ConnectionString
        {
            get
            {
                return $"Server={this._Host};Port={this._Port};Database={this._Database};User Id={this._User};Password={this._Password}";
            }
        }
    }
}
