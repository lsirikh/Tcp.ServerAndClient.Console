namespace TcpSocketAsyncLibrary
{
    public interface ITcpClient
    {
        string UserID { get; set; }

        event TcpCliAccept_dele AcceptedClientConnected;
        event TcpCliDiscon_dele AcceptedClientDisconnected;
        event TcpReceive_dele AcceptedClientReceived;

        void Init();
        void MessageSend(string msg);
        void SocketClosed();
    }
}