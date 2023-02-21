using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Communication
{
    public class BaseMessageBuilder : IMessageBuilder
    {
        protected const int BUFFER_BASE_SIZE = 4096;

        protected byte[] Buffer;
        protected int Position;
        protected int Capacity;

        public BaseMessageBuilder()
        {
            Buffer = null;
            Position = 0;
            Capacity = 0;
        }

        public int Size
        {
            get
            {
                return this.Position;
            }
        }

        public void Clean()
        {
            this.Position = 0;
        }

        public void ReadAll(byte[] msg, int n = 0)
        {
            Array.Copy(this.Buffer, 0, msg, n, this.Size);
        }

        public byte[] ReadAll()
        {
            byte[] ret = new byte[this.Size];

            this.ReadAll(ret);

            return ret;
        }

        public void Append(byte[] part)
        {
            if (Buffer == null)
            {
                this.Create(BUFFER_BASE_SIZE);
                Position = 0;
            }

            int part_len = part.Length;
            if (Position + part_len > this.Capacity)
            {
                byte[] old = this.Buffer;
                this.Create(Position + part_len);

                old.CopyTo(this.Buffer, 0);
            }

            part.CopyTo(this.Buffer, this.Position);
            this.Position += part_len;
        }

        private void Create(int size)
        {
            int new_size = BUFFER_BASE_SIZE;
            if (size > BUFFER_BASE_SIZE)
            {
                new_size = (size / BUFFER_BASE_SIZE + 1) * BUFFER_BASE_SIZE;
            }

            Buffer = new byte[new_size];
            Capacity = new_size;
        }
    }

    public class BaseBinaryMessageBuilder : BaseMessageBuilder
    {
        public BaseBinaryMessageBuilder()
        : base()
        {
        }

        public byte[] GetData()
        {
            byte[] ret = new byte[this.Size];
            Array.Copy(this.Buffer, 0, ret, 0, this.Size);

            return ret;
        }

        public void Add(byte data)
        {
            this.Append(new byte[] { data });
        }

        public void Add(string data, int size)
        {
            if (data.Length > size) throw new Exception("String Exceed Size");

            byte[] msg_buf = new byte[2 + size];
            msg_buf[0] = (byte)size;
            msg_buf[1] = (byte)data.Length;

            Encoding.ASCII.GetBytes(data).CopyTo(msg_buf, 2);

            this.Append(msg_buf);
        }

        public void Add(string data)
        {
            byte[] len = BitConverter.GetBytes((short)data.Length);

            this.Append(len);
            this.Append(Encoding.UTF8.GetBytes(data));
        }
    }

    public class BinaryMessageBuilder : BaseBinaryMessageBuilder, IMessageBuilder
    {
        public BinaryMessageBuilder()
            : base()
        {
        }

        public void Add(short data)
        {
            this.Append(BitConverter.GetBytes(data));
        }

        public void Add(int data)
        {
            this.Append(BitConverter.GetBytes(data));
        }

        public void Add(long data)
        {
            this.Append(BitConverter.GetBytes(data));
        }

        public void Add(float data)
        {
            this.Append(BitConverter.GetBytes(data));
        }
        public void Add(double data)
        {
            this.Append(BitConverter.GetBytes(data));
        }
    }

    public class InvertBinaryMessageBuilder : BaseBinaryMessageBuilder, IMessageBuilder
    {
        public void Add(short data)
        {
            byte[] buf = BitConverter.GetBytes(data);
            Array.Reverse(buf);

            this.Append(buf);
        }

        public void Add(int data)
        {
            byte[] buf = BitConverter.GetBytes(data);
            Array.Reverse(buf);

            this.Append(buf);
        }

        public void Add(long data)
        {
            byte[] buf = BitConverter.GetBytes(data);
            Array.Reverse(buf);

            this.Append(buf);
        }

        public void Add(float data)
        {
            byte[] buf = BitConverter.GetBytes(data);
            Array.Reverse(buf);

            this.Append(buf);
        }

    }

    public class TextMessageBuilder : BaseMessageBuilder
    {
        public TextMessageBuilder()
            : base()
        { 
        }

        public void Add(string str, int len)
        {
            if (str == null) str = "";

            var paded_str = this.Pad(str, len);
            this.Append(paded_str);
        }

        public void Add(int value, int len)
        {
            string str = value.ToString("D" + len.ToString());

            this.Add(str, len);
        }

        public string Pad(string str, int len)
        {
            string ret = str;

            if (str.Length < len)
            {
                ret = str.PadRight(len, ' ');
            }
            else if (str.Length > len)
            {
                throw new Exception($"超出数据范围 {len} / {str.Length} {str}");
            }

            return ret;
        }

        public void Append(string str)
        {
            var bs = Encoding.UTF8.GetBytes(str);
            this.Append(bs);
        }
    }

    public class TextBuilder
    {
        private StringBuilder Builder;

        public TextBuilder()
        {
            this.Builder = new StringBuilder();
        }

        public void Add(string str, int len)
        {
            if (str == null) str = "";

            var paded_str = this.Pad(str, len);
            this.Builder.Append(paded_str);
        }

        public void Add(int value, int len)
        {
            string str = value.ToString("D" + len.ToString());

            this.Add(str, len);
        }

        public string ReadAll()
        {
            return this.Builder.ToString();
        }

        public string Pad(string str, int len)
        {
            string ret = str;

            if (str.Length < len)
            {
                ret = str.PadRight(len, ' ');
            }
            else if (str.Length > len)
            {
                throw new Exception($"超出数据范围: {len} / {str.Length}");
            }

            return ret;
        }
    }
}
