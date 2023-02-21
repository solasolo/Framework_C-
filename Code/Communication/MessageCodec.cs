using SoulFab.Core.Base;
using SoulFab.Core.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Communication
{
    public abstract class BaseMessageCode<T, B, P> : IMessageCodec<T> where B : IMessageBuilder
    {
        protected MessageScheme Scheme;

        public object Decode(T cmd, byte[] buf)
        {
            object ret = null;

            var scheme = this.Scheme[cmd.ToString()];

            if (scheme != null)
            {
                var parser = (P)Activator.CreateInstance(typeof(P), buf);

                ret = this.DecodeObject(scheme, parser);
            }

            return ret;
        }

        public byte[] Encode(T cmd, object obj)
        {
            byte[] ret = null;

            var scheme = this.Scheme[cmd.ToString()];

            if (scheme != null)
            {
                var builder = (B)Activator.CreateInstance(typeof(B));

                this.EncodeObjcect(scheme, builder, obj);

                ret = builder.ReadAll();
            }

            return ret;
        }

        public void EncodeObjcect(TelegramScheme scheme, B builder, object obj)
        {
            foreach (var fd in scheme)
            {
                object value = fd.GetObjectValue(obj);

                if (fd.IsObject)
                {
                    var sub_cheme = new TelegramScheme(fd.PropertyType, fd.SubFieldList);

                    this.EncodeObjcect(sub_cheme, builder, value);
                }
                else if (fd.IsTable)
                {
                    var sub_cheme = new TelegramScheme(fd.PropertyType, fd.SubFieldList);

                    foreach (object sub_value in value as IEnumerable)
                    {
                        this.EncodeObjcect(sub_cheme, builder, sub_value);
                    }
                }
                else
                {
                    try
                    {
                        this.EncodeItem(fd, builder, value);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{fd.Property} Buuild Error:", ex);
                    }
                }
            }
        }

        private object DecodeObject(TelegramScheme scheme, P parser)
        {
            object ret = ReflectHelper.CreateObject(scheme.ObjectType);

            foreach (var fd in scheme)
            {
                object value;

                if (fd.IsObject)
                {
                    var sub_cheme = new TelegramScheme(fd.PropertyType, fd.SubFieldList);
                    value = this.DecodeObject(sub_cheme, parser);
                }
                else if (fd.IsTable)
                {
                    var sub_cheme = new TelegramScheme(fd.PropertyType, fd.SubFieldList);

                    var gen_type = ReflectHelper.CreateType(typeof(List<>), fd.PropertyType);
                    value = ReflectHelper.CreateObject(gen_type);

                    var add_method = gen_type.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

                    int len = fd.Length;
                    if (len == 0 && !String.IsNullOrEmpty(fd.LengthRef)) len = (int)ReflectHelper.GetValue(ret, fd.LengthRef);
                    for (int i = 0; i < len; i++)
                    {
                        var obj = this.DecodeObject(sub_cheme, parser);

                        add_method.Invoke(value, new object[] { obj });
                    }
                }
                else
                {
                    value = this.DecodeItem(fd, parser);
                }

                fd.SetObjectValue(ret, value);
            }

            return ret;
        }

        abstract protected object DecodeItem(FieldDef fd, P parser);
        abstract protected void EncodeItem(FieldDef fd, B builder, object value);
    }

    public class BinaryMessageCodec<T> : BaseMessageCode<T, BinaryMessageBuilder, BinaryMessageParser>
    {
        public BinaryMessageCodec(MessageScheme scheme)
        {
            this.Scheme = scheme;
        }

        protected override object DecodeItem(FieldDef fd, BinaryMessageParser parser)
        {
            return parser.Decode(fd);
        }

        protected override void EncodeItem(FieldDef fd, BinaryMessageBuilder builder, object value)
        {
            builder.Encode(fd, value);
        }
    }

    public static class BinaryCodecExtension
    {
        public static void Encode(this BinaryMessageBuilder builder, FieldDef fd, object value)
        {
            switch (fd.Type)
            {
                case CommonType.String:
                    builder.Add((string)value);
                    break;

                case CommonType.Byte:
                    builder.Add((byte)value);
                    break;

                case CommonType.Word:
                    builder.Add((short)value);
                    break;

                case CommonType.Integer:
                    builder.Add((int)value);
                    break;

                case CommonType.LongInt:
                    builder.Add((long)value);
                    break;

                case CommonType.Float:
                    builder.Add(Convert.ToSingle(value));
                    break;

                case CommonType.Double:
                    builder.Add(Convert.ToDouble(value));
                    break;

                case CommonType.DataTime:
                    builder.Add(((DateTime)value).Ticks);
                    break;

            }
        }

        static public object Decode(this BinaryMessageParser parser, FieldDef fd)
        {
            object ret = null;

            switch (fd.Type)
            {
                case CommonType.String:
                    ret = parser.GetString();
                    break;

                case CommonType.Byte:
                    ret = parser.GetByte();
                    break;

                case CommonType.Word:
                    ret = parser.GetShort();
                    break;

                case CommonType.Integer:
                    ret = parser.GetInt();
                    break;

                case CommonType.LongInt:
                    ret = parser.GetLong();
                    break;

                case CommonType.Float:
                    ret = parser.GetFloat();
                    break;

                case CommonType.Double:
                    ret = parser.GetDouble();
                    break;

                case CommonType.DataTime:
                    ret = new DateTime(parser.GetLong());
                    break;
            }

            return ret;
        }
    }

    public class TextMessageCodec<T> : BaseMessageCode<T, TextMessageBuilder, TextMessageParser>
    {
        public TextMessageCodec(MessageScheme scheme)
        {
            this.Scheme = scheme;
        }

        protected override object DecodeItem(FieldDef fd, TextMessageParser parser)
        {
            object ret = null;

            switch (fd.Type)
            {
                case CommonType.String:
                    ret = parser.GetString(fd.Length);
                    break;

                case CommonType.Integer:
                    ret = parser.GetInt(fd.Length);
                    break;
            }

            return ret;
        }

        protected override void EncodeItem(FieldDef fd, TextMessageBuilder builder, object value)
        {
            switch (fd.Type)
            {
                case CommonType.String:
                    builder.Add((string)value, fd.Length);
                    break;

                case CommonType.Integer:
                    builder.Add((int)value, fd.Length);
                    break;
            }
        }
    }
}
