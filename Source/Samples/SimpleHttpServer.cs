namespace Samples
{
    using System;
    using System.IoFx.Http;
    using System.Text;

    class SimpleHttpServer
    {
        public static void Main(string args)
        {
            var contexts = HttpObservable.GetContexts("http://+:8080/");                        
            contexts.Subscribe(async context =>
            {
                var response = context.Response;
                var buffer = Encoding.UTF8.GetBytes("<HTML><BODY> Hello world!</BODY></HTML>");
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            });

            Console.ReadKey();
            contexts.Dispose();
        }
    }
}
