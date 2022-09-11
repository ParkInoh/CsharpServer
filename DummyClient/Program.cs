using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient {
    class GameSession : Session {
        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected: {endPoint}");

            // 보내기
            for (int i = 0; i < 5; i++) {
                byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello world: {i}");
                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint) {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer) {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From server] {recvData}");
        }

        public override void OnSend(int numOfBytes) {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }

    internal class Program {
        static void Main(string[] args) {
            // DNS는 도메인으로부터 IP를 찾는다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // 도메인에 ip가 여럿일 수 있다. 이 중 0번째를 사용하기로 한다.
            IPAddress ipAddr = ipHost.AddressList[0];
            // 최종 주소를 만들고 클라이언트가 접속할 포트를 지정한다.
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 커넥터를 사용하도록 연결 변경
            Connecter connecter = new Connecter();
            connecter.Connect(endPoint, () => { return new GameSession(); });

            while (true) {
                //try {
                    
                //}
                //catch (Exception e) {
                //    Console.WriteLine(e.ToString());
                //}

                // 0.1초마다 연결 시도
                Thread.Sleep(100);
            }
        }
    }
}
