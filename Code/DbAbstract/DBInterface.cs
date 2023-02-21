using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using SoulFab.Core.Base;

namespace SoulFab.Core.Data
{
    public class SQLItem
    {
        public readonly string Name;
        public readonly CommonType Type;
        public readonly object Value;
        public readonly object OldValue;

        public SQLItem(string name, object value)
        {
            this.Name = name;
            this.Value = value == null ? DBNull.Value : value; ;
        }

        public SQLItem(string name, object value, CommonType type)
        {
            this.Name = name;
            this.OldValue = null;
            this.Value = value == null ? DBNull.Value : value;
            this.Type = type;
        }

        public SQLItem(string name, object value, object old_value, CommonType type)
            : this(name, value, type)
        {
            this.OldValue = old_value;
        }
    }

    public class SQLParameters : IEnumerable<SQLItem>
    {
        private IList<SQLItem> Items;

        public SQLParameters()
        {
            this.Items = new List<SQLItem>();
        }

        private SQLParameters(IList<SQLItem> lst)
        {
            this.Items = lst;
        }

        public int Count
        {
            get
            {
                return this.Items.Count;
            }
        }

        public SQLItem this[int index]
        {
            get
            {
                return this.Items[index];
            }
        }

        public SQLItem this[string name]
        {
            get
            {
                SQLItem ret = null;

                foreach (var item in this.Items)
                { 
                    if(item.Name == name)
                    {
                        ret = item;
                        break;
                    }
                }

                return ret;
            }
        }

        public void Add(string name, object value)
        {
            this.Items.Add(new SQLItem(name, value));
        }

        public void Add(string name, object value, CommonType type)
        {
            this.Items.Add(new SQLItem(name, value, type));
        }

        public void Add(string name, object value, object old_value, CommonType type)
        {
            this.Items.Add(new SQLItem(name, value, old_value, type));
        }

        public SQLParameters Union(SQLParameters other)
        {
            return new SQLParameters(this.Items.Union(other.Items).ToArray());
        }

        public IEnumerator<SQLItem> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }
    }

    public struct SQLPrepare
    {
        public string SQL;
        public List<SQLItem> Parameters;
    }

    public interface IDbConfig
    {
        string ConnectionString { get; }
    }

    public interface ISQLBuilder
    {
        (string, SQLParameters) PrepareExist(string table, SQLParameters conds);
        (string, SQLParameters) PrepareDelete(string table, SQLParameters condb);
        (string, SQLParameters) PrepareSelect(string table, SQLParameters condb);
        (string, SQLParameters) PrepareUpdate(string table, SQLParameters fields, SQLParameters conds);
        (string, SQLParameters) PrepareInsert(string table, SQLParameters fields);
    }

    public interface ISQLExecutor
    {
        int Execute(string sql, SQLParameters parameters = null);
        IDataReader Query(string sql, SQLParameters parameters = null);

        ProxyDataReader QueryReader(string sql, SQLParameters parameters = null);
        void Update(string sql, DataSet ds);
        object QueryScalar(string sql, SQLParameters parameters = null);
        bool IfExist(string sql, SQLParameters parameters = null);

        DataTable RetrieveDataTable(string sql, SQLParameters parameters = null);
        DataSet RetrieveDataSet(string sql, SQLParameters parameters = null);
    }

    public interface IAsyncExecutor
    {
        Task<int> ExecuteAsync(string sql, SQLParameters parameters = null);
        Task<IDataReader> QueryAsync(string sql, SQLParameters parameters = null);

        Task<ProxyDataReader> QueryReaderAsync(string sql, SQLParameters parameters = null);
        Task<object> QueryScalarAsync(string sql, SQLParameters parameters = null);
        Task<bool> IfExistAsync(string sql, SQLParameters parameters = null);
    }

    public interface ISQLCommand : ISQLExecutor, IDisposable
    {
        void ClearParameters();
        void SetParameter(string name, object value);
    }

    public interface IAsyncSQLCommand : IAsyncExecutor, IDisposable
    {
    }

    public interface IDBInterface
    {
        ISQLBuilder GetSQLBuilder();
        void ReadDbScheme(DbScheme scheme);
    }

    public interface IDbSupport : ISQLExecutor, IAsyncExecutor
    {
        ISQLCommand GetCommand(bool with_transaction = false);
        IAsyncSQLCommand GetAsyncCommand(bool with_transaction = false);
    }
}
