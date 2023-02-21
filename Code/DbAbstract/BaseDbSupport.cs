using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace SoulFab.Core.Data
{
    public class DbFactory
    {
        public readonly DbProviderFactory Factory;
        public readonly string ConnectionString;

        public DbFactory(DbProviderFactory factory, string connection_string)
        {
            Factory = factory;
            ConnectionString = connection_string;
        }

        public IDbConnection CreateConnection()
        {
            DbConnection conn = Factory.CreateConnection();
            conn.ConnectionString = ConnectionString;
            conn.Open();

            return conn;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            DbConnection conn = Factory.CreateConnection();
            conn.ConnectionString = ConnectionString;
            await conn.OpenAsync();

            return conn;
        }

        public DbDataAdapter CreateAdapter()
        {
            return Factory.CreateDataAdapter();
        }

        public DbCommandBuilder CreateCommandBuilder()
        {
            return Factory.CreateCommandBuilder();
        }

        public void Recycle(IDbConnection conn)
        {
            if ((conn.State == ConnectionState.Open))
            {
                conn.Close();
            }

            conn = null;
        }
    }


    public abstract class BaseDbSupport : IDbSupport, IDBInterface
    {
        protected IDbConfig Config;
        protected DbFactory Factory;

        protected abstract DbProviderFactory GetFactory();
        public abstract void ReadDbScheme(DbScheme scheme);

        protected virtual string GetConnectionString()
        {
            return this.Config.ConnectionString;
        }

        public BaseDbSupport(IDbConfig config)
        {
            this.Config = config;

            Factory = new DbFactory(GetFactory(), GetConnectionString());
        }

        #region ISQLExecutor Members

        public int Execute(string sql, SQLParameters parameters = null)
        {
            int result = 0;

            using (ISQLCommand cmd = this.GetCommand())
            {
                result = cmd.Execute(sql, parameters);
            }

            return result;
        }

        public IDataReader Query(string sql, SQLParameters parameters = null)
        {
            return this.QueryReader(sql, parameters);
        }

        public ProxyDataReader QueryReader(string sql, SQLParameters parameters = null)
        {
            ISQLCommand cmd = GetCommand();
            IDataReader reader = cmd.QueryReader(sql, parameters);

            return new ProxyDataReader(cmd as BaseSQLCommand, reader);
        }

        public object QueryScalar(string sql, SQLParameters parameters = null)
        {
            object result = null;

            using (ISQLCommand cmd = GetCommand())
            {
                result = cmd.QueryScalar(sql, parameters);
            }

            return result;
        }

        public bool IfExist(string sql, SQLParameters parameters = null)
        {
            bool result = false;

            using (ISQLCommand cmd = GetCommand())
            {
                result = cmd.IfExist(sql, parameters);
            }

            return result;
        }

        public void Update(string sql, DataSet ds)
        {
            using (ISQLCommand cmd = GetCommand())
            {
                cmd.Update(sql, ds);
            }
        }

        public DataTable RetrieveDataTable(string sql, SQLParameters parameters = null)
        {
            DataTable ret  = null;

            using (ISQLCommand cmd = GetCommand())
            {
                ret = cmd.RetrieveDataTable(sql, parameters);
            }

            return ret;
        }

        public DataSet RetrieveDataSet(string sql, SQLParameters parameters = null)
        {
            DataSet ret = null;

            using (ISQLCommand cmd = GetCommand())
            {
                ret = cmd.RetrieveDataSet(sql, parameters);
            }

            return ret;
        }

        #endregion


        #region ISQLExecutor Members

        public virtual ISQLBuilder GetSQLBuilder()
        {
            return new BaseSQLBuilder();
        }

        #endregion

        public ISQLCommand GetCommand(bool with_transaction = false)
        {
            return new SQLCommand(Factory, with_transaction);
        }

        public IAsyncSQLCommand GetAsyncCommand(bool with_transaction = false)
        {
            return new AsyncSQLCommand(Factory, with_transaction);
        }

        #region ISQLExecutor Members

        public async Task<int> ExecuteAsync(string sql, SQLParameters parameters = null)
        {
            int result = 0;

            using (IAsyncSQLCommand cmd = this.GetAsyncCommand())
            {
                result = await cmd.ExecuteAsync(sql, parameters);
            }

            return result;
        }

        public async Task<IDataReader> QueryAsync(string sql, SQLParameters parameters = null)
        {
            return await this.QueryReaderAsync(sql, parameters);
        }

        public async Task<ProxyDataReader> QueryReaderAsync(string sql, SQLParameters parameters = null)
        {
            IAsyncSQLCommand cmd = GetAsyncCommand();
            IDataReader reader = await cmd.QueryReaderAsync(sql, parameters);

            return new ProxyDataReader(cmd as BaseSQLCommand, reader);
        }

        public async Task<object> QueryScalarAsync(string sql, SQLParameters parameters = null)
        {
            object result = null;

            using (IAsyncSQLCommand cmd = this.GetAsyncCommand())
            {
                result = await cmd.QueryScalarAsync(sql, parameters);
            }

            return result;
        }

        public async Task<bool> IfExistAsync(string sql, SQLParameters parameters = null)
        {
            bool result = false;

            using (IAsyncSQLCommand cmd = this.GetAsyncCommand())
            {
                result = await cmd.IfExistAsync(sql, parameters);
            }

            return result;
        }

        #endregion
    }
}
