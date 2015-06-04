using System;
using System.Globalization;
using CmdLine;

namespace Connect
{
    public enum ConnectionType
    {
        socket,
        wcf,
        http,
        none
    }

    [CommandLineArguments(Program = "Connect", Title = "Connection Limit Test", Description = "Test for connection limit")]
    class ConnectArgs
    {
        [CommandLineParameter(Command = "?", Default = false, Description = "Show Help", Name = "Help", IsHelp = true)]
        public bool Help { get; set; }

        [CommandLineParameter(Command = "mode", ParameterIndex = 1, Required = false, Description = "Specified either client mode or server mode.")]
        public string Mode { get; set; }

        [CommandLineParameter(Command = "server", Default = "127.0.0.1", Required = false, Description = "Server to connect to.")]
        public string Server { get; set; }


        [CommandLineParameter(Command = "port", Required = false, Default = 8080, Description = "Server port to listen or connect to.")]
        public int Port { get; set; }

        [CommandLineParameter(Command = "climit", Required = false, Default = 1, Description = "Number of connection.")]
        public int ConnectionLimit { get; set; }

        [CommandLineParameter(Command = "rate", Required = false, Default = 1, Description = "Rate of outbound messages.")]
        public int MessageRate { get; set; }

        [CommandLineParameter(Command = "type", Required = false, Default = "socket", Description = "Connection type.")]
        public string Scenario { get; set; }


        [CommandLineParameter(Command = "args", Required = false, Default = "", Description = "Arguments that need to passed into the scenario.")]
        public string ScenarioArguments { get; set; }

        public ConnectionType Type {
            get
            {
                ConnectionType result;
                if (Enum.TryParse(Scenario, true, out result))
                {
                    return result;
                }

                return ConnectionType.none;
            }
        }
    }

    static class ConnectArgsExtension
    {
        public static string CreateNetTcpAddress(this ConnectArgs args)
        {
            return "net.tcp://" + args.Server + ":" + args.Port;
        }

        public static bool IsServer(this ConnectArgs args)
        {
            return String.Compare("server", args.Mode, true, CultureInfo.InvariantCulture) == 0;
        }
    }
}
