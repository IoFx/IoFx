using System.Reactive.Disposables;
using CmdLine;
using System;
using System.Diagnostics;
using System.Threading;

namespace Connect
{
    class Program
    {
        static void Main(string[] args)
        {
            SetupThreadPool();

            try
            {
                Console.WriteLine("PID:" + Process.GetCurrentProcess().Id);
                var arguments = CommandLine.Parse<ConnectArgs>();
                IDisposable resources = Disposable.Empty;

                var scenario = TestScenario.Create(arguments);
                scenario.Run();
                CommandLine.Pause();
                scenario.Dispose();
            }
            catch (CommandLineException exception)
            {
                Console.WriteLine(exception.ArgumentHelp.Message);
                Console.WriteLine(exception.ArgumentHelp.GetHelpText(Console.BufferWidth));
            }
        }

        private static void SetupThreadPool()
        {
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
            }

            ThreadPool.SetMinThreads(500, 1000);
        }
    }
}
