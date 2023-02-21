using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace SoulFab.Core.Communication
{
    public class PipeClient
    {
        private string Name;

        public PipeClient(string name)
        {
            this.Name = name;
        }

        public void Send(string msg)
        {
            var pipe = new NamedPipeClientStream(".", this.Name, PipeDirection.InOut);
            using (pipe)
            {
                pipe.Connect();

                using (StreamWriter writer = new StreamWriter(pipe))
                {
                    writer.WriteLine(msg);
                }
            }
        }
    }
}
