using System;
using System.Collections.Generic;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using SoulFab.Core.Base;

namespace SoulFab.Core.Data
{
    public class OracleDbSupport : BaseDbSupport
    {
        static private IDictionary<string, string> TypeMap = new Dictionary<string, string>()
        {
            { "VARCHAR2", "string" },
            { "DATE", "datetime" },
        };

        private INamingRule NameConvertor;

        public OracleDbSupport(IDbConfig config)
            : base(config)
        {
            this.NameConvertor = new UnderscoreNaming();
        }

        protected override DbProviderFactory GetFactory()
        {
            return new OracleClientFactory();
        }

        public override void ReadDbScheme(DbScheme scheme)
        {
            foreach (string table_name in this.GetTables())
            {
                scheme.Add(this.GetTableScheme(table_name));
            }
        }

        public override ISQLBuilder GetSQLBuilder()
        {
            return new OracleSQLBuilder();
        }

        private IList<string> GetTables()
        {
            IList<string> ret = new List<string>();

            string sql = "SELECT TABLE_NAME FROM USER_TABLES";
            using (var reader = this.Query(sql))
            {
                while (reader.Read())
                {
                    ret.Add(reader.GetString(0));
                }
            }

            return ret;
        }

        private TableScheme GetTableScheme(string name)
        {
            TableScheme ret = new TableScheme(name); ;

            string sql = "SELECT f.column_name AS \"Name\", f.nullable AS \"Nullable\", f.data_type AS \"Type\", f.data_length AS \"Length\", f.DATA_PRECISION as \"Precision\", f.DATA_SCALE as \"Scale\", c.constraint_type as \"Key\" FROM user_tab_cols f left join (SELECT uc.table_name, ucc.column_name, uc.constraint_type FROM user_cons_columns ucc JOIN user_constraints uc ON uc.constraint_name = ucc.constraint_name WHERE uc.constraint_type = 'P') c on f.table_name = c.table_name and f.column_name = c.column_name WHERE f.table_name = '" + name + "'";
            using (var reader = this.QueryReader(sql))
            {
                while (reader.Read())
                {
                    int len = 0;
                    string field = reader.GetString("Name");
                    string type = reader.GetString("Type");
                    bool isKey = (reader.GetString("Key") == "P");

                    int precision = reader.GetInt32("Precision", 0);
                    int scale = reader.GetInt32("Scale", 0);

                    string db_type = this.FormalType(type, scale); ;
                    string property = this.NameConvertor.From(field.ToLower());

                    FieldDef def = new FieldDef(field, db_type, property, len, precision, isKey);
                    ret.Add(def);
                }
            }

            return ret;
        }

        private string FormalType(string type, int scale)
        {
            string ret = "";

            if (type == "NUMBER")
            {
                ret = scale == 0 ? "int" : "float";
            }
            else
            {
                if (TypeMap.ContainsKey(type))
                {
                    ret = TypeMap[type];
                }
            }

            return ret;
        }
    }
}
