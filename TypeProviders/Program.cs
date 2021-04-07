using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace TypeProviders
{
    class Program
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        class TypeAData
        {
            public string email { get; set; }
            public string important { get; set; }
            public string typeAData { get; set; }
        }
        class TypeBData
        {
            public string email { get; set; }
            public string important { get; set; }
            public string typeBData { get; set; }
        }
        class TypeAOffer
        {
            public string offerData { get; set; }
            public string typeAData { get; set; }
        }
        class TypeBOffer
        {
            public string offerData { get; set; }
            public string typeBData { get; set; }
        }

        static private void WorkerGenerate3Offers(Object data)
        {
            var request = (NetMQMessage)data;
            int offerNo = 0;
            while (offerNo++ < 3)
            {
                Thread.Sleep(new Random().Next(10) * 1000);
                string offerData;
                var message = new NetMQMessage();
                message.Append(request[0].ConvertToString(Encoding.UTF8));
                if (request[0].ConvertToString(Encoding.UTF8).Equals("TypeA"))
                {
                    TypeAData typeAData = JsonConvert.DeserializeObject<TypeAData>(request[1].ConvertToString(Encoding.UTF8));
                    message.Append(typeAData.email);
                    TypeAOffer offer = new TypeAOffer();
                    offer.offerData = $"Pricing and stuff from provider number {offerNo}";
                    offer.typeAData = typeAData.typeAData;
                    offerData = JsonConvert.SerializeObject(offer);
                }
                else if (request[0].ConvertToString(Encoding.UTF8).Equals("TypeB"))
                {
                    TypeBData typeBData = JsonConvert.DeserializeObject<TypeBData>(request[1].ConvertToString(Encoding.UTF8));
                    message.Append(typeBData.email);
                    TypeBOffer offer = new TypeBOffer();
                    offer.offerData = $"Pricing and stuff from provider number {offerNo}";
                    offer.typeBData = typeBData.typeBData;
                    offerData = JsonConvert.SerializeObject(offer);
                }
                else
                    throw new Exception("Incorrect type");
                message.Append(offerData);
                using (var offerSocket = new RequestSocket())
                {
                    offerSocket.Connect("tcp://127.0.0.1:5557");
                    offerSocket.SendMultipartMessage(message);
                }
            }
        }

        static private void WorkerTypeA()
        {
            using (var typeASubscriber = new SubscriberSocket())
            {
                typeASubscriber.Options.ReceiveHighWatermark = 1000;
                typeASubscriber.Connect("tcp://127.0.0.1:5556");
                typeASubscriber.Subscribe("TypeA");
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var message = typeASubscriber.ReceiveMultipartMessage(2);
                    Console.WriteLine("TypeA : Server Received '{0}'-{1}", message[0].ConvertToString(Encoding.UTF8), message[1].ConvertToString(Encoding.UTF8));
                    Task.Factory.StartNew(WorkerGenerate3Offers, message);
                }
                // Cancel the task and exit
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }
        static private void WorkerTypeB()
        {
            using (var typeASubscriber = new SubscriberSocket())
            {
                typeASubscriber.Options.ReceiveHighWatermark = 1000;
                typeASubscriber.Connect("tcp://127.0.0.1:5556");
                typeASubscriber.Subscribe("TypeB");
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var message = typeASubscriber.ReceiveMultipartMessage(2);
                    Console.WriteLine("TypeB : Server Received '{0}'-{1}", message[0].ConvertToString(Encoding.UTF8), message[1].ConvertToString(Encoding.UTF8));
                    Task.Factory.StartNew(WorkerGenerate3Offers, message);
                }
                // Cancel the task and exit
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Providers!");
            Task.Factory.StartNew(WorkerTypeA
            , cancellationTokenSource.Token
            , TaskCreationOptions.LongRunning
            , TaskScheduler.Default);

            Task.Factory.StartNew(WorkerTypeB
            , cancellationTokenSource.Token
            , TaskCreationOptions.LongRunning
            , TaskScheduler.Default);
            while (true)
                Thread.Sleep(1000);
        }
    }
}
