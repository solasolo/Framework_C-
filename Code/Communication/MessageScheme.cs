using SoulFab.Core.Base;
using SoulFab.Core.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace SoulFab.Core.Communication
{
    public class TelegramScheme : IEnumerable<FieldDef>
    {
        int ByteCount;

        public Type ObjectType;
        private IList<FieldDef> FieldList;

        public TelegramScheme(string type)
        {
            this.ByteCount = 0;

            this.ObjectType = null;
            this.FieldList = new List<FieldDef>();

            if (!String.IsNullOrEmpty(type))
            {
                this.ObjectType = ReflectHelper.CreateType(type);
            }
        }
        public int TotalSize { get { return this.ByteCount; } }

        public TelegramScheme(Type type, IList<FieldDef> fd_list)
        {
            this.ObjectType = type;
            this.FieldList = fd_list;
        }

        public void Add(FieldDef def)
        {
            this.FieldList.Add(def);
            this.ByteCount += def.Length;
        }

        public void Set(IList<FieldDef> lst, int size)
        {
            this.FieldList = lst;
            this.ByteCount = size;
        }

        public IEnumerator<FieldDef> GetEnumerator()
        {
            return this.FieldList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.FieldList.GetEnumerator();
        }
    }

    public class MessageScheme
    {
        private IDictionary<string, TelegramScheme> TelegramMap;

        public MessageScheme()
        {
            this.TelegramMap = new Dictionary<string, TelegramScheme>();
        }

        public void Load(string file)
        {
            XmlDocument Doc = new XmlDocument();
            Doc.Load(file);

            XmlNodeList TableNodes = Doc.SelectNodes("/Teleprams/Telepram");
            foreach (XmlNode t_node in TableNodes)
            {
                string name = t_node.Attributes["Name"].Value;

                var ts = LoadTelegram(t_node);

                this.TelegramMap.Add(name, ts);
            }
        }

        static public TelegramScheme LoadTelegram(string file)
        {
            XmlDocument Doc = new XmlDocument();
            Doc.Load(file);

            var ret = LoadTelegram(Doc.DocumentElement);

            return ret;
        }

        public TelegramScheme this[string name]
        {
            get
            {
                TelegramScheme ret = null;

                if (this.TelegramMap.ContainsKey(name))
                {
                    ret = this.TelegramMap[name];
                }

                return ret;
            }
        }

        static private TelegramScheme LoadTelegram(XmlNode node)
        {
            int count = 0;

            string ObjectType = null;
            XMLHelper.GetAttribute(node, "Class", ref ObjectType);

            TelegramScheme ret = new TelegramScheme(ObjectType);

            XmlNodeList FieldNodes = node.SelectNodes("Field");
            var list = LoadNodes(FieldNodes, ref count);
            ret.Set(list, count);

            return ret;
        }

        static private IList<FieldDef> LoadNodes(XmlNodeList nodes, ref int count)
        {
            IList<FieldDef> ret = new List<FieldDef>();

            foreach (XmlNode f_node in nodes)
            {
                //bool Key = false;

                string FieldName = f_node.Attributes["Name"].Value;
                string FieldType = f_node.Attributes["Type"].Value;

                string ObjectProp = FieldName;
                XmlAttribute PropertyAttribute = f_node.Attributes["Property"];
                if (PropertyAttribute != null)
                {
                    ObjectProp = PropertyAttribute.Value;
                }

                int Len = 0;
                int Prec = 0;
                FieldDef fd;

                if (FieldType == "OBJECT")
                {
                    string ObjectType = null;
                    XMLHelper.GetAttribute(f_node, "Class", ref ObjectType);

                    var list = LoadNodes(f_node.ChildNodes, ref count);

                    fd = new FieldDef(FieldName, FieldType, ObjectProp, ObjectType, "0", list);
                }
                else if (FieldType == "LIST" || FieldType == "TABLE")
                {
                    string ObjectType = null;

                    string LenStr = f_node.Attributes["Length"].Value;
                    XMLHelper.GetAttribute(f_node, "Class", ref ObjectType);

                    var list = LoadNodes(f_node.ChildNodes, ref count);

                    fd = new FieldDef(FieldName, FieldType, ObjectProp, ObjectType, LenStr, list);
                }
                else
                {
                    XMLHelper.GetAttribute(f_node, "Length", ref Len);
                    XMLHelper.GetAttribute(f_node, "Precision", ref Prec);
                    fd = new FieldDef(FieldName, FieldType, ObjectProp, Len, Prec);
                }

                ret.Add(fd);
                count += fd.Length;
            }

            return ret;
        }
    }
}
