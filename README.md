# TCP socket 통신을 위한 서버와 클라이언트

## TCP Server와 Client를 Console 프로그램 상에서 확인 할 수 있음.

*이슈사항

1. 클라이언트에 IPAddress 및 포트를 bindin 할 경우, 재 접속시 (CMD 창에)netstat에 TIME_OUT 상태로 남아서 한동안 해당 포트와 IP를 사용한 소켓을 이용할 수 없게 된다.

2. 물론 e.SocketError에서도 확인 가능하다.