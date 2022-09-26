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
    public class TcpClientManagerAsync : ITcpClientManagerAsync
    {
        // 소켓 생성
        private Socket socket;

        // TCP 인터페이스 이벤트
        public event TcpConnect_dele Connceted;
        public event TcpReceive_dele Received;
        public event TcpDisconnect_dele_ Disconnected;

        //private QueueClass QC;

        //Singleton 적용 20210910
        private static readonly Lazy<TcpClientManagerAsync> _instance = new Lazy<TcpClientManagerAsync>(() => new TcpClientManagerAsync());

        public static TcpClientManagerAsync Instance { get { return _instance.Value; } }

        #region -ctor-
        private TcpClientManagerAsync()
        {
        }
        #endregion

        public void Init(IPEndPoint ipep, IPEndPoint localIpep = null)
        {
            CreateSocket(ipep, localIpep);
        }

        private void CreateSocket(IPEndPoint ipep, IPEndPoint localIpep = null)
        {
            try
            {
                //소켓 생성
                Socket createdSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                createdSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                if (localIpep != null)
                    createdSocket.Bind(localIpep);

                //연결요청 확인 이벤트
                SocketAsyncEventArgs lookingEvent = new SocketAsyncEventArgs();

                //이벤트 RemoteEndPoint 설정
                lookingEvent.RemoteEndPoint = ipep;

                //연결 완료 이벤트 연결
                lookingEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);

                //서버 메시지 대기
                createdSocket.ConnectAsync(lookingEvent);
            }
            catch (Exception)
            {
            }
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            //socket = (Socket)sender;
            if (e.SocketError == System.Net.Sockets.SocketError.ConnectionRefused)
            {
                Received($"연결이 거부되었습니다.({e.SocketError})");
                SocketClosed();
                return;
            }

            if (e.SocketError == System.Net.Sockets.SocketError.AddressAlreadyInUse)
            {
                Received($"사용중인 아이피 혹은 포트입니다.({e.SocketError})");
                SocketClosed();
                return;
            }

            try
            {
                if (true == ((Socket)sender).Connected)
                {
                    this.socket = e.ConnectSocket;
                    var info = this.socket.LocalEndPoint.ToString();

                    // 프록시 클라이언트가 미들웨어 연결됨을 이벤트로 알림
                    Connceted();

                    //서버에 보낼 객체를 만든다.
                    SocketAsyncEventArgs receiveEvent = new SocketAsyncEventArgs();
                    //보낼 데이터를 설정하고
                    receiveEvent.UserToken = this.socket;
                    //데이터 길이 세팅
                    receiveEvent.SetBuffer(new byte[4096], 0, 4096);
                    //받음 완료 이벤트 연결
                    receiveEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Recieve_Completed);
                    //받음 보냄
                    socket.ReceiveAsync(receiveEvent);
                }
            }
            catch (Exception)
            {
            }

        }

        private void Recieve_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                // 접속이 연결되어 있으면...
                if (((Socket)sender).Connected && e.BytesTransferred > 0)
                {
                    // 수신 데이터는 e.Buffer에 있다.
                    byte[] data = e.Buffer;
                    // 데이터를 string으로 변환한다.

                    // 메모리 버퍼를 초기화 한다. 크기는 4096이다.
                    e.SetBuffer(new byte[4096], 0, 4096);

                    // 데이터를 string으로 변환한다.
                    string msg = Encoding.UTF8.GetString(data);

                    //메시지를 받아온다.
                    Received(msg.Trim('\0').Replace(Environment.NewLine, string.Empty));

                    // 메시지가 오면 이벤트를 발생시킨다. (IOCP로 넣는 것)
                    this.socket.ReceiveAsync(e);
                }
                else
                {
                    Disconnected?.Invoke();
                }
            }
            catch (Exception)
            {
            }
        }

        public void MessageSend(byte[] msg)
        {
            try
            {
                //3rd Party에게 송신할 객체를 생성
                SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                //보낼 메시지 내용 byte[]로 변환
                //byte[] sendData = Encoding.ASCII.GetBytes(msg);
                //데이터 길이 세팅
                sendArgs.SetBuffer(msg, 0, msg.Length);

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

        public void SocketClosed()
        {
            if (this.socket != null)
            {
                if (this.socket.Connected)
                {
                    //연결요청 확인 이벤트
                    SocketAsyncEventArgs disconnectEvent = new SocketAsyncEventArgs();
                    this.socket.DisconnectAsync(disconnectEvent);

                    disconnectEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Disconnect_Complete);
                }
            }
        }

        private void Disconnect_Complete(object sender, SocketAsyncEventArgs e)
        {
            if (true != ((Socket)sender).Connected)
            {
                this.socket.Close();
                this.socket.Dispose();
                this.socket = null;
            }
        }
    }
}
