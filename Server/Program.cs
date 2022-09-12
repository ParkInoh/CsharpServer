using ServerCore;
using System.Net;
using System.Text;

namespace Server {
    class GameSession : Session {
        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected: {endPoint}");

            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Server");
            Send(sendBuff);

            Thread.Sleep(1000);

            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint) {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        // 받은 버퍼 크기를 반환
        public override int OnRecv(ArraySegment<byte> buffer) {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From client] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes) {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }

    internal class Program {
        static Listener _listener = new Listener();

        static void Main(string[] args) {
            // 하드코딩하지 않고 DNS를 사용한다.
            // DNS는 도메인으로부터 IP를 찾는다.
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // 도메인에 ip가 여럿일 수 있다. 이 중 0번째를 사용하기로 한다.
            IPAddress ipAddr = ipHost.AddressList[0];
            // 최종 주소를 만들고 클라이언트가 접속할 포트를 지정한다.
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 리스너 초기화
            // 리스너 내부 동작에 의해 반복된다.
            _listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("listening..");

            while (true) {
                ;
            }
        }
    }
}
