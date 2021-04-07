using System;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Opg6_1_2_ZMQ
{

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
    class Program
    {
        private const string TYPEA = "TypeA";
        private const string TYPEB = "TypeB";
        public static string GenerateName(int len)
        {
            Random r = new Random();
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
            string Name = "";
            Name += consonants[r.Next(consonants.Length)].ToUpper();
            Name += vowels[r.Next(vowels.Length)];
            int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                Name += consonants[r.Next(consonants.Length)];
                b++;
                Name += vowels[r.Next(vowels.Length)];
                b++;
            }

            return Name;


        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            using(var client = new RequestSocket())
            {
                client.Connect("tcp://127.0.0.1:5555");
                int idx = 6;
                while (idx-- > 0)
                {
                    if (new Random().Next(100) < 50)
                        AskTypeA(client);
                    else
                        AskTypeB(client);
                }
            }
        }

        static void AskTypeA(RequestSocket client)
        {
            Console.WriteLine("Building type A message");
            var message = new NetMQMessage();
            message.Append(TYPEA);
            TypeAData data = new TypeAData();
            data.email = $"{GenerateName(10)}@somewhere.com";
            data.important = "Secret information";
            data.typeAData = "Type specific data";
            message.Append(JsonConvert.SerializeObject(data));
            client.SendMultipartMessage(message);
            Console.WriteLine($"Message sent from {data.email}!");
            client.ReceiveFrameString();
        }
        static void AskTypeB(RequestSocket client)
        {
            Console.WriteLine("Building type B message");
            var message = new NetMQMessage();
            message.Append(TYPEB);
            TypeBData data = new TypeBData();
            data.email = $"{GenerateName(10)}@somewhere.com";
            data.important = "Secret information";
            data.typeBData = "Type specific data";
            message.Append(JsonConvert.SerializeObject(data));
            client.SendMultipartMessage(message);
            Console.WriteLine($"Message sent from {data.email}!");
            client.ReceiveFrameString();
        }
    }
}
