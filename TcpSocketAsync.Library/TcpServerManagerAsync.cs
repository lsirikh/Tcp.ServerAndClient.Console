using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpSocketAsyncLibrary
{
    public class TcpServerManagerAsync : ITcpServerManagerAsync
    {
        //소켓
        private Socket socket;

        //연결된 클라이언트 수
        public int ClientCount { get; set; }

        //연결된 클라이언트 소켓 리스트
        public List<TcpClient> ClientList { get; set; }

        public event TcpAccept_dele ServerAccepted;
        public event TcpReceive_dele ServerReceive;
        public event TcpDisconnect_dele ServerDisconnected;

        //문자열 처리용 Stringbuilder
        private StringBuilder sb;

        #region -ctor-
        private static readonly Lazy<TcpServerManagerAsync> _instance = new Lazy<TcpServerManagerAsync>(() => new TcpServerManagerAsync());
        public static TcpServerManagerAsync Instance { get { return _instance.Value; } }

        public TcpServerManagerAsync() { }
        #endregion

        public void Init(IPEndPoint ipep)
        {
            sb = new StringBuilder();
            ClientCount = 0;
            ClientList = new List<TcpClient>();

            CreateSocket(ipep);
        }

        private void CreateSocket(IPEndPoint ipep)
        {
            try
            {
                //소켓 생성
                Socket createdSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                createdSocket.Bind(ipep);
                createdSocket.Listen(20);

                //연결요청 확인 이벤트
                SocketAsyncEventArgs hearingEvent = new SocketAsyncEventArgs();

                //이벤트 RemoteEndPoint 설정
                hearingEvent.RemoteEndPoint = ipep;

                //연결 완료 이벤트 연결
                hearingEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);

                //서버 메시지 대기
                createdSocket.AcceptAsync(hearingEvent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
            }
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                TcpClient cliUser = new TcpClient(e.AcceptSocket);

                //이벤트 연결
                cliUser.AcceptedClientConnected += ClientConncted;
                cliUser.AcceptedClientReceived += ClientReceived;
                cliUser.AcceptedClientDisconnected += ClientDisconnected;

                //클라이언트 리스트 등록
                ClientList.Add(cliUser);

                cliUser.Init();

                Socket socketServer = (Socket)sender;
                e.AcceptSocket = null;
                socketServer.AcceptAsync(e);
            }
            catch (Exception)
            {
            }
        }

        public void SendRequestAll(string msg)
        {
            //연결된 클라이언트에게 메시지 브로드캐스팅
            foreach (TcpClient client in ClientList)
            {
                client.MessageSend(msg);
            }
        }

        private void ClientConncted(object cli)
        {
            //프록시서버로 클라이언트가 성공적으로 접속했을 때

            TcpClient client = (TcpClient)cli;

            client.MessageSend("Welcome!");
            ServerAccepted();
        }

        private void ClientDisconnected(object cli)
        {
            //프록시서버에서 클라이언트 연결이 끊어지면
            try
            {
                TcpClient removeCli = (TcpClient)cli;
                var remoteEP = removeCli.socket.RemoteEndPoint;

                // 클라이언트 리스트에서 해당 소켓 삭제
                ClientList.Remove(removeCli);
                // 소켓 종료
                removeCli.SocketClosed();
                // 서버 연결 종료 이벤트 송신
                ServerDisconnected(remoteEP);
            }
            catch (Exception)
            {
            }
        }

        private void ClientReceived(object data)
        {
            ServerReceive(data);
        }

        public void SocketClosed()
        {
            try
            {
                if (ClientList.Count > 0)
                {
                    foreach (var item in ClientList.ToList())
                    {
                        item.SocketClosed();
                    }
                }
                if (this.socket != null)
                {
                    this.socket.Disconnect(false);
                    this.socket.Close();
                    this.socket.Dispose();
                    this.socket = null;
                }
            }
            catch (Exception)
            {
            }

        }
    }

    public class TcpClient : ITcpClient
    {

        /// <summary>
		/// 문자열처리
		/// </summary>
		private StringBuilder sb;

        /// <summary>
        /// 이 유저의 소켓정보
        /// </summary>
        public Socket socket;

        public event TcpCliAccept_dele AcceptedClientConnected;
        public event TcpReceive_dele AcceptedClientReceived;
        public event TcpCliDiscon_dele AcceptedClientDisconnected;

        /// <summary>
        /// 이 유저의 아이디
        /// </summary>
        public string UserID { get; set; }

        public TcpClient(Socket socketClient)
        {

            //전달받은 소켓 전역으로 활용
            socket = socketClient;

            //문자열생성함수 초기화
            sb = new StringBuilder();
        }

        public void Init()
        {
            try
            {
                // 서버에 보낼 객체를 만든다.
                SocketAsyncEventArgs receiveEvent = new SocketAsyncEventArgs();
                //보낼 데이터를 설정하고
                receiveEvent.UserToken = socket;
                //receiveEvent.UserToken = e.ConnectSocket;

                //ID 설정
                UserID = $"{((IPEndPoint)socket.RemoteEndPoint).Address.ToString()}:{((IPEndPoint)socket.RemoteEndPoint).Port.ToString()}";

                //데이터 길이 세팅
                receiveEvent.SetBuffer(new byte[4096], 0, 4096);
                //받음 완료 이벤트 연결
                receiveEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Recieve_Completed);
                //클라이언트 연결 후, 호출한 서버클래스 내부 작업 요청
                AcceptedClientConnected(this);

                //받음 보냄
                socket.ReceiveAsync(receiveEvent);
            }
            catch (Exception)
            {
            }
        }

        private void Recieve_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                //서버에서 넘어온 정보
                Socket socketClient = (Socket)sender;

                var remoteEP = (IPEndPoint)socketClient.RemoteEndPoint;

                // 접속이 연결되어 있으면...
                if (true == socketClient.Connected && e.BytesTransferred > 0)
                {
                    // 수신 데이터는 e.Buffer에 있다.
                    byte[] data = e.Buffer;
                    // 데이터를 string으로 변환한다.

                    // 메모리 버퍼를 초기화 한다. 크기는 4096이다.
                    e.SetBuffer(new byte[4096], 0, 4096);

                    // 데이터를 string으로 변환한다.
                    string msg = Encoding.UTF8.GetString(data);
                    // StringBuilder에 추가한다.
                    sb.Append(msg.Trim('\0').Replace(Environment.NewLine, string.Empty));

                    //수신시 확인
                    AcceptedClientReceived(sb.ToString());
                    // StringBuilder의 내용을 비운다.
                    sb.Clear();
                    // 메시지가 오면 이벤트를 발생시킨다. (IOCP로 넣는 것)
                    socketClient.ReceiveAsync(e);
                }
                else
                {
                    // TcpServerMangerAsync 클래스로 알림
                    AcceptedClientDisconnected(this);
                }
            }
            catch (Exception)
            {
            }
        }

        public void MessageSend(string msg)
        {
            try
            {
                //3rd Party에게 송신할 객체를 생성
                SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                //보낼 메시지 내용 byte[]로 변환
                byte[] sendData = Encoding.UTF8.GetBytes(msg);
                //데이터 길이 세팅
                sendArgs.SetBuffer(sendData, 0, sendData.Length);

                #region 추가 송신완료 이벤트 연결 시
                //보내기 완료 이벤트 연결
                //sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);
                //보낼 데이터 설정
                //sendArgs.UserToken = sendData;
                #endregion

                // Client로 메시지 전송(비동기식)
                this.socket.SendAsync(sendArgs);
            }
            catch (Exception)
            {
            }
        }

        #region 송신완료 이벤트 연결 시
        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            //유저 소켓
            Socket socketClient = (Socket)sender;
            //UserToken에서 데이터 가져오기
            byte[] msg = (byte[])e.UserToken;
            //데이터 보내기 마무리
            socketClient.Send(msg);
        }
        #endregion

        public void SocketClosed()
        {
            if (socket.Connected)
            {
                socket.Disconnect(false);
            }

            socket.Close();
            socket.Dispose();
            socket = null;
        }

    }

}
