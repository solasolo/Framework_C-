using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace SoulFab.Core.Data
{
    public class BaseSQLBuilder : ISQLBuilder
    {
        public (string, SQLParameters) PrepareExist(string table, SQLParameters conds)
        {
            string sql;
            SQLParameters param = new SQLParameters();

            sql = "select null from " + this.MakeTableName(table);

            (sql, param) = Merge((sql, param), MakeCondiction(conds));

            return (sql, param);
        }

        public (string, SQLParameters) PrepareDelete(string table, SQLParameters conds)
        {
            string sql = String.Empty;
            SQLParameters param = new SQLParameters();

            sql = "delete from " + this.MakeTableName(table);

            (sql, param) = Merge((sql, param), MakeCondiction(conds));

            return (sql, param);
        }

        public (string, SQLParameters) PrepareSelect(string table, SQLParameters conds)
        {
            string sql = String.Empty;
            SQLParameters param = new SQLParameters();

            sql = "select * from " + this.MakeTableName(table);

            (sql, param) = Merge((sql, param), MakeCondiction(conds));

            return (sql, param);
        }

        public (string, SQLParameters) PrepareUpdate(string table, SQLParameters fields, SQLParameters conds)
        {
            string sql = String.Empty;
            SQLParameters param = new SQLParameters();

            IList<string> FieldList = new List<string>();

            if (fields.Count > 0)
            {
                sql = "update " + this.MakeTableName(table) + " set ";

                foreach (var item in fields)
                { 
                    if (conds[item.Name] == null)
                    {
                        string FieldName = MakeFieldName(item.Name);
                        string ParamName = MakeParamName(item.Name);

                        
                        FieldList.Add(FieldName + "=" + ParamName);

                        param.Add(ParamName, item.Value, item.Type);
                    }
                }

                sql += String.Join(",", FieldList);
            }

            (sql, param) = Merge((sql, param), MakeCondiction(conds));

            return (sql, param);
        }

        public (string, SQLParameters) PrepareInsert(string table, SQLParameters fields)
        {
            string sql = String.Empty;
            SQLParameters param = new SQLParameters();

            IList<string> FieldList = new List<string>();
            IList<string> ValueList = new List<string>();

            if (fields.Count > 0)
            {
                sql = "insert into " + this.MakeTableName(table);

                foreach (var item in fields)
                {
                    string FieldName = MakeFieldName(item.Name);
                    string ParamName = MakeParamName(item.Name);


                    FieldList.Add(FieldName);
                    ValueList.Add(ParamName);

                    param.Add(ParamName, item.Value, item.Type);
                }

                sql += " (" + String.Join(",", FieldList) + ") values (" + String.Join(",", ValueList) + ")";
            }

            return (sql, param);
        }

        private (string, SQLParameters) MakeCondiction(SQLParameters conds)
        {
            string sql = String.Empty;
            SQLParameters param = new SQLParameters();

            for (int i = 0; i < conds.Count; i++)
            {
                var item = conds[i];

                string FieldName = MakeFieldName(item.Name);
                string ParamName = MakeParamName(item.Name);

                sql += (i > 0) ? " and " : " where ";
                sql += FieldName + "=" + ParamName;

                param.Add(ParamName, item.Value, item.Type);
            }

            return (sql, param);
        }

        private (string, SQLParameters) Merge((string, SQLParameters) A, (string, SQLParameters) B)
        {
            string sql;
            SQLParameters param;

            sql = A.Item1 + B.Item1;
            param = A.Item2.Union(B.Item2);

            return (sql, param);
        }

        protected virtual string MakeTableName(string name)
        {
            return "[" + name + "]";
        }

        protected virtual string MakeFieldName(string field)
        {
            return "[" + field + "]";
        }

        protected virtual string MakeParamName(string param)
        {
            return "@" + param;
        }
    }
}
