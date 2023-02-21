using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using SoulFab.Core.Base;
using SoulFab.Core.Helper;

namespace SoulFab.Core.Data
{
    public class ORMTableAttribute : Attribute
    {
        public readonly string TableName;

        public ORMTableAttribute(string name)
        {
            this.TableName = name;
        }
    }

    public class ORMFieldAttribute : Attribute
    {
        public readonly string FieldName;

        public ORMFieldAttribute(string name)
        {
            this.FieldName = name;
        }
    }

    public class DbScheme
    {
        private IList<TableScheme> Tables;

        private IDictionary<Type, TableScheme> TypeMap;
        private IDictionary<string, TableScheme> TableMap;

        public DbScheme()
        {
            this.Tables = new List<TableScheme>();

            this.TypeMap = new ConcurrentDictionary<Type, TableScheme>();
            this.TableMap = new Dictionary<string, TableScheme>();
        }

        public void Add(TableScheme table_scheme)
        {
            this.Tables.Add(table_scheme);
            this.TableMap[table_scheme.TableName] = table_scheme;
        }

        public TableScheme this[Type type]
        {
            get
            {
                TableScheme ret = null;

                if (this.TypeMap.ContainsKey(type))
                {
                    ret = this.TypeMap[type];
                }
                else
                {
                    ret = this.Regist(type);
                }

                return ret;
            }
        }

        public TableScheme this[string name]
        {
            get
            {
                TableScheme ret = null;

                if (this.TableMap.ContainsKey(name))
                {
                    ret = this.TableMap[name];
                }

                return ret;
            }
        }

        public TableScheme Regist(Type type, string table_name)
        {
            TableScheme ret = this[table_name];

            if (ret != null)
            {
                this.TypeMap[type] = ret;
            }

            return ret;
        }

        private TableScheme Regist(Type type)
        {
            TableScheme ret = null;

            var attribute = type.GetCustomAttribute<ORMTableAttribute>();
            if (attribute != null)
            {
                string table_name = attribute.TableName;

                ret = this.Regist(type, table_name);
            }

            return ret;
        }
    }

    public class TableScheme : IEnumerable<FieldDef>
    {
        private string _TableName;
        private bool _HasAuto;
        private IList<FieldDef> _KeyFields;
        private IList<FieldDef> FieldList;
        private IDictionary<string, FieldDef> PropertyMap;

        public TableScheme(string name)
        {
            this._TableName = name;
            this._HasAuto = false;

            this.FieldList = new List<FieldDef>();
            this._KeyFields = new List<FieldDef>();
            this.PropertyMap = new Dictionary<string, FieldDef>();
        }

        public void Add(FieldDef field)
        {
            FieldList.Add(field);
            this.PropertyMap[field.Property] = field;

            if (field.IsKey) this._KeyFields.Add(field);

            if (field.IsAuto) this.SetAuto();
        }

        public FieldDef GetField(string name)
        {
            FieldDef Result = null;

            foreach (FieldDef fd in FieldList)
            {
                if (fd.Name.ToUpper() == name.ToUpper())
                {
                    Result = fd;
                    break;
                }
            }

            return Result;
        }

        public string TableName { get { return this._TableName; } }
        public IList<FieldDef> KeyFields { get { return this._KeyFields; } }
        public IList<FieldDef> Fields { get { return this.FieldList; } }
        public bool HasAuto { get { return this._HasAuto; } }

        public FieldDef ByProperty(string name)
        {
            FieldDef ret = null;

            if(this.PropertyMap.ContainsKey(name))
            {
                ret = this.PropertyMap[name];
            }

            return ret;
        }

        internal void SetAuto()
        {
            this._HasAuto = true;
        }

        public IEnumerator<FieldDef> GetEnumerator()
        {
            return this.Fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Fields.GetEnumerator();
        }
    }

    public class SchemeConfigure
    {
        public void Load(DbScheme scheme, string path)
        {
            XmlDocument Doc = new XmlDocument();
            Doc.Load(path);

            XmlNodeList TableNodes = Doc.SelectNodes("/Dictionary/Table");
            foreach (XmlNode t_node in TableNodes)
            {
                string TableName = t_node.Attributes["Name"].Value;
                string ObjectType = t_node.Attributes["Class"].Value;

                TableScheme td = new TableScheme(TableName);
                scheme.Add(td);

                XmlNodeList FieldNodes = t_node.SelectNodes("Field");
                foreach (XmlNode f_node in FieldNodes)
                {
                    //bool Key = false;

                    string FieldName = f_node.Attributes["Name"].Value;
                    string FieldType = f_node.Attributes["Type"].Value;
                    bool IsKey = false;
                    int Len = 0;
                    XMLHelper.GetAttribute(f_node, "Length", ref Len);

                    if (f_node.Attributes["Key"] != null)
                    {
                        IsKey = (Convert.ToBoolean(f_node.Attributes["Key"].Value));
                    }

                    string ObjectProp = FieldName;
                    XmlAttribute PropertyAttribute = f_node.Attributes["Property"];
                    if (PropertyAttribute != null)
                    {
                        ObjectProp = PropertyAttribute.Value;
                    }

                    FieldDef fd = new FieldDef(FieldName, FieldType, ObjectProp, Len, 0, IsKey);
                    td.Add(fd);
                }
            }
        }
    }

}
