namespace TcpSocketAsyncLibrary
{
    public delegate void TcpConnect_dele();
    public delegate void TcpSocketCreate_dele();
    public delegate void TcpAccept_dele();
    public delegate void TcpReceive_dele(object data);
    public delegate void TcpSend_dele(string msg);
    public delegate void TcpDisconnect_dele(object obj);
    //20210910
    public delegate void TcpDisconnect_dele_();

    public delegate void TcpCliConn_dele(object obj);
    public delegate void TcpCliAccept_dele(object obj);
    public delegate void TcpCliDiscon_dele(object obj);




    interface ITcpClientControl
    {
        event TcpConnect_dele Connceted;
        event TcpReceive_dele Received;
        //event TcpSend_dele ClientSend;
        event TcpDisconnect_dele_ Disconnected;
    }

    interface ITcpServerControl
    {
        event TcpAccept_dele ServerAccepted;
        event TcpReceive_dele ServerReceive;
        //event TcpSend_dele ServerSend;
        event TcpDisconnect_dele ServerDisconnected;
    }

    interface ITcpAcceptedClientControl
    {
        event TcpCliAccept_dele AcceptedClientConnected;
        event TcpReceive_dele AcceptedClientReceived;
        event TcpCliDiscon_dele AcceptedClientDisconnected;
    }
}