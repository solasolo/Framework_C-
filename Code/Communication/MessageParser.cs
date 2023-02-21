using SoulFab.Core.Base;
using SoulFab.Core.Helper;
using System;
using System.Text;

namespace SoulFab.Core.Communication
{
    public class BaseMessageParser
    {
        protected byte[] Data;
        protected int Pos;
        protected int Size;

        public BaseMessageParser(byte[] buf)
        {
            Data = buf;
            Pos = 0;

            Size = (buf == null) ? 0 : buf.Length;
        }

        public byte[] GetBytes(int len)
        {
            Check(len);

            byte[] ret = new byte[len];
            Array.Copy(Data, Pos, ret, 0, len);

            Pos += len;

            return ret;
        }

        protected void Check(int len)
        {
            if (Pos + len > Size)
            {
                throw new Exception($"超出数据范围: {Pos} + {len} / {Size}");
            }
        }
    }

    public class BaseBinaryMessageParser : BaseMessageParser
    {
        Encoding Encoder;

        public BaseBinaryMessageParser(byte[] buf)
        : base(buf)
        {
            this.Encoder = Encoding.UTF8;
        }

        public int Remain()
        {
            return Size - Pos;
        }

        public byte GetByte()
        {
            Check(1);

            byte ret;

            ret = Data[Pos];

            Pos += 1;

            return ret;
        }

        public string GetString()
        {
            string str = string.Empty;
            short len = GetLength();

            byte[] bytebuffer = this.GetBytes(len);
            str = this.Encoder.GetString(bytebuffer, 0, len);

            return str;
        }

        public string GetPLCString()
        {
            string str = string.Empty;
            byte size = GetByte();
            byte len = GetByte();

            byte[] bytebuffer = this.GetBytes(size);

            str = this.Encoder.GetString(bytebuffer, 0, len);

            return str;
        }

        public string GetString(int len)
        {
            string str = string.Empty;

            byte[] bytebuffer = this.GetBytes(len);

            str = this.Encoder.GetString(bytebuffer, 0, len);

            return str;
        }


        protected short GetLength()
        {
            Check(2);

            short ret = BitConverter.ToInt16(Data, Pos);

            Pos += 2;

            return ret;
        }
    }

    public class BinaryMessageParser : BaseBinaryMessageParser
    {
        public BinaryMessageParser(byte[] buf)
            : base(buf)
        {
        }

        public short GetShort()
        {
            Check(2);

            short ret = BitConverter.ToInt16(Data, Pos);

            Pos += 2;

            return ret;
        }

        public int GetInt()
        {
            Check(4);

            int ret = BitConverter.ToInt32(Data, Pos);

            Pos += 4;

            return ret;
        }

        public long GetLong()
        {
            Check(8);

            long ret = BitConverter.ToInt64(Data, Pos);

            Pos += 8;

            return ret;
        }

        public float GetFloat()
        {
            Check(4);

            float ret = BitConverter.ToSingle(Data, Pos);

            Pos += 4;

            return ret;
        }

        public double GetDouble()
        {
            Check(8);

            double ret = BitConverter.ToDouble(Data, Pos);

            Pos += 8;

            return ret;
        }
    }

    public class InvertBinaryMessageParser : BaseBinaryMessageParser
    {
        public InvertBinaryMessageParser(byte[] buf)
            : base(buf)
        {
        }       

        public short GetShort()
        {
            short ret;

            byte[] Temp = GetReverse(2);
            ret = BitConverter.ToInt16(Temp, 0);

            Pos += 2;

            return ret;
        }

        public int GetInt()
        {
            int ret;

            byte[] Temp = GetReverse(4);
            ret = BitConverter.ToInt32(Temp, 0);

            Pos += 4;

            return ret;
        }

        public long GetLong()
        {
            long ret;

            byte[] Temp = GetReverse(8);
            ret = BitConverter.ToInt64(Temp, 0);

            Pos += 8;

            return ret;
        }

        public float GetFloat()
        {
            float ret;

            byte[] Temp = GetReverse(4);
            ret = BitConverter.ToSingle(Temp, 0);

            Pos += 4;

            return ret;
        }

        private byte[] GetReverse(int len)
        {
            Check(len);

            byte[] ret = new byte[len];
            Array.Copy(Data, Pos, ret, 0, len);
            Array.Reverse(ret);

            return ret;
        }
    }

    public class TextMessageParser
    {
        private byte[] Data;
        private Encoding Encoder;
        int Pos;
        int Size;

        public TextMessageParser(byte[] data)
        {
            Data = data;
            Pos = 0;
            this.Encoder = Encoding.UTF8;

            Size = (data == null) ? 0 : data.Length;
        }

        public string GetString(int len)
        {
            string str = string.Empty;
            Check(len);

            var part = this.Data[this.Pos..(this.Pos + len)];
            str = Encoder.GetString(part);

            Pos += len;

            return str.Trim();
        }

        public int GetInt(int len)
        {
            string str = this.GetString(len);

            return Int32.Parse(str);
        }

        private void Check(int len)
        {
            if (Pos + len > Size)
            {
                throw new Exception($"超出数据范围: {Pos} + {len} / {Size}");
            }
        }
    }
}
