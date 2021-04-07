using System;
using System.Collections.Generic;
using System.Text;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Emailer
{
    class Program
    {
        private const string TYPEA = "TypeA";
        private const string TYPEB = "TypeB";

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

        static void Main(string[] args)
        {
            Console.WriteLine("Emailer!");

            using (var server = new ResponseSocket())
            {
                server.Bind("tcp://*:5558");
                while (true)
                {
                    var message = server.ReceiveMultipartMessage(5);
                    Console.WriteLine("responseSocket : Server Received '{0}'-{1}", message[0].ConvertToString(Encoding.UTF8), message[1].ConvertToString(Encoding.UTF8));
                    EmailOffer(message);
                    server.SendFrameEmpty();
                }
            }

        }

        private static void EmailOffer(NetMQMessage message)
        {
            if (message[0].ConvertToString(Encoding.UTF8).Equals(TYPEA))
            {
                EmailTypeAOffer(message);
            } else if (message[0].ConvertToString(Encoding.UTF8).Equals(TYPEB))
            {
                EmailTypeBOffer(message);
            }
        }

        private static void EmailTypeBOffer(NetMQMessage message)
        {
            List<TypeBOffer> offers = new List<TypeBOffer>();

            for(int idx = 2; idx < 5; ++idx)
            {
                offers.Add(JsonConvert.DeserializeObject<TypeBOffer>(message[idx].ConvertToString(Encoding.UTF8)));
            }
            SendEmail(message[0].ConvertToString(Encoding.UTF8), message[1].ConvertToString(Encoding.UTF8), JsonConvert.SerializeObject(offers));
        }


        private static void EmailTypeAOffer(NetMQMessage message)
        {
            List<TypeAOffer> offers = new List<TypeAOffer>();

            for (int idx = 2; idx < 5; ++idx)
            {
                offers.Add(JsonConvert.DeserializeObject<TypeAOffer>(message[idx].ConvertToString(Encoding.UTF8)));
            }
            SendEmail(message[0].ConvertToString(Encoding.UTF8), message[1].ConvertToString(Encoding.UTF8), JsonConvert.SerializeObject(offers));
        }


        private static void SendEmail(string type, string email, string offers)
        {
            Console.WriteLine($"Type: {type}, Email: {email}. Offers:{offers}");
        }
    }
}
