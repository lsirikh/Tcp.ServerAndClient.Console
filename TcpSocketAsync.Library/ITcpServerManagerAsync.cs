using System.Collections.Generic;
using System.Net;

namespace TcpSocketAsyncLibrary
{
    public interface ITcpServerManagerAsync
    {
        int ClientCount { get; set; }
        List<TcpClient> ClientList { get; set; }

        event TcpAccept_dele ServerAccepted;
        event TcpDisconnect_dele ServerDisconnected;
        event TcpReceive_dele ServerReceive;

        void Init(IPEndPoint ipep);
        void SendRequestAll(string msg);
        void SocketClosed();
    }
}