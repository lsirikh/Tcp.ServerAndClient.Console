using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TcpSocketAsyncLibrary;

namespace Tcp.Server.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo keyInfo;

            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("192.168.202.223"), 5000);

            TcpServerManagerAsync.Instance.Init(iPEndPoint);

            TcpServerManagerAsync.Instance.ServerAccepted += Instance_ServerAccepted;

            TcpServerManagerAsync.Instance.ServerReceive += Instance_ServerReceive;

            TcpServerManagerAsync.Instance.ServerDisconnected += Instance_ServerDisconnected;

            

            while (true) 
            {
                keyInfo = System.Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Q)
                    break;
            };

            TcpServerManagerAsync.Instance.SocketClosed();
        }

        private static void Instance_ServerDisconnected(object obj)
        {
            System.Console.WriteLine($"종료");
        }

        private static void Instance_ServerReceive(object data)
        {
            System.Console.WriteLine($"수신 : {data}");

        }

        private static void Instance_ServerAccepted()
        {
            System.Console.WriteLine($"연결");
        }
    }
}
