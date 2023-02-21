using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SoulFab.Core.Config
{
    public class XMLConfig : BaseConfig
    {
        private XmlDocument Doc;
        private XmlNode Root;

        public XMLConfig(string path)
        {
            this.Load(path);
        }

        public XMLConfig(XmlNode node)
        {
            this.Doc = null;
            this.Root = node;
        }

        public override IConfig GetConfig(string key)
        {
            IConfig ret = null;

            var path = key.Replace('.', '/');

            var node = this.Root.SelectSingleNode(path);
            if (node != null)
            {
                ret = new XMLConfig(node);
            }

            return ret;
        }


        protected void Load(string path)
        {
            this.Doc = new XmlDocument();
            this.Doc.Load(path);
            
            this.Root = this.Doc.DocumentElement;
        }

        protected override string GetItem(string key)
        {
            string ret = null;

            var parts = key.Split('.');

            int part_len = parts.Length;
            if (part_len == 1)
            {
                string path = parts[0];

                ret = this.GetAttrText(this.Root, path);
                if (ret == null)
                {
                    ret = this.GetNodeText(path);
                }
            }
            else
            {
                var path = String.Join('/', parts);

                ret = this.GetNodeText(path);
                if (ret == null)
                {
                    path = String.Join('/', parts, 0, part_len - 1);
                    string name = parts[part_len - 1];

                    var node = this.Root.SelectSingleNode(path);
                    if (node != null)
                    {
                        ret = this.GetAttrText(node, name);
                    }
                }
            }

            return ret;
        }

        private string GetNodeText(string path)
        {
            string ret = null;

            var node = this.Root.SelectSingleNode(path);
            if (node != null)
            {
                ret = node.InnerText;
            }

            return ret;
        }

        private string GetAttrText(XmlNode node, string name)
        {
            string ret = null;

            var attr = node.Attributes[name];

            if (attr != null)
            {
                ret = attr.Value;
            }

            return ret;
        }
    }
}
