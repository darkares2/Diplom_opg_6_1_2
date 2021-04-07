using System;
using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace TypeContentRouter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Type content router");
            using (var server = new ResponseSocket())
            using (var publish = new PublisherSocket())
            {
                server.Bind("tcp://*:5555");
                publish.Bind("tcp://*:5556");
                while (true)
                {
                    var message = server.ReceiveMultipartMessage(2);

                    Console.WriteLine("responseSocket : Server Received '{0}'-{1}", message[0].ConvertToString(Encoding.UTF8), message[1].ConvertToString(Encoding.UTF8));
                    server.SendFrameEmpty();

                    publish.SendMultipartMessage(message);
                }
            }
        }
    }
}
