using System.Net;

namespace TcpSocketAsyncLibrary
{
    public interface ITcpClientManagerAsync
    {
        event TcpConnect_dele Connceted;
        event TcpDisconnect_dele_ Disconnected;
        event TcpReceive_dele Received;

        void Init(IPEndPoint ipep, IPEndPoint localIpep = null);
        void MessageSend(byte[] msg);
        void SocketClosed();
    }
}