using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace SoulFab.Core.Communication
{
    public class PipeMessageEventArgs : EventArgs
    {
        public readonly string Message;

        public PipeMessageEventArgs(string msg)
        {
               this.Message = msg;
        }
    }

    public class PipeServer
    {
        private string Name;
        private bool Running;

        private Thread ServerThread;

        public event EventHandler<PipeMessageEventArgs> OnMessage;

        public PipeServer(string name)
        {
            this.Running = false;
            this.Name = name;

            this.ServerThread = new Thread(new ThreadStart(this.ServerProcess));
        }

        public void Start()
        {
            this.Running = true;
            this.ServerThread.Start();
        }

        private void ServerProcess()
        {
            while (this.Running)
            {
                var pipe = new NamedPipeServerStream(this.Name, PipeDirection.InOut);
                using (pipe)
                {
                    pipe.WaitForConnection();

                    StreamReader reader = new StreamReader(pipe);
                    using (reader)
                    {
                        while (this.Running)
                        {
                            string txt = reader.ReadLine();

                            if (!String.IsNullOrEmpty(txt))
                            {
                                OnMessage?.Invoke(this, new PipeMessageEventArgs(txt));
                            }
                        }
                    }
                }

            }
        }
    }
}
