using System;
using System.Text;

namespace SoulFab.Core.Communication
{
    /// <summary>
    /// 
    /// </summary>
    public class SimpleFrameCodec : IFrameCodec<short>
    {
        const byte STX = 0x02;
        const byte ETX = 0x03;

        const int CMD_SIZE = 2;
        const int LEN_SIZE = 4;
        const int HEAD_SIZE = CMD_SIZE + LEN_SIZE;

        static readonly byte[] HEARTBEAT_MSG = { STX, 0, 0, 0, 0, 0, 0, ETX };

        public bool Decode(StreamBuffer buf, ref MessageEnity<short> msg)
        {
            bool result = false;
            bool remain = false;

            do
            {
                int pos = buf.Scan(STX);
                if (pos > 0) buf.Pick(pos);

                int bufSize = buf.GetSize();
                if (bufSize >= HEAD_SIZE + 2)
                {
                    if (buf.ReadByte(0) == STX)  // Meet Head
                    {
                        int telSize = buf.ReadInt(1);   // 4 bytes Len
                        if (telSize >= HEAD_SIZE && bufSize >= telSize + 2)
                        {
                            if (buf.ReadByte(telSize + 1) == ETX)
                            {
                                short cmd = buf.ReadShort(LEN_SIZE + 1);  // 2 bytes Cmd
                                msg.Code = cmd;

                                buf.Pick(HEAD_SIZE + 1);        // 删除cmd及之前的字节 1 + 2 + 4


                                if (telSize > HEAD_SIZE)
                                {
                                    msg.Data = new byte[telSize - HEAD_SIZE];

                                    buf.PickData(msg.Data, telSize - HEAD_SIZE);
                                }
                                else
                                {
                                    msg.Data = null;
                                }

                                buf.Pick(1); // Remove ETX
                                result = true;
                            }
                            else
                            {
                                buf.Pick(1); // Remove STX
                                remain = true;
                            }
                        }
                    }
                }
            }
            while (!result && remain);

            return result;
        }

        public byte[] Encode(short cmd, byte[] data)
        {
            int size = HEAD_SIZE + 2 + data.Length;
            byte[] msg = new byte[size];

            msg[0] = (byte)STX;

            byte[] data_Len = BitConverter.GetBytes(size - 2);
            msg[1] = data_Len[0];
            msg[2] = data_Len[1];
            msg[3] = data_Len[2];
            msg[4] = data_Len[3];

            byte[] TelID = BitConverter.GetBytes(cmd);
            msg[LEN_SIZE + 1] = TelID[0];
            msg[LEN_SIZE + 2] = TelID[1];

            msg[size - 1] = (byte)ETX;

            data.CopyTo(msg, HEAD_SIZE + 1);            
            msg[size - 1] = ETX;

            return msg;
        }

        public byte[] EncodeHeartbeat()
        {
            return HEARTBEAT_MSG;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SimpleMessageCodec : IMessageCodec<short>
    {
        const char STX = '\x02';
        const char ETX = '\x03';

        private StreamBuffer Buffer;

        public SimpleMessageCodec()
        {
            Buffer = new StreamBuffer();
        }

        public object Decode(short cmd, byte[] buf)
        {
            throw new NotImplementedException();
        }

        public byte[] Encode(short cmd, object obj)
        {
            throw new NotImplementedException();
        }
    }
}
