using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using SoulFab.Core.Base;
using SoulFab.Core.Helper;

namespace SoulFab.Core.Data
{
    public class ORMapping
    {
        private IDBInterface Db;
        private INamingRule NameConvertor;
        private DbScheme Scheme;

        public ORMapping(IDBInterface db, string cfg_file = null)
        {
            this.Db = db;
            this.Scheme = new DbScheme();
            NameConvertor = new UnderscoreNaming();

            db.ReadDbScheme(this.Scheme);

            if (!String.IsNullOrEmpty(cfg_file))
            {
                SchemeConfigure config = new SchemeConfigure();

                config.Load(this.Scheme, cfg_file);
            }
        }

        #region Private Method

        private SQLParameters MakeKeyItems(TableScheme td, object obj)
        {
            SQLParameters Result = new SQLParameters();

            foreach (FieldDef fd in td.KeyFields)
            {
                object key_value = null;

                if (fd.GetObjectValue(obj, ref key_value))
                {
                    Result.Add(fd.Name, key_value, fd.Type);
                }
            }

            return Result;
        }

        private SQLParameters MakeCondItems(TableScheme td, object obj)
        {
            SQLParameters Result = new SQLParameters();

            foreach (var pf in obj.GetType().GetProperties())
            {
                var fd = td.ByProperty(pf.Name);
                if (fd != null)
                {
                    var reader = pf.GetGetMethod();
                    var value = reader.Invoke(obj, null);
                    Result.Add(fd.Name, value, fd.Type);
                }
            }

            return Result;
        }

        private void FetchObject(object obj, IDataReader reader, TableScheme td)
        {
            Type type = obj.GetType();

            foreach (FieldDef fd in td.Fields)
            {
                if (fd == null) continue;

                string FieldName = fd.Name;
                string PropName = fd.Property;
                object val_obj = null;

                if (!fd.IsTable)
                {
                    int FieldIndex = reader.GetOrdinal(FieldName);
                    val_obj = reader.GetValue(FieldIndex);

                    if (val_obj != null && val_obj != DBNull.Value)
                    {
                        switch (fd.Type)
                        {
                            case CommonType.Guid:
                                fd.SetObjectValue(obj, new Guid(val_obj.ToString()));
                                break;

                            case CommonType.DataTime:
                                fd.SetObjectValue(obj, DateTime.Parse(val_obj.ToString()));
                                break;

                            //case CommonDBType.String:
                            //    SetObjectField(type, obj, PropName, val_obj.ToString());
                            //    break;

                            default:
                                fd.SetObjectValue(obj, val_obj);
                                break;
                        }
                    }
                    else
                    {
                        fd.SetObjectValue(obj, null);
                    }
                }
                else
                {
                    object table_obj = null;
                    TableScheme table_fd = this.Scheme[fd.Name];

                    FieldInfo ObjField = type.GetField(fd.Property, BindingFlags.Public | BindingFlags.Instance);
                    if (ObjField != null)
                    {
                        table_obj = ObjField.GetValue(obj);
                        if (table_obj != null)
                        {
                            this.FetchObject(table_obj, reader, table_fd);
                        }
                        else
                        {
                            //TODO:
                        }
                    }
                }
            }
        }

        private void FetchRawObject(object obj, IDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string name = reader.GetName(i);
                string property = this.NameConvertor.From(name);

                object value = reader.GetValue(i);
                if (value == DBNull.Value)
                {
                    ReflectHelper.SetValue(obj, property, null);
                }
                else
                {
                    ReflectHelper.SetValue(obj, property, value);
                }
            }
        }

        private void MakeUpdateData(TableScheme td, object obj, object old_obj, SQLParameters items)
        {
            foreach (FieldDef fd in td.Fields)
            {
                string FieldName = fd.Name;

                if (fd.IsTable)
                {
                    TableScheme table_td = this.Scheme[fd.Name];

                    object table_obj = null;
                    object old_table_obj = null;
                    if (fd.GetObjectValue(old_obj, ref table_obj) && fd.GetObjectValue(obj, ref old_table_obj))
                    {
                        this.MakeUpdateData(table_td, table_obj, old_table_obj, items);
                    }
                }
                else
                {
                    if (!fd.IsAuto)
                    {
                        object FieldValue = null;
                        object OldFieldValue = null;

                        if (fd.GetObjectValue(obj, ref FieldValue) && fd.GetObjectValue(old_obj, ref OldFieldValue))
                        {
                            if (fd.Type == CommonType.Guid)
                            {
                                FieldValue = FieldValue == null ? null : FieldValue.ToString();
                                OldFieldValue = OldFieldValue == null ? null : OldFieldValue.ToString();
                            }
                            if (fd.Type == CommonType.DataTime)
                            {
                                if ((DateTime)FieldValue == DateTime.MinValue)
                                {
                                    FieldValue = null;
                                }

                                if ((DateTime)OldFieldValue == DateTime.MinValue)
                                {
                                    OldFieldValue = null;
                                }
                            }

                            if ((FieldValue == null ^ OldFieldValue == null) || (FieldValue != null && !FieldValue.Equals(OldFieldValue)))
                            {
                                items.Add(FieldName, FieldValue, OldFieldValue, fd.Type);
                            }
                        }
                    }
                }
            }
        }

        private void MakeUpdateData(TableScheme td, object obj, SQLParameters items)
        {
            foreach (FieldDef fd in td.Fields)
            {
                string FieldName = fd.Name;

                if (fd.IsTable)
                {
                    TableScheme table_td = this.Scheme[fd.Name];
                    object table_obj = null;

                    if (fd.GetObjectValue(obj, ref table_obj))
                    {
                        this.MakeUpdateData(table_td, table_obj, items);
                    }
                }
                else
                {
                    if (!fd.IsAuto)
                    {
                        object FieldValue = null;

                        if (fd.GetObjectValue(obj, ref FieldValue))
                        {
                            if (fd.Type == CommonType.Guid)
                            {
                                FieldValue = FieldValue == null ? null : FieldValue;
                            }
                            if (fd.Type == CommonType.DataTime)
                            {
                                if ((FieldValue != null) && ((DateTime)FieldValue == DateTime.MinValue))
                                {
                                    FieldValue = null;
                                }
                            }

                            items.Add(FieldName, FieldValue, fd.Type);
                        }
                    }
                }
            }
        }

        private long UpdateObject(object obj, TableScheme td, SQLParameters SQLData)
        {
            long Result = 0;

            /*
            if (this.IfExist(td, obj, db))
            {
                SQLParameters CondData = this.MakeKeyItems(td, obj);
                string sql = db.GetSQLBuilder().PrepareUpdate(td.TableName, SQLData, CondData, db);
                Result = -db.ExecuteNonQuery(sql);
            }
            else
            {
                Result = this.InsertObject(obj, db, td, SQLData);
            }
            */

            SQLParameters CondData = this.MakeKeyItems(td, obj);
            var (sql, param) = Db.GetSQLBuilder().PrepareUpdate(td.TableName, SQLData, CondData);
            Result = -(Db as ISQLExecutor).Execute(sql, param);

            if (Result == 0)
            {
                Result = this.InsertObject(obj, td, SQLData);
            }

            return Result;
        }

        private long InsertObject(object obj, TableScheme td, SQLParameters SQLData)
        {
            long Result = 0;

            var (sql, param) = Db.GetSQLBuilder().PrepareInsert(td.TableName, SQLData);

            using (var cmd = (Db as IDbSupport).GetCommand())
            {
                Result = -cmd.Execute(sql, param);

                if (td.HasAuto)
                {
                    sql += " SELECT @@IDENTITY;";

                    object rt = cmd.QueryScalar(sql);
                    if (rt != DBNull.Value)
                    {
                        Result = Convert.ToInt64(rt);
                    }
                }
            }

            return Result;
        }

        private bool IfExist(TableScheme td, object obj)
        {
            bool Result = false;

            SQLParameters ItemList = this.MakeKeyItems(td, obj);

            if (ItemList.Count > 0)
            {
                var (sql, param) = Db.GetSQLBuilder().PrepareExist(td.TableName, ItemList);

                using (var cmd = (Db as IDbSupport).GetCommand())
                {
                    Result = (cmd.QueryScalar(sql, param) != null);
                }
            }

            return Result;
        }

        private async Task<T> SelectOneAsync<T>(TableScheme td, SQLParameters CondData) where T : new()
        {
            var ret = default(T);

            var (sql, param) = this.Db.GetSQLBuilder().PrepareSelect(td.TableName, CondData);
            var reader = await (Db as IAsyncExecutor).QueryReaderAsync(sql, param);

            ret = this.FetchOne<T>(reader);

            return ret;
        }

        private async Task<IList<T>> SelectAsync<T>(TableScheme td, SQLParameters CondData) where T : new()
        {
            var ret = default(IList<T>);

            var (sql, param) = this.Db.GetSQLBuilder().PrepareSelect(td.TableName, CondData);
            var reader = await (Db as IAsyncExecutor).QueryReaderAsync(sql, param);

            ret = this.FetchAll<T>(reader);

            return ret;
        }

        private async Task<long> UpdateObjectAsync(object obj, TableScheme td, SQLParameters SQLData)
        {
            long Result = 0;

            /*
            if (this.IfExist(td, obj, db))
            {
                SQLParameters CondData = this.MakeKeyItems(td, obj);
                string sql = db.GetSQLBuilder().PrepareUpdate(td.TableName, SQLData, CondData, db);
                Result = -db.ExecuteNonQuery(sql);
            }
            else
            {
                Result = this.InsertObject(obj, db, td, SQLData);
            }
            */

            SQLParameters CondData = this.MakeKeyItems(td, obj);
            var (sql, param) = Db.GetSQLBuilder().PrepareUpdate(td.TableName, SQLData, CondData);

            using (var cmd = (Db as IDbSupport).GetAsyncCommand())
            {
                Result = -(await cmd.ExecuteAsync(sql, param));

                if (Result == 0)
                {
                    Result = await this.InsertObjectAsync(obj, td, SQLData);
                }
            }

            return Result;
        }

        private async Task<long> InsertObjectAsync(object obj, TableScheme td, SQLParameters SQLData)
        {
            long Result = 0;

            var (sql, param) = Db.GetSQLBuilder().PrepareInsert(td.TableName, SQLData);

            using (var cmd = (Db as IDbSupport).GetAsyncCommand())
            {
                Result = -(await cmd.ExecuteAsync(sql, param));

                if (td.HasAuto)
                {
                    sql += " SELECT @@IDENTITY;";

                    object rt = await cmd.QueryScalarAsync(sql);
                    if (rt != DBNull.Value)
                    {
                        Result = Convert.ToInt64(rt);
                    }
                }
            }

            return Result;
        }

        private async Task<bool> IfExistAsync(TableScheme td, object obj)
        {
            bool Result = false;

            SQLParameters ItemList = this.MakeKeyItems(td, obj);

            if (ItemList.Count > 0)
            {
                var (sql, param) = Db.GetSQLBuilder().PrepareExist(td.TableName, ItemList);

                using (var cmd = (Db as IDbSupport).GetAsyncCommand())
                {
                    Result = (await cmd.QueryScalarAsync(sql, param) != null);
                }
            }

            return Result;
        }

        private SQLParameters MakeUpdateData(object obj)
        {
            SQLParameters Items = new SQLParameters();

            Type type = obj.GetType();
            TableScheme td = this.Scheme[type];

            this.MakeUpdateData(td, obj, Items);

            return Items;
        }

        #endregion

        #region Public Method

        public List<T> FetchRawAll<T>(IDataReader reader) where T : new()
        {
            List<T> ret = new List<T>();

            using (reader)
            {
                while (reader.Read())
                {
                    T obj = new T();

                    this.FetchRawObject(obj, reader);

                    ret.Add(obj);
                }
            }

            return ret;
        }

        public IList<T> FetchAll<T>(IDataReader reader) where T : new()
        {
            List<T> ret = new List<T>();

            using (reader)
            {
                while (reader.Read())
                {
                    T obj = new T();

                    this.Fetch(obj, reader);

                    ret.Add(obj);
                }
            }

            return ret;
        }

        public T FetchOne<T>(IDataReader reader) where T : new()
        {
            T ret = default(T);

            using (reader)
            {
                while (reader.Read())
                {
                    ret = new T();

                    this.Fetch(ret, reader);
                }
            }

            return ret;
        }

        public IList<T> FetchList<T>(IDataReader reader, string field)
        {
            IList<T> ret = new List<T>();

            using (reader)
            {
                if (reader.Read())
                {
                    var value = reader.GetValue(reader.GetOrdinal(field));
                    ret.Add((T)value);
                }
            }

            return ret;
        }

        public void Fetch<T>(T obj, IDataReader reader)
        {
            Type ObjectType = typeof(T);
            TableScheme td = this.Scheme[ObjectType];

            if (td != null)
            {
                this.FetchObject(obj, reader, td);
            }
            else
            {
                this.FetchRawObject(obj, reader);
            }
        }

        public bool IfExist<T>(T obj)
        {
            Type type = typeof(T);
            TableScheme td = this.Scheme[type];

            return this.IfExist(td, obj);
        }

        public long Insert<T>(T obj)
        {
            TableScheme td = this.Scheme[typeof(T)];

            SQLParameters SQLData = new SQLParameters();
            this.MakeUpdateData(td, obj, SQLData);

            return InsertObject(obj, td, SQLData);
        }

        public long Update<T>(T obj)
        {
            Type type = obj.GetType();
            TableScheme td = this.Scheme[type];

            SQLParameters SQLData = new SQLParameters();
            this.MakeUpdateData(td, obj, SQLData);

            return UpdateObject(obj, td, SQLData);
        }

        public SQLParameters Update<T>(T obj, T old_obj)
        {
            Type type = typeof(T);

            TableScheme td = this.Scheme[type];

            SQLParameters SQLData = new SQLParameters();
            if (old_obj != null)
            {
                this.MakeUpdateData(td, obj, old_obj, SQLData);

                if (SQLData.Count > 0)
                {
                    SQLParameters CondData = this.MakeKeyItems(td, obj);
                    var (sql, param) = Db.GetSQLBuilder().PrepareUpdate(td.TableName, SQLData, CondData);

                    int rt = (Db as ISQLExecutor).Execute(sql, param);
                    if (rt != 1) SQLData = null;
                }
            }
            else
            {
                this.MakeUpdateData(td, obj, SQLData);
                this.UpdateObject(obj, td, SQLData);
            }

            return SQLData;
        }

        public void Get<T>(T obj)
        {
            Type type = typeof(T);
            TableScheme td = this.Scheme[type];

            SQLParameters ItemList = null;
            //if (td.KeyFields.Count > 1)
            {
                ItemList = this.MakeKeyItems(td, obj);
            }
            //else
            //{
            //    ItemList = new SQLParameters();
            //    ItemList.Add(new SQLItem(td.KeyFields[0].Name, obj, td.KeyFields[0].Type));
            //}

            var (sql, param) = Db.GetSQLBuilder().PrepareSelect(td.TableName, ItemList);
            IDataReader Reader = (Db as ISQLExecutor).Query(sql, param);

            using (Reader)
            {
                if (Reader.Read())
                {
                    FetchObject(obj, Reader, td);
                }
            }
        }

        public async Task<T> SelectOneAsync<T>(object cond) where T : new()
        {
            TableScheme td = this.Scheme[typeof(T)];

            SQLParameters CondData = this.MakeCondItems(td, cond);

            return await this.SelectOneAsync<T>(td, CondData);
        }

        public async Task<IList<T>> SelectAsync<T>(object cond) where T: new()
        {
            TableScheme td = this.Scheme[typeof(T)];

            SQLParameters CondData = this.MakeCondItems(td, cond);

            return await this.SelectAsync<T>(td, CondData);
        }


        public async Task<bool> IfExistAsync<T>(T obj)
        {
            return await this.IfExistAsync<T>((object)obj);
        }

        public async Task<bool> IfExistAsync<T>(object obj)
        {
            TableScheme td = this.Scheme[typeof(T)];

            return await this.IfExistAsync(td, obj);
        }

        public async Task<long> InsertAsync<T>(T obj)
        {
            return await this.InsertAsync<T>((object)obj);
        }

        public async Task<long> InsertAsync<T>(object obj)
        {
            TableScheme td = this.Scheme[typeof(T)];

            SQLParameters SQLData = new SQLParameters();
            this.MakeUpdateData(td, obj, SQLData);

            return await this.InsertObjectAsync(obj, td, SQLData);
        }

        public async Task<long> UpdateAsync<T>(object obj)
        {
            TableScheme td = this.Scheme[typeof(T)];

            SQLParameters SQLData = new SQLParameters();
            this.MakeUpdateData(td, obj, SQLData);

            return await this.UpdateObjectAsync(obj, td, SQLData);
        }

        public async Task<long> UpdateAsync<T>(T obj)
        {
            return await this.UpdateAsync<T>((object)obj);
        }

        public async Task<T> GetAsync<T>(object cond) where T : new()
        {
            T ret = default(T);

            TableScheme td = this.Scheme[typeof(T)];

            SQLParameters SQLData = new SQLParameters();
            if (cond != null)
            {
                SQLParameters CondData = this.MakeKeyItems(td, cond);
                var (sql, param) = Db.GetSQLBuilder().PrepareSelect(td.TableName, CondData);

                var reader = await (Db as IAsyncExecutor).QueryReaderAsync(sql, param);

                ret = this.FetchOne<T>(reader);
            }

            return ret;
        }

        public async Task<T> GetAsync<T, U>(U cond)
            where T : new()
        {
            return await this.GetAsync<T>(cond);
        }

        public async Task<long> SetAsync<T>(T obj)
        {
            return await this.SetAsync<T>((object)obj);
        }

        public async Task<long> SetAsync<T>(object obj)
        {
            long ret;

            if (await this.IfExistAsync<T>(obj))
            {
                ret = await this.UpdateAsync<T>(obj);
            }
            else
            {
                ret = await this.InsertAsync<T>(obj);
            }

            return ret;
        }

        public async Task<SQLParameters> UpdateAsync<T>(T obj, T old_obj)
        {
            Type type = typeof(T);

            TableScheme td = this.Scheme[type];

            SQLParameters SQLData = new SQLParameters();
            if (old_obj != null)
            {
                this.MakeUpdateData(td, obj, old_obj, SQLData);

                if (SQLData.Count > 0)
                {
                    SQLParameters CondData = this.MakeKeyItems(td, obj);
                    var (sql, param) = Db.GetSQLBuilder().PrepareUpdate(td.TableName, SQLData, CondData);

                    int rt = await (Db as IAsyncExecutor).ExecuteAsync(sql, param);
                    if (rt != 1) SQLData = null;
                }
            }
            else
            {
                this.MakeUpdateData(td, obj, SQLData);
                await this.UpdateObjectAsync(obj, td, SQLData);
            }

            return SQLData;
        }

        #endregion
    }
}

