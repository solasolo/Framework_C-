using SoulFab.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoulFab.Core.Communication
{
    public abstract class BaseFrameChannel<T> : BaseChannel, IFrameChannel<T>
    {
        private StreamBuffer IncommingBuffer;
        private Thread ReceiveThread;

        protected bool _IsOnline;
        protected bool IsRunning;
        protected DateTime LastSendTime;
        protected int ReceiveTimeer;
        protected IFrameCodec<T> FrameCodec;

        public bool IsOnline
        {
            get
            {
                return _IsOnline;
            }
        }

        public BaseFrameChannel()
        {
            _IsOnline = false;
            FrameCodec = null;

            Init();
        }

        ~BaseFrameChannel()
        {
            this.Shutdown();
        }

        private void Init()
        {
            IsRunning = false;
            ReceiveTimeer = 10;
            this.LastSendTime = DateTime.Now;

            this.IncommingBuffer = new StreamBuffer();
            this.ReceiveThread = new Thread(new ThreadStart(ReceiveProc));
        }

        public void SetFrameCodec(IFrameCodec<T> codec)
        {
            this.FrameCodec = codec;
        }

        public virtual void Startup()
        {
            this.IsRunning = true;
            ReceiveThread.Start();
        }

        public virtual void Shutdown()
        {
            this.IsRunning = false;
        }

        public void Send(T cmd, byte[] data)
        {
            byte[] msg = this.FrameCodec.Encode(cmd, data);

            this.Send(msg);
        }

        protected override void HandleData(IChannel channel, byte[] data)
        {
            IncommingBuffer.Append(data);

            MessageEnity<T> msg = new MessageEnity<T>();

            bool ret = true;
            while (ret)
            {
                ret = this.FrameCodec.Decode(IncommingBuffer, ref msg);

                if (ret)
                {
                    this.LastSendTime = DateTime.Now;

                    if (msg != null)
                    {
                        HandleFrameData(this, msg);
                    }
                }
            }
        }

        private void ReceiveProc()
        {
            while (IsRunning)
            {
                int Count = 0;

                try
                {
                    if (!_IsOnline)
                    {
                        ResetConnection();
                    }

                    byte[] buf = null;
                    Count = this.Receive(ref buf);

                    if (Count > 0)
                    {
                        this.HandleData(this, buf);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        this.Logger.Error(ex, "Channel Receive Data"); 
                    }
                    catch (Exception)
                    {
                    }
                }

                if (Count == 0)
                {
                    Thread.Sleep(ReceiveTimeer);
                }
            }

            this.Release();
        }

        protected abstract int Receive(ref byte[] buf);
        protected abstract void Release();
        protected abstract void ResetConnection();
        protected abstract void HandleFrameData(IChannel channel, MessageEnity<T> msg);
    }
}
