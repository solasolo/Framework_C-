using SoulFab.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SoulFab.Core.Base
{
    public enum CommonType
    {
        None,

        Guid,
        String,
        DataTime,
        Binary,
        Numeric,
        Boolean,
        Byte,
        Word,
        Integer,
        LongInt,
        Float,
        Double,
        Single,

        Blob,
        JSON,
    }


    public class FieldDef
    {
        static private IDictionary<string, CommonType> TypeMap = new Dictionary<string, CommonType>()
        {
            { "guid", CommonType.Guid },
            { "intid", CommonType.Integer },
            { "longid", CommonType.LongInt },
            { "string", CommonType.String },
            { "byte", CommonType.Byte },
            { "short", CommonType.Word },
            { "int", CommonType.Integer },
            { "long", CommonType.LongInt },
            { "money", CommonType.Numeric },
            { "numeric", CommonType.Numeric },
            { "float", CommonType.Float },
            { "bool", CommonType.Boolean },
            { "binary", CommonType.Binary },
            { "datetime", CommonType.DataTime },
            { "date", CommonType.DataTime },
            { "time", CommonType.DataTime },
            { "blob", CommonType.Blob },
            { "json", CommonType.JSON },

            { "TABLE", CommonType.None },
            { "LIST", CommonType.None },
            { "OBJECT", CommonType.None },
            { "ARRAY", CommonType.None },
        };

        static private IDictionary<string, int> LengthMap = new Dictionary<string, int>()
        {
            { "guid", 16 },
            { "intid", 4 },
            { "longid", 8 },
            { "string", 0 },
            { "byte", 1 },
            { "short", 2 },
            { "int", 4 },
            { "long", 8 },
            { "money", 8 },
            { "numeric", 8 },
            { "float", 4 },
            { "bool", 1 },
            { "binary", 0 },
            { "datetime", 8 },
            { "date", 8 },
            { "time", 8 },
            { "blob", 0 },
            { "json", 0 },

            { "TABLE", 0 },
            { "LIST", 0 },
            { "OBJECT", 0 },
            { "ARRAY", 0 },
        };

        public class FieldAccessor
        {
            public Type Type { get; set; }
            public Action<object, object> Set { get; set; }
            public Func<object, object> Get { get; set; }
        }

        private Dictionary<Type, FieldAccessor> PropertyObjectCache;


        public readonly IList<FieldDef> SubFieldList;
        private bool _IsAuto;

        public readonly bool IsTable;
        public readonly bool IsObject;
        public readonly string Name;
        public readonly string Property;
        public readonly Type PropertyType;
        public readonly CommonType Type;
        public readonly int Length;
        public readonly int Precision;
        public readonly string LengthRef;
        public readonly bool IsKey;

        public FieldDef(string name, string type, string prop, string prop_type, string len, IList<FieldDef> fd_list)
            : this(name, type, prop, 0)
        {
            this.PropertyType = ReflectHelper.CreateType(prop_type);
            this.SubFieldList = fd_list;

            if (len.IsInteger())
            {
                this.Length = Int32.Parse(len);
                this.LengthRef = null;
            }
            else
            {
                this.Length = 0;
                this.LengthRef = len;
            }
        }

        public FieldDef(string name, string type, string prop, int len, int prec = 0, bool key = false)
        {
            this.PropertyObjectCache = new Dictionary<Type, FieldAccessor>();

            this.SubFieldList = null;
            this.PropertyType = null;

            this.Name = name;
            this.Property = prop;
            this.Precision = prec;
            this.Length = len > 0 ? len : LengthMap[type];

            CommonType _type;
            bool _key = key;

            if (FieldDef.TypeMap.ContainsKey(type))
            {
                _type = FieldDef.TypeMap[type];
            }
            else
            {
                throw new Exception(String.Format("不可识别的类型: {0}, 属性: {1}.{2}", type, name, prop));
            }

            switch (type)
            {
                case "intid":
                case "longid":
                    this.SetAuto();
                    _key = true;
                    break;

                case "LIST":
                case "TABLE":
                    IsTable = true;
                    this.SubFieldList = new List<FieldDef>();
                    break;

                case "OBJECT":
                    IsObject = true;
                    this.SubFieldList = new List<FieldDef>();
                    break;
            }

            Type = _type;
            IsKey = _key;

            //if (IsKey) this.Owner.KeyFields.Add(this);
        }

        public FieldAccessor GetAccessor(Type type)
        {
            FieldAccessor ret = null;

            if (this.PropertyObjectCache.ContainsKey(type))
            {
                ret = this.PropertyObjectCache[type];
            }
            else
            {
                PropertyInfo PublicProp = ReflectHelper.GetProperty(type, this.Property);
                if (PublicProp != null)
                {
                    var getter = PublicProp.GetGetMethod();
                    var setter = PublicProp.GetSetMethod();

                    ret = new FieldAccessor()
                    {
                        Type = PublicProp.PropertyType,
                        Set = (object obj, object value) => { if(PublicProp.CanWrite) PublicProp.SetValue(obj, value, null); },
                        Get = (object obj) => { return PublicProp.GetValue(obj, null); }
                    };
                }
                else
                {
                    FieldInfo PublicField = ReflectHelper.GetField(type, this.Property);
                    if (PublicField != null)
                    {
                        ret = new FieldAccessor()
                        {
                            Type = PublicField.FieldType,
                            Set = (object obj, object value) => { PublicField.SetValue(obj, value); },
                            Get = (object obj) => { return PublicField.GetValue(obj); }
                        };
                    }
                }

                this.PropertyObjectCache[type] = ret;
            }

            return ret;
        }

        public bool GetObjectValue(object obj, ref object value)
        {
            bool ret = true;

            var accessor = this.GetAccessor(obj.GetType());

            if (accessor != null)
            {
                var getter = accessor.Get;
                if (getter != null)
                {
                    value = getter(obj);
                }
                else
                {
                    throw new Exception("No Property to Read:" + this.Property);
                }
            }
            else
            {
                ret = false;
            }

            return ret;
        }

        public object GetObjectValue(object obj)
        {
            object ret = null;

            var accessor = this.GetAccessor(obj.GetType());

            if (accessor != null)
            {
                var getter = accessor.Get;
                if (getter != null)
                {
                    ret = getter(obj);
                }
            }

            return ret;
        }

        public void SetObjectValue(object obj, object value)
        {
            var accessor = this.GetAccessor(obj.GetType());

            if (accessor != null)
            {
                var setter = accessor.Set;
                var type = accessor.Type;

                if (setter != null)
                {
                    if (this.Type == CommonType.JSON)
                    {
                        JsonSerializerOptions options = new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = null,
                            PropertyNameCaseInsensitive = true,

                        };

                        var json_obj = JsonSerializer.Deserialize((String)value, type, options);

                        setter(obj, json_obj);

                    }
                    else if (type == typeof(Single))
                    {
                        setter(obj, Convert.ToSingle(value));
                    }
                    else if (type == typeof(Int32))
                    {
                        setter(obj, Convert.ToInt32(value));
                    }
                    else
                    {
                        setter(obj, value);
                    }
                }
            }
        }

        public bool IsAuto
        {
            get
            {
                return this._IsAuto;
            }
        }

        internal void SetAuto()
        {
            this._IsAuto = true;
            // this.Owner.SetAuto();
        }
    }

}
