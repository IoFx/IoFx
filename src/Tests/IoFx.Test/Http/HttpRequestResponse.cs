using System;
using System.IO;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using IoFx.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoFx.Test.Http
{
    [TestClass]
    public class HttpRequestResponse
    {
        [TestMethod]
        public void HelloWorld()
        {
            const string address = "http://localhost:8080/";
            var contextsHot = HttpObservable.GetContexts(address);
            int count = 0;
            const int expected = 2;

            try
            {
                var contexts = contextsHot.Publish();
                contexts.Subscribe(RespondWithHelloWorld);
                contexts.Subscribe(_ => Interlocked.Increment(ref count));
                var waiter = contexts.ToTask();
                contexts.Connect();

                for (int i = 0; i < expected; i++)
                {
                    GetRequest(address);
                }

                contextsHot.Dispose();
                waiter.Wait(TimeSpan.FromSeconds(2));
            }
            finally 
            {
                contextsHot.Dispose();
            }

            Assert.AreEqual(expected, count);
        }

        private static void RespondWithHelloWorld(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            const string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        public static void GetRequest(string address)
        {
            WebRequest request = WebRequest.Create(address);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            Stream dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine(responseFromServer);
            reader.Close();
            response.Close();
        }
    }
}
