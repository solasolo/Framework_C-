using System;
using System.Collections.Generic;
using System.Text;

namespace SoulFab.Core.Data
{
    class OracleSQLBuilder : BaseSQLBuilder
    {
        protected override string MakeTableName(string name)
        {
            return "\"" + name + "\"";
        }

        protected override string MakeFieldName(string field)
        {
            return "\"" + field + "\"";
        }
    }
}
