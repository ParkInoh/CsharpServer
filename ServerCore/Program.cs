using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore {
    internal class Program {
        static void Main(string[] args) {
            // 하드코딩하지 않고 DNS를 사용한다.
            // DNS는 도메인으로부터 IP를 찾는다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // 도메인에 ip가 여럿일 수 있다. 이 중 0번째를 사용하기로 한다.
            IPAddress ipAddr = ipHost.AddressList[0];
            // 최종 주소를 만들고 클라이언트가 접속할 포트를 지정한다.
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 소켓 생성
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try {
                // 소켓에 주소 연동
                listenSocket.Bind(endPoint);
                // 소켓을 listen 상태로 만듦.
                // backlog: 접속 가능한 최대 연결 수
                listenSocket.Listen(10);

                while (true) {
                    Console.WriteLine("listening..");

                    // 연결
                    Socket clientSocket = listenSocket.Accept();

                    // 받기
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From client] {recvData}");

                    // 보내기
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Server");
                    clientSocket.Send(sendBuff);

                    // 연결 중단
                    // 연결이 중단됨을 미리 알림
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
