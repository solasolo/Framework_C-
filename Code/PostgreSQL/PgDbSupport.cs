using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Npgsql;
using SoulFab.Core.Base;

namespace SoulFab.Core.Data
{
    public class PgDbSupport : BaseDbSupport
    {
        static private IDictionary<string, string> TypeMap = new Dictionary<string, string>()
        {
            { "character", "string" },
            { "integer", "int" },
            { "numeric", "numeric" },
            { "array", "array" },
            { "timestamp", "datetime" },
            { "double", "float"},
            { "bytea", "blob"},
            { "json", "json"},

            { "ARRAY", "ARRAY" },
        };

        private INamingRule NameConvertor;

        public PgDbSupport(IDbConfig config)
            : base(config)
        {
            this.NameConvertor = new UnderscoreNaming();
        }

        protected override DbProviderFactory GetFactory()
        {
            return NpgsqlFactory.Instance;
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
            return new PgSQLBuilder();
        }

        private IList<string> GetTables()
        {
            IList<string> ret = new List<string>();

            string sql = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'";
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

            string sql = "SELECT cl.column_name AS \"Name\", cl.is_nullable AS \"Nullable\", data_type AS \"Type\", cl.character_maximum_length AS \"Length\", st.constraint_type AS \"Key\" FROM INFORMATION_SCHEMA.COLUMNS cl LEFT JOIN(SELECT kc.table_name, kc.column_name, ts.constraint_type FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kc JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS ts ON kc.constraint_name = ts.constraint_name) st ON st.column_name = cl.column_name and st.table_name = cl.table_name WHERE cl.table_name = '" + name + "'";
            using (var reader = this.QueryReader(sql))
            {
                while (reader.Read())
                {
                    int len = 0;
                    string field = reader.GetString("Name");
                    string type = reader.GetString("Type");
                    bool isKey = (reader.GetString("Key") == "PRIMARY KEY");

                    string db_type = this.FormalType(type);
                    string property = this.NameConvertor.From(field);

                    FieldDef def = new FieldDef(field, db_type, property, len, 0, isKey);
                    ret.Add(def);
                }
            }

            return ret;
        }

        private string FormalType(string type)
        {
            string ret = "";

            int pos = type.IndexOf(' ');
            if (pos > -1)
            {
                type = type.Substring(0, pos);
            }

            if (TypeMap.ContainsKey(type))
            {
                ret = TypeMap[type];
            }

            return ret;
        }
    }
}
