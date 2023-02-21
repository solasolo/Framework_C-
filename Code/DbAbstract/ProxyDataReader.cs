using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SoulFab.Core.Data
{
    public class ProxyDataReader : IEnumerable, IDataReader, IDisposable
    {
        private BaseSQLCommand dba;
        private IDataReader dbr;

        public ProxyDataReader(BaseSQLCommand db, IDataReader reader)
        {
            this.dba = db;
            this.dbr = reader;
        }

        ~ProxyDataReader()
        {
            this.Dispose(false);
        }

        public BaseSQLCommand Command
        {
            get { return this.dba; }
        }

        #region IDataReader Members

        public void Close()
        {
            this.dbr.Close();
        }

        public int Depth
        {
            get { return this.dbr.Depth; }
        }

        public DataTable GetSchemaTable()
        {
            return this.dbr.GetSchemaTable();
        }

        public bool IsClosed
        {
            get { return this.dbr.IsClosed; }
        }

        public bool NextResult()
        {
            bool temp = this.dbr.NextResult();
            return temp;
        }

        public bool Read()
        {
            return this.dbr.Read();
        }

        public int RecordsAffected
        {
            get { return this.dbr.RecordsAffected; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { return this.dbr.FieldCount; }
        }

        public bool GetBoolean(int i)
        {
            return this.dbr.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return this.dbr.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return this.dbr.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return this.dbr.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return this.dbr.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        IDataReader IDataRecord.GetData(int i)
        {
            throw new NotSupportedException("GetData not supported.");
        }

        public string GetDataTypeName(int i)
        {
            return this.dbr.GetDataTypeName(i);
        }

        public DateTime GetDateTime(int i)
        {
            return this.dbr.GetDateTime(i);
        }

        public decimal GetDecimal(int i)
        {
            return this.dbr.GetDecimal(i);
        }

        public double GetDouble(int i)
        {
            return this.dbr.GetDouble(i);
        }

        public Type GetFieldType(int i)
        {
            return this.dbr.GetFieldType(i);
        }

        public float GetFloat(int i)
        {
            return this.dbr.GetFloat(i);
        }

        public Guid GetGuid(int i)
        {
            return this.dbr.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return this.dbr.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return this.dbr.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return this.dbr.GetInt64(i);
        }

        public string GetName(int i)
        {
            return this.dbr.GetName(i);
        }

        public int GetOrdinal(string name)
        {
            return this.dbr.GetOrdinal(name);
        }

        public string GetString(int i)
        {
            return this.dbr.GetString(i);
        }

        public object GetValue(int i)
        {
            return this.dbr.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            return this.dbr.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return this.dbr.IsDBNull(i);
        }

        public object this[string name]
        {
            get
            {
                return dbr[name];
            }
        }

        public object this[int i]
        {
            get
            {
                return dbr[i];
            }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region Field Reader

        public int GetInt32(string name, int def_val)
        {
            int Result = def_val;

            object value = this[name];
            if (value != DBNull.Value)
            {
                Result = Convert.ToInt32(value);
            }

            return Result;
        }

        public long GetInt64(string name, int def_val)
        {
            long Result = def_val;

            object value = this[name];
            if (value != DBNull.Value)
            {
                Result = Convert.ToInt64(value);
            }

            return Result;
        }

        public string GetString(string name, string def_val = null)
        {
            string Result = def_val;

            object value = this[name];
            if (value != DBNull.Value)
            {
                Result = Convert.ToString(value).Trim();
            }

            return Result;
        }

        public bool GetBoolean(string name, bool def_val)
        {
            bool Result = def_val;

            object value = this[name];
            if (value != DBNull.Value)
            {
                Result = Convert.ToBoolean(value);
            }

            return Result;
        }

        public decimal GetDecimal(string name, decimal def_val)
        {
            decimal Result = def_val;

            object value = this[name];
            if (value != DBNull.Value)
            {
                Result = Convert.ToDecimal(value);
            }

            return Result;
        }

        public DateTime GetDateTime(string name, DateTime def_val)
        {
            DateTime Result = def_val;

            object value = this[name];
            if (value != DBNull.Value)
            {
                Result = Convert.ToDateTime(value);
            }

            return Result;
        }

        public double GetDouble(string name, double def_val)
        {
            double Result = def_val;

            object value = this[name];
            if (value != DBNull.Value)
            {
                Result = Convert.ToDouble(value);
            }

            return Result;
        }

        //
        //
        //
        public void Get(string name, ref int val)
        {
            object value = this[name];
            if (value != DBNull.Value)
            {
                val = Convert.ToInt32(value);
            }
        }

        public void Get(string name, ref long val)
        {
            object value = this[name];
            if (value != DBNull.Value)
            {
                val = Convert.ToInt64(value);
            }
        }

        public void Get(string name, ref string val)
        {
            object value = this[name];
            if (value != DBNull.Value)
            {
                val = Convert.ToString(value).Trim();
            }
        }

        public void Get(string name, ref bool val)
        {
            object value = this[name];
            if (value != DBNull.Value)
            {
                val = Convert.ToBoolean(value);
            }
        }

        public void Get(string name, ref decimal val)
        {
            object value = this[name];
            if (value != DBNull.Value)
            {
                val = Convert.ToDecimal(value);
            }
        }

        public void Get(string name, ref DateTime val)
        {
            object value = this[name];
            if (value != DBNull.Value)
            {
                val = Convert.ToDateTime(value);
            }
        }

        public void Get(string name, ref double val)
        {
            object value = this[name];
            if (value != DBNull.Value)
            {
                val = Convert.ToDouble(value);
            }
        }

        //
        //
        //
        public char GetChar(string name)
        {
            object Result = this[name];

            char c = Convert.ToChar(Result);

            return c;
        }

        public T GetEnum<T>(string name)
        {
            return (T)Enum.ToObject(typeof(T), this[name]);
        }

        #endregion

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.dbr != null)
                {
                    if (!this.IsClosed)
                    {
                        this.Close();
                    }

                    this.dbr.Dispose();
                    dbr = null;
                }

                if (this.dba != null)
                {
                    this.dba.Dispose();
                    this.dba = null;
                }
            }
        }
    }

}

