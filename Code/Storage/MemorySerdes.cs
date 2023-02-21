using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SoulFab.Core.Base;
using SoulFab.Core.Communication;
using SoulFab.Core.Helper;

namespace SoulFab.Core.Storage
{
    public class MemorySerdes
    {
        TelegramScheme Scheme;

        public MemorySerdes(TelegramScheme scheme)
        {
            this.Scheme = scheme;
        }

        public byte[] Serialize(object obj)
        {
            var builder = new BinaryMessageBuilder();
            this.SerializeObjcect(this.Scheme, builder, obj);

            return builder.GetData();
        }

        public void Deserialize(byte[] data, ref object obj)
        {
            var parser = new BinaryMessageParser(data);
            this.DeserializeObject(this.Scheme, parser, ref obj);
        }

        public string ToJSON(object obj)
        {
            StringBuilder builder = new StringBuilder();
            this.ObjectToJson(this.Scheme, builder, obj);

            return builder.ToString();
        }

        private void SerializeObjcect(TelegramScheme scheme, BinaryMessageBuilder builder, object obj)
        {
            foreach (var fd in scheme)
            {
                object value = fd.GetObjectValue(obj);

                if (fd.IsObject)
                {
                    var sub_cheme = new TelegramScheme(fd.PropertyType, fd.SubFieldList);

                    this.SerializeObjcect(sub_cheme, builder, value);
                }
                else
                {
                    this.SerializeItem(fd, builder, value);
                }
            }
        }

        private void DeserializeObject(TelegramScheme scheme, BinaryMessageParser parser, ref object obj)
        {
            foreach (var fd in scheme)
            {
                if (fd.IsObject)
                {
                    object value = fd.GetObjectValue(obj);

                    var sub_cheme = new TelegramScheme(fd.PropertyType, fd.SubFieldList);
                    this.DeserializeObject(sub_cheme, parser, ref value);

                    fd.SetObjectValue(obj, value);
                }
                else
                {
                    this.DeserializeItem(fd, parser, ref obj);
                }
            }
        }

        private void ObjectToJson(TelegramScheme scheme, StringBuilder builder, object obj)
        {
            builder.Append("{");

            bool first = true;
            foreach (var fd in scheme)
            {
                if (first) first = false; else builder.Append(",");

                builder.Append("\"");
                builder.Append(fd.Property);
                builder.Append("\":");

                object value = fd.GetObjectValue(obj);

                if (fd.IsObject)
                {
                    var sub_cheme = new TelegramScheme(fd.PropertyType, fd.SubFieldList);

                    this.ObjectToJson(sub_cheme, builder, value);
                }
                else
                {
                    this.ItemToJson(fd, builder, value);
                }
            }

            builder.Append("}");
        }

        private void DeserializeItem(FieldDef fd, BinaryMessageParser parser, ref object obj)
        {
            object value = null;

            if (fd.Type == CommonType.String)
            {
                int size = fd.Length - 2;
                int len = parser.GetShort();

                var buf = parser.GetBytes(size);
                value = Encoding.ASCII.GetString(buf[0..len]);
            }            
            else
            {
                value = parser.Decode(fd);
            }

            fd.SetObjectValue(obj, value);
        }

        private void SerializeItem(FieldDef fd, BinaryMessageBuilder builder, object obj)
        {
            if (fd.Type == CommonType.String)
            {
                int size = fd.Length - 2;

                string data = obj.ToString();
                if (data.Length > size) throw new Exception("String Exceed Size");

                byte[] buf = new byte[size];

                Encoding.ASCII.GetBytes(data).CopyTo(buf, 0);

                builder.Add((short)data.Length);
                builder.Append(buf);
            }
            else
            {
                builder.Encode(fd, obj);
            }
        }

        private void ItemToJson(FieldDef fd, StringBuilder builder, object obj)
        {
            if (fd.Type == CommonType.String || obj is Enum)
            {
                builder.Append("\"");
                builder.Append(obj.ToString());
                builder.Append("\"");
            }
            else if (fd.Type == CommonType.DataTime)
            {
                builder.Append("\"");
                builder.Append(((DateTime)obj).ToString());
                builder.Append("\"");
            }
            else
            {
                builder.Append(obj);
            }
        }
    }
}
