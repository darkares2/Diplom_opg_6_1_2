using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace TypeClientAggregator
{
    class Program
    {
        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        static void Main(string[] args)
        {
            Dictionary<string, List<string>> offerStorage = new Dictionary<string, List<string>>();
            Console.WriteLine("TypeClientAggregator");
            using (var server = new ResponseSocket())
            using (var emailer = new RequestSocket())
            {
                server.Bind("tcp://*:5557");
                emailer.Connect("tcp://127.0.0.1:5558");
                while (true)
                {
                    var message = server.ReceiveMultipartMessage(3);

                    Console.WriteLine("responseSocket : Server Received '{0}'-{1}", message[0].ConvertToString(Encoding.UTF8), message[1].ConvertToString(Encoding.UTF8));
                    string hashKey = GetHashString(message[0].ConvertToString(Encoding.UTF8) + message[1].ConvertToString(Encoding.UTF8));
                    if (offerStorage.ContainsKey(hashKey))
                    {
                        offerStorage[hashKey].Add(message[2].ConvertToString(Encoding.UTF8));
                        if (offerStorage[hashKey].Count >= 3)
                        {
                            mailOffer(emailer, message[0].ConvertToString(Encoding.UTF8), message[1].ConvertToString(Encoding.UTF8), offerStorage[hashKey]);
                            offerStorage.Remove(hashKey);
                        }
                    } else
                    {
                        offerStorage.Add(hashKey, new List<string>());
                        offerStorage[hashKey].Add(message[2].ConvertToString(Encoding.UTF8));
                    }
                    server.SendFrameEmpty();
                }
            }

        }

        private static void mailOffer(RequestSocket emailer, string type, string email, List<string> offers)
        {
            Console.WriteLine($"Have all offers, sending {type} to {email}");
            var message = new NetMQMessage();
            message.Append(type);
            message.Append(email);
            foreach (string offer in offers)
                message.Append(offer);
            emailer.SendMultipartMessage(message);
            emailer.ReceiveFrameString();
        }
    }
}
