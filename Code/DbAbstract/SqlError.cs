using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;

namespace SoulFab.Core.Data
{
    [Serializable]
    public class SqlError : Exception, ISerializable
    {
        public readonly string SQL;
        public readonly Dictionary<string, string> Params;

        public SqlError(IDbCommand command, Exception ex)
            : base(ex.Message, ex)
        {
            this.SQL = String.Empty;
            this.Params = new Dictionary<string, string>();

            if (command != null)
            {
                this.SQL = command.CommandText;

                if (command.Parameters != null)
                    foreach (DbParameter p in command.Parameters)
                    {
                        if (p.Value == null)
                        {
                            this.Params.Add(p.ParameterName, "null");
                        }
                        else if (p.Value == DBNull.Value)
                        {
                            this.Params.Add(p.ParameterName, "[NULL]");
                        }
                        else
                        {
                            this.Params.Add(p.ParameterName, p.Value.ToString());
                        }
                    }
            }
        }

        #region ISerializable Members

        public SqlError(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.SQL = info.GetString("sql");
            this.Params = (Dictionary<string, string>)info.GetValue("params", typeof(Dictionary<string, string>));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("sql", this.SQL);
            info.AddValue("params", this.Params);
        }

        #endregion
    }
}
