using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TcpSocketAsyncLibrary;

namespace Tcp.Client.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo keyInfo;

            TcpClientStart();


            while (true)
            {
                keyInfo = System.Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Q)
                    break;

                else if(keyInfo.Key == ConsoleKey.R)
                {
                    TcpClientStop();
                    TcpClientStart();
                }

            };

            TcpClientManagerAsync.Instance.SocketClosed();

        }

        private static void TcpClientStop()
        {
            System.Console.WriteLine("Socket Closed");

            TcpClientManagerAsync.Instance.SocketClosed();

            TcpClientManagerAsync.Instance.Connceted -= Instance_Connceted; ;

            TcpClientManagerAsync.Instance.Disconnected -= Instance_Disconnected; ;

            TcpClientManagerAsync.Instance.Received -= Instance_Received; ;
        }

        private static void TcpClientStart()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("192.168.202.223"), 5000);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("192.168.202.223"), 6000);

            System.Console.WriteLine("Socket Created");
            TcpClientManagerAsync.Instance.Init(remoteEndPoint, localEndPoint);

            TcpClientManagerAsync.Instance.Connceted += Instance_Connceted; ;

            TcpClientManagerAsync.Instance.Disconnected += Instance_Disconnected; ;

            TcpClientManagerAsync.Instance.Received += Instance_Received; ;
        }

        private static void Instance_Connceted()
        {
            System.Console.WriteLine("연결");
            byte[] sendData = Encoding.UTF8.GetBytes("반갑습니다.");
            TcpClientManagerAsync.Instance.MessageSend(sendData);
        }

        private static void Instance_Disconnected()
        {
            System.Console.WriteLine("해제");

        }

        private static void Instance_Received(object data)
        {
            System.Console.WriteLine($"수신 : {data}");
        }
    }
}
