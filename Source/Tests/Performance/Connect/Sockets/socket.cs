using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CmdLine;

namespace Connect.Sockets
{
    class socket : TestScenario
    {
        private IClientManager clients;
        private SocketServer server;

        private SocketCommandArgs _socketArgs;
        public override void Run()
        {
            CommandLine.CommandEnvironment = new CommandEnvironment(this.GetType().Name, ConnectArguments.ScenarioArguments);
            _socketArgs = CommandLine.Parse<SocketCommandArgs>();

            if (ConnectArguments.IsServer())
            {
                server = new SocketServer(ConnectArguments.Port, _socketArgs);
                server.StartServer();
            }
            else
            {
                clients = new SocketClientManager(ConnectArguments, _socketArgs);
                clients.Start();
            }
        }

        class CommandEnvironment : ICommandEnvironment
        {
            private const char SEPERATOR = ' ';
            private readonly string _args;
            private string _program;
            public CommandEnvironment(string program, string args)
            {
                _program = program;
                _args = args;
            }

            public string CommandLine
            {
                get { return _args; }
            }

            public string[] GetCommandLineArgs()
            {
                return GetArgs().ToArray();
            }

            private IEnumerable<string> GetArgs()
            {
                yield return _program;
                foreach (var arg in _args.Split(SEPERATOR))
                {
                    yield return arg;
                }
            }

            public string Program
            {
                get { return _program; }
            }
        }

    }
}
