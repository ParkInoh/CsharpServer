using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient {
    internal class Program {
        static void Main(string[] args) {
            // DNS는 도메인으로부터 IP를 찾는다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // 도메인에 ip가 여럿일 수 있다. 이 중 0번째를 사용하기로 한다.
            IPAddress ipAddr = ipHost.AddressList[0];
            // 최종 주소를 만들고 클라이언트가 접속할 포트를 지정한다.
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            while (true) {
                // 소켓 생성
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try {
                    // 소켓 연결 시도
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected to: {socket.RemoteEndPoint.ToString()}");

                    // 보내기
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Hello world");
                    int sendBytes = socket.Send(sendBuff);

                    // 받기
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From server] {recvData}");

                    // 연결 끊기
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }

                // 0.1초마다 연결 시도
                Thread.Sleep(100);
            }
        }
    }
}
