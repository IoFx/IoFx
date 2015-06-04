using CmdLine;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace FixPortLimit
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var arguments = CommandLine.Parse<ConnectArgs>();
                if (arguments.fix || arguments.show)
                {
                    Elevate(arguments);
                }
                else if (arguments.show)
                {

                }
                else
                {
                    var help = new CommandArgumentHelp(typeof(ConnectArgs));
                    Console.WriteLine(help.Message);
                    Console.WriteLine(help.GetHelpText(Console.BufferWidth));

                }
            }
            catch (CommandLineException exception)
            {
                Console.WriteLine(exception.ArgumentHelp.Message);
                Console.WriteLine(exception.ArgumentHelp.GetHelpText(Console.BufferWidth));
            }
        }

        private static void Elevate(ConnectArgs arguments)
        {
            string key = @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameter";
            if (arguments.show)
            {
                ProcessStartInfo info = new ProcessStartInfo()
                { 
                    Arguments = "/c reg query " + key,
                    FileName = "cmd.exe"
                };
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                var p = Process.Start(info);
                p.WaitForExit();
                Console.WriteLine(p.StandardOutput.ReadToEnd());
                if (p.ExitCode != 0)
                {
                    Console.WriteLine("Default port configuration is present on the system.");
                }
            }
            else if (arguments.fix)
            {
                ProcessStartInfo info = new ProcessStartInfo()
                {
                    Arguments = "/c reg add " + key,
                    FileName = "cmd.exe"
                };
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;                
                var p = Process.Start(info);
                p.WaitForExit();
                Console.WriteLine(p.StandardOutput.ReadToEnd());
                if (p.ExitCode != 0)
                {
                    Console.WriteLine("Default port configuration is present on the system.");
                }
            }
        }
    }

    [CommandLineArguments(Program = "FixPortLimit", Title = "Update Port limit", Description = "Update port limit for high number of connection.")]
    class ConnectArgs
    {
        [CommandLineParameter(Command = "?", Default = true, Description = "Show Help", Name = "Help", IsHelp = true)]
        public bool Help { get; set; }

        [CommandLineParameter(Command = "fix", Required = false, Description = "Update the MaxUserPort")]
        public bool fix { get; set; }

        [CommandLineParameter(Command = "show", Required = false, Description = "Query the MaxUserPort")]
        public bool show { get; set; }

    }
}
