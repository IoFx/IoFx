using System;
using System.Diagnostics.Contracts;
using IoFx.Connections;
using IoFx.ServiceModel;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml;

namespace SM.Rx.Test.ServiceModel.TypedContracts
{

    #region Contracts 
    [ServiceContract]
    interface IService
    {
        [OperationContract]
        Order GetOrder(Customer newCustomer);
    }


    [DataContract]
    class Customer
    {
        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    class Order
    {
        [DataMember]
        public string Name { get; set; }
    }

    #endregion Contracts

    #region IService Helper Extensions
    static class ServiceDispatcherExtensions
    {
        public static bool IsGetOrder(this Message m)
        {
            return m.Headers.Action == "http://tempuri.org/IService/GetOrder";
        }

        public static Customer DecodeGetOrder(this Message r)
        {
            Customer customer;
            r.Deserialize<Customer>(out  customer);
            return customer;
        }

        public static Message EncodeGetOrderResponse(this Order order, Message request)
        {
            var msg = PrepareResponse(order, request);
            return msg;
        }

        private static Message PrepareResponse<T>(T order, Message request)
        {
            var dcs = new DataContractSerializer(typeof(T));

            Action<XmlDictionaryWriter, T> onWrite = (writer, o) =>
            {
                writer.WriteStartElement("GetOrderResponse", "http://tempuri.org/");
                writer.WriteStartElement("GetOrderResult", "http://tempuri.org/");
                dcs.WriteObjectContent(writer, o);
                writer.WriteEndElement();
                writer.WriteEndElement();
            };

            var msg = Message.CreateMessage(request.Version,
                                            "http://tempuri.org/IService/GetOrderResponse",
                                            new DelegatingBodyWriter<T>(
                                                onWrite,
                                                order));

            msg.Headers.RelatesTo = request.Headers.MessageId;
            return msg;
        }

        public static IDisposable OnGetOrder(
            this IObservable<Context<Message>> iochannel,
            Func<Customer, Order> operation)
        {
            var responses = iochannel.OnOperation(m => m.IsGetOrder(),
                                                operation,
                                                m => m.DecodeGetOrder(),
                                                (o, req) => o.EncodeGetOrderResponse(req));

            return responses.Consume();
        }

        public static IDisposable OnGetOrder(
            this IObservable<Context<Message>> iochannel, 
            Func<Customer, Task<Order>> operation)
        {
            var responses = iochannel
                .Where(m => m.Message.IsGetOrder())
                .Select(async (m) =>
                    {
                        var input = m.Message.DecodeGetOrder();
                        var output = await operation(input);
                        var response = output.EncodeGetOrderResponse(m.Message);
                        return new Context<Message>
                        {
                            Channel =  m.Channel,
                            Message =  response
                        };
                    });

            return responses.Consume();
        }

        class DelegatingBodyWriter<T> : BodyWriter
        {
            private readonly Action<XmlDictionaryWriter, T> OnWrite;

            public DelegatingBodyWriter(Action<XmlDictionaryWriter, T> onWrite, T instance)
                : base(true)
            {
                Contract.Assert(onWrite != null);
                this.OnWrite = onWrite;
                this.State = instance;
            }

            T State { get; set; }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                this.OnWrite(writer, this.State);
            }            
        }

        static void Deserialize<T>(this Message m, out T arg1)
        {
            var serializer = new DataContractSerializer(typeof(T));
            var reader = m.GetReaderAtBodyContents();
            reader.Read();
            arg1 = (T)serializer.ReadObject(reader, false);
        }
    }
    #endregion

}


