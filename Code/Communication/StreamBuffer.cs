using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoulFab.Core.Communication
{
    public class StreamBuffer
    {
        //
        struct Cursor
        {
            public int ListIndex;
            public int Pos;
        }

        //
        private List<byte[]> Buffers;

        //
        public StreamBuffer()
        {
            Buffers = new List<byte[]>();
        }

        public void Clear()
        {
            Buffers.Clear();
        }

        public static StreamBuffer operator +(StreamBuffer buffer, byte[] data)
        {
            buffer.Append(data);

            return buffer;
        }

        private bool locate(int pos, ref Cursor cur)
        {
            bool bret = false;

            if (Buffers.Count > 0)
            {
                cur.ListIndex = 0;
                cur.Pos = 0;

                if (pos == 0)
                {
                    bret = true;
                }
                else
                {
                    while (cur.ListIndex < Buffers.Count && pos > 0)
                    {
                        int n = Buffers[cur.ListIndex].Length;

                        if (pos >= n)
                        {
                            pos -= n;
                            cur.ListIndex++;
                        }
                        else
                        {
                            cur.Pos = pos;
                            bret = true;

                            break;
                        }
                    }
                }
            }

            return bret;
        }

        public void Append(byte[] data)
        {
            lock (this.Buffers)
            {
                Buffers.Add(data);

            }
        }

        public int GetSize()
        {
            int size = 0;

            List<byte[]>.Enumerator it = Buffers.GetEnumerator();
            while (it.MoveNext())
            {
                size += it.Current.Length;
            }

            return size;
        }

        private bool ReadData(byte[] buffer, int pos, int len)
        {
            bool bret = false;

            Cursor cur = new Cursor();
            bret = locate(pos, ref cur);

            if (bret)
            {
                int c = 0;

                do
                {
                    if (len - c == 0)
                    {
                        break;
                    }

                    int n = Buffers[cur.ListIndex].Length;
                    n -= cur.Pos;
                    if (len - c < n)
                    {
                        n = len - c;
                    }

                    Array.Copy(Buffers[cur.ListIndex], cur.Pos, buffer, c, n);
                    c += n;
                    cur.Pos = 0;
                }
                while (cur.ListIndex < Buffers.Count && c < len);

                if (len != c) bret = false;
            }

            return bret;
        }

        public bool ReadData(byte[] buffer, int len)
        {
            bool bret = false;
            int c = 0;

            if (GetSize() >= len)
            {
                var it = Buffers.GetEnumerator();
                while (it.MoveNext() && c < len)
                {
                    int n = it.Current.Length;
                    if (len - c < n)
                    {
                        n = len - c;
                    }

                    Array.Copy(it.Current, 0, buffer, c, n);

                    c += n;
                }

                bret = true;
            }

            return bret;
        }

        public void PickData(byte[] buffer, int len)
        {
            bool bret = ReadData(buffer, len);

            if (bret)
            {
                Pick(len);
            }
        }

        public void Pick(int len)
        {
            // 删除解析完成的数据

            if (GetSize() >= len)
            {
                int c = 0;
                int n = 0;
                int n_len = 0;

                for (int i = 0; i < Buffers.Count; i++)
                {
                    if (len - c == 0)
                    {
                        break;
                    }

                    n = Buffers[i].Length;
                    n_len = n;

                    if (len - c < n)
                    {
                        n = len - c;
                        n_len -= n;
                        //it->erase(0, n);
                        byte[] n_buffer = new byte[n_len];

                        Array.Copy(Buffers[i], n, n_buffer, 0, n_len);

                        Buffers[i] = n_buffer;

                        break;
                    }
                    else
                    {
                        //it = Buffer.erase(it);
                        lock (Buffers)
                        {
                            Buffers.RemoveAt(i);
                        }
                    }

                    c += n;
                }

            }
        }

        public int Scan(byte ch)
        {
            int ret = -1;

            int pos = 0;
            foreach (var buf in this.Buffers)
            {
                int offset = -1;

                for (int i = 0; i < buf.Length; i++)
                {
                    if (buf[i] == ch)
                    {
                        offset = i;
                        break;
                    }
                }

                if (offset == -1)
                {
                    pos += buf.Length;
                }
                else
                {
                    ret = pos + offset;
                    break;
                }
            }

            return ret;
        }

        public byte ReadByte(int pos)
        {
            byte data = 0;

            byte[] b = new byte[1];

            ReadData(b, pos, 1);
            data = b[0];

            return data;
        }

        public int ReadInt(int pos)
        {
            int data = 0;

            byte[] b = new byte[4];

            ReadData(b, pos, 4);

            data = BitConverter.ToInt32(b, 0);

            return data;
        }

        public int ReadIntReverse(int pos)
        {
            int data = 0;

            byte[] b = new byte[4];

            ReadData(b, pos, 4);
            var rb = b.Reverse().ToArray();

            data = BitConverter.ToInt32(rb, 0);

            return data;
        }

        public short ReadShort(int pos)
        {
            short data = 0;

            byte[] b = new byte[2];

            ReadData(b, pos, 2);

            data = BitConverter.ToInt16(b, 0);

            return data;
        }

        public short ReadShortReverse(int pos)
        {
            short data = 0;

            byte[] b = new byte[2];

            ReadData(b, pos, 2);
            var rb = b.Reverse().ToArray();

            data = BitConverter.ToInt16(rb, 0);

            return data;
        }

        public string ReadString(int pos)
        {
            string data = string.Empty;

            short len = ReadShort(pos);
            byte[] temp = new byte[len];

            if (ReadData(temp, pos + 2, len))
            {
                data = Encoding.ASCII.GetString(temp);
            }

            return data;
        }

        public string ReadString(int pos, int len)
        {
            string data = string.Empty;

            byte[] temp = new byte[len];

            if (ReadData(temp, pos, len))
            {
                data = Encoding.ASCII.GetString(temp);
            }

            return data;
        }

        public void ReadAll(byte[] msg, int n)
        {
            List<byte[]>.Enumerator it = Buffers.GetEnumerator();

            while (it.MoveNext())
            {
                Array.Copy(it.Current, 0, msg, n, it.Current.Length);
                n += it.Current.Length;
            }

            Buffers.Clear();
        }
    }
}
