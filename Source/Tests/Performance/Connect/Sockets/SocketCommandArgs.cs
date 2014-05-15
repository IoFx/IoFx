using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CmdLine;

namespace Connect.Sockets
{
     [CommandLineArguments(Program = "socket", Title = "socket arguments", Description = "Test for connection limit using system.net.socket")]
    class SocketCommandArgs
    {
         [CommandLineParameter(Command = "ack", Required = false, Default = false, Description = "acknowledge messages")]
         public bool Ack{ get; set; }
    }
}
