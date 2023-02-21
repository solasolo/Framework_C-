using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SoulFab.Core.Helper
{
    public class XMLHelper
    {
        static public void GetAttribute(XmlNode node, string name, ref int value)
        {
            XmlAttribute attr = node.Attributes[name];
            if (attr != null)
            {
                string val_str = attr.Value;
                if (!String.IsNullOrEmpty(val_str))
                {
                    value = Int32.Parse(val_str);
                }
            }
        }

        static public void GetAttribute(XmlNode node, string name, ref long value)
        {
            XmlAttribute attr = node.Attributes[name];
            if (attr != null)
            {
                string val_str = attr.Value;
                if (!String.IsNullOrEmpty(val_str))
                {
                    value = Int64.Parse(val_str);
                }
            }
        }

        static public void GetAttribute(XmlNode node, string name, ref decimal value)
        {
            XmlAttribute attr = node.Attributes[name];
            if (attr != null)
            {
                string val_str = attr.Value;
                if (!String.IsNullOrEmpty(val_str))
                {
                    value = Decimal.Parse(val_str);
                }
            }
        }

        static public void GetAttribute(XmlNode node, string name, ref float value)
        {
            XmlAttribute attr = node.Attributes[name];
            if (attr != null)
            {
                string val_str = attr.Value;
                if (!String.IsNullOrEmpty(val_str))
                {
                    value = Single.Parse(val_str);
                }
            }
        }

        static public void GetAttribute(XmlNode node, string name, ref string value)
        {
            XmlAttribute attr = node.Attributes[name];
            if (attr != null)
            {
                value = attr.Value;
            }
        }

        static public void GetAttribute(XmlNode node, string name, ref bool value)
        {
            XmlAttribute attr = node.Attributes[name];
            if (attr != null)
            {
                string v = attr.Value.ToLower();

                if (v == "true")
                {
                    value = true;
                }
                else if (v == "false")
                {
                    value = false;
                }
            }
        }

        static public void GetText(XmlNode node, ref string value)
        {
            value = node.InnerText;
        }
    
        static public void GetText(XmlNode node, ref int value)
        {
            string txt = node.InnerText;
            if (!String.IsNullOrEmpty(txt))
            {
                value = Int32.Parse(txt);
            }
        }
    }

}
