using System.IoFx.ServiceModel;
using System.Reactive.Linq;

namespace SM.Rx.Test.ServiceModel.TypedContracts
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    #region Contracts
    class Service : IService
    {
        public Order GetOrder(Customer newCustomer)
        {
            return new Order
            {
                Name = newCustomer.Name + "Order"
            };
        }
    }


    #endregion Contracts 

    public class TypedServices
    {
        private const string Address = "net.tcp://localhost:8080";

        public static IDisposable StartService()
        {
            //StartWcfService(address);      
            var binding = new NetTcpBinding() { };
            binding.Security.Mode = SecurityMode.None;
            var listener = binding.Start(Address);
            return listener
                    .OnMessage()
                    .OnGetOrder(c =>
                    {
                        Console.WriteLine(c.Name);
                        return new Order { Name = c.Name + ":Order" };
                    });
        }

        public static IDisposable ChannelModelDispatcher()
        {
            var binding = new NetTcpBinding() { };
            binding.Security.Mode = SecurityMode.None;
            var listener = binding.Start(Address);
            var res = listener.OnMessage()
                        .Do(m => Console.WriteLine(m.Data.Headers.Action))
                        .Subscribe(
                            r =>
                            {
                                Customer c = r.Data.DecodeGetOrder();
                                Console.WriteLine(c.Name);
                                var output = new Order { Name = c.Name + ":Order" };
                                var response = output.EncodeGetOrderResponse(r.Data);
                                r.Channel.Publish(response);
                            },
                            listener.Close
                        );

            return res;
        }

        public static string Invoke(string val = "John Doe")
        {
            var customer = new Customer
            {
                Name = val,
            };

            var binding = new NetTcpBinding { Security = { Mode = SecurityMode.None } };
            var factory = new ChannelFactory<IService>(binding, Address);
            IService proxy = factory.CreateChannel();            
            var response = proxy.GetOrder(customer);
            Console.WriteLine("Response received: " + response.Name);
            return response.Name;
        }

        static void StartWcfService(string address)
        {
            var binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            var host = new ServiceHost(typeof(Service));
            host.AddServiceEndpoint(typeof(IService), binding, address);
            host.Description.Endpoints[0].EndpointBehaviors.Add(new TestInspector());
            host.Open();
            Invoke();
        }

        class TestInspector : IDispatchMessageInspector, IEndpointBehavior
        {
            public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
            {
                return null;
            }
            /*
             * <s:Envelope xmlns:a="http://www.w3.org/2005/08/addressing" xmlns:s="http://www.w3.org/2003/05/soap-envelope">
  <s:Header>
    <a:Action s:mustUnderstand="1">http://tempuri.org/IService/GetOrderResponse</a:Action>
    <a:RelatesTo>urn:uuid:63fa1e27-fccc-4a81-9442-e77d1648c319</a:RelatesTo>
  </s:Header>
  <s:Body>
    <GetOrderResponse xmlns="http://tempuri.org/">
      <GetOrderResult xmlns:d4p1="http://schemas.datacontract.org/2004/07/System.IoFx" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        <d4p1:Name>John DoeOrder</d4p1:Name>
      </GetOrderResult>
    </GetOrderResponse>
  </s:Body>
</s:Envelope>
             * */
            public void BeforeSendReply(ref Message reply, object correlationState)
            {

            }

            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
            {

            }

            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {

            }

            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
            {
                endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
            }

            public void Validate(ServiceEndpoint endpoint)
            {
            }
        }
    }
}
