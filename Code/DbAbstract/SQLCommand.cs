using SoulFab.Core.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Data
{
    public class BaseSQLCommand
    {
        protected DbFactory Factory;

        protected DbConnection Connection;
        protected DbCommand Command;
        protected IDbTransaction Transaction = null;

        public BaseSQLCommand(DbFactory factory, bool with_transaction = false)
        {
            Factory = factory;
            Connection = Factory.CreateConnection() as DbConnection;
            Command = Connection.CreateCommand() as DbCommand;

            if (with_transaction)
            {
                Transaction = Connection.BeginTransaction();
                Command.Transaction = Transaction as DbTransaction;
            }
        }

        public void Dispose()
        {
            try
            {
                if (Transaction != null)
                {
                    try
                    {
                        Transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Transaction.Rollback();
                    }
                }
            }
            finally
            {
                Factory.Recycle(Connection);
            }
        }

        protected IDbDataParameter CreateParameter(string parameterName, object value)
        {
            IDbDataParameter param = Command.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = value;

            return param;
        }

        protected IDbDataParameter[] CreateParameters(IDictionary<string, object> nameVlues)
        {
            foreach (KeyValuePair<string, object> pair in nameVlues)
            {
                Type ValueType = pair.Value.GetType();

                if (ValueType == typeof(Int32))
                {

                }

                this.CreateParameter(pair.Key, pair.Value);
            }

            return null;
        }

        protected void SetParameters(SQLParameters param)
        {
            this.ClearParameters();

            foreach (var item in param)
            {
                this.SetParameter(item.Name, item.Value, item.Type);
            }
        }

        protected void SetParameters(IDictionary<string, object> param)
        {
            this.ClearParameters();

            foreach (var item in param)
            {
                SetParameter(item.Key, item.Value);
            }
        }


        public void ClearParameters()
        {
            Command.Parameters.Clear();
        }

        public void SetParameter(string name, object value, CommonType type)
        {
            var param = CreateParameter(name, value);

            Command.Parameters.Add(param);
        }

        public void SetParameter(string name, object value)
        {
            CommonType DbType = CommonType.String;

            if (value != null)
            {
                Type ValueType = value.GetType();

                if (ValueType == typeof(Int32))
                {
                    DbType = CommonType.Integer;
                }
                else if (ValueType == typeof(Int64))
                {
                    DbType = CommonType.LongInt;
                }
                else if (ValueType == typeof(String))
                {
                    DbType = CommonType.String;
                }
                else if (ValueType == typeof(DateTime))
                {
                    DbType = CommonType.DataTime;
                }
                else if (ValueType == typeof(Guid))
                {
                    DbType = CommonType.Guid;
                }
                else if (ValueType == typeof(Decimal))
                {
                    DbType = CommonType.Numeric;
                }
                else if (ValueType == typeof(Boolean))
                {
                    DbType = CommonType.Boolean;
                }
                else if (ValueType == typeof(Double) || ValueType == typeof(Single))
                {
                    DbType = CommonType.Float;
                }
            }

            if (DbType == CommonType.String)
            {
                if (value != null)
                {
                    this.SetParameter(name, value.ToString(), CommonType.String);
                }
                else
                {
                    this.SetParameter(name, null, CommonType.String);
                }
            }
            else
            {
                this.SetParameter(name, value, DbType);
            }

        }

        public void SetParameter(IDictionary<string, object> nameVlues)
        {
            foreach (KeyValuePair<string, object> pair in nameVlues)
            {
                this.SetParameter(pair.Key, pair.Value);
            }
        }
    }

    public class SQLCommand : BaseSQLCommand, ISQLCommand
    {
        public SQLCommand(DbFactory factory, bool with_transaction = false)
            : base(factory, with_transaction)
        {
        }

        #region ISQLCommand Member

        public int Execute(string sql, SQLParameters parameters = null)
        {
            int ret;

            Command.CommandText = sql;

            try
            {
                if (parameters != null)
                {
                    SetParameters(parameters);
                }

                ret = Command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return ret;
        }

        public IDataReader Query(string sql, SQLParameters parameters = null)
        {
            IDataReader reader;

            Command.CommandText = sql;

            try
            {
                if (parameters != null)
                {
                    SetParameters(parameters);
                }

                reader = Command.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return reader;
        }

        public ProxyDataReader QueryReader(string sql, SQLParameters parameters = null)
        {
            IDataReader reader;

            Command.CommandText = sql;

            try
            {
                if (parameters != null)
                {
                    SetParameters(parameters);
                }

                reader = Command.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return new ProxyDataReader(this, reader);
        }

        public object QueryScalar(string sql, SQLParameters parameters = null)
        {
            object ret;

            Command.CommandText = sql;

            try
            {
                if (parameters != null)
                {
                    SetParameters(parameters);
                }

                ret = Command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return ret;
        }

        public bool IfExist(string sql, SQLParameters parameters = null)
        {
            bool ret = false;
            IDataReader dr;

            Command.CommandText = sql;

            try
            {
                if (parameters != null)
                {
                    SetParameters(parameters);
                }

                using (dr = Command.ExecuteReader())
                {
                    if (dr.Read()) ret = true;
                }
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return ret;
        }

        public DataTable RetrieveDataTable(string sql, SQLParameters parameters = null)
        {
            DataTable dt = new DataTable();

            Command.CommandText = sql;

            try
            {
                var Adaper = Factory.CreateAdapter();

                Adaper.Fill(dt);
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return dt;
        }

        public DataSet RetrieveDataSet(string sql, SQLParameters parameters = null)
        {
            DataSet ds = new DataSet();

            Command.CommandText = sql;

            try
            {
                var Adaper = Factory.CreateAdapter();

                Adaper.Fill(ds);
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return ds;
        }

        public void Update(string sql, DataSet ds)
        {
            Command.CommandText = sql;

            try
            {
                var Adaper = Factory.CreateAdapter();
                Adaper.SelectCommand = Command as DbCommand;

                var builder = Factory.CreateCommandBuilder();
                builder.DataAdapter = Adaper;

                Adaper.Update(ds);
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }
        }

        #endregion
    }

    public class AsyncSQLCommand : BaseSQLCommand, IAsyncSQLCommand
    {
        public AsyncSQLCommand(DbFactory factory, bool with_transaction = false)
            : base(factory, with_transaction)
        {
        }

        public async Task<int> ExecuteAsync(string sql, SQLParameters parameters = null)
        {
            int ret;

            Command.CommandText = sql;

            try
            {
                if (parameters != null)
                {
                    SetParameters(parameters);
                }

                ret = await Command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return ret;
        }

        public async Task<IDataReader> QueryAsync(string sql, SQLParameters parameters = null)
        {
            IDataReader reader;

            Command.CommandText = sql;

            try
            {
                if (parameters != null)
                {
                    SetParameters(parameters);
                }

                reader = await Command.ExecuteReaderAsync();
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return reader;
        }

        public async Task<bool> IfExistAsync(string sql, SQLParameters parameters = null)
        {
            bool ret = false;
            IDataReader dr;

            Command.CommandText = sql;

            try
            {
                if (parameters != null)
                {
                    SetParameters(parameters);
                }

                using (dr = await Command.ExecuteReaderAsync())
                {
                    if (dr.Read()) ret = true;
                }
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return ret;
        }

        public async Task<ProxyDataReader> QueryReaderAsync(string sql, SQLParameters parameters = null)
        {
            IDataReader reader;

            Command.CommandText = sql;

            try
            {
                if (parameters != null)
                {
                    SetParameters(parameters);
                }

                reader = await Command.ExecuteReaderAsync();
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return new ProxyDataReader(this, reader);
        }

        public async Task<object> QueryScalarAsync(string sql, SQLParameters parameters = null)
        {
            object ret;

            Command.CommandText = sql;

            try
            {
                if (parameters != null)
                {
                    SetParameters(parameters);
                }

                ret = await Command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                throw new SqlError(Command, ex);
            }

            return ret;
        }
    }
}
