using CmdLine;

namespace Connect.Sockets
{
     [CommandLineArguments(Program = "socket", Title = "socket arguments", Description = "Test for connection limit using system.net.socket")]
    class SocketCommandArgs
    {
         [CommandLineParameter(Command = "ack", Required = false, Default = false, Description = "acknowledge messages")]
         public bool Ack{ get; set; }

         [CommandLineParameter(Command = "size", Required = false, Default = 256, Description = "Size of the payload to send. (There might be framing overhead)")]
         public int Size { get; set; }
    }
}
