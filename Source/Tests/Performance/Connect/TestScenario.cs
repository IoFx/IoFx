using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Connect
{
    abstract class TestScenario : IDisposable
    {
        public ConnectArgs ConnectArguments { get; set; }

        public static TestScenario Create(ConnectArgs commandline)
        {
            string scenario = commandline.Scenario;

            if (string.IsNullOrEmpty(scenario))
            {
                throw new ArgumentNullException("scenario");
            }            

            var scenarios = Assembly.GetExecutingAssembly()
                                    .GetTypes()
                                    .Where(t => typeof(TestScenario).IsAssignableFrom(t) && t != typeof(TestScenario) && t.IsClass)
                                    .ToArray();

            var type = scenarios.FirstOrDefault(t => t.Name.EndsWith(scenario, true, CultureInfo.InvariantCulture));
            if (type == null)
            {
                Console.WriteLine("Scenario {0} not found.", scenario);
                Console.WriteLine("Available scenarios: \n");
                foreach (var name in scenarios)
                {
                    Console.WriteLine("\t" + name.Name);
                }

                return null;
            }

            Console.WriteLine("Executing scenario:" + type.Name);
            var instance = (TestScenario)Activator.CreateInstance(type);
            instance.ConnectArguments = commandline;
            return instance;
        }

        public abstract void Run();

        class none : TestScenario
        {
            public override void Run()
            {                
            }
        }

        public void Dispose()
        {
            
        }
    }
}
