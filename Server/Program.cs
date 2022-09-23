using ServerCore;
using System.Net;

namespace Server {
    internal class Program {
        static Listener _listener = new();
        public static GameRoom Room = new();

        static void FlushRoom() {
            Room.Push(() => Room.Flush());
            // 다음 호출을 예약함
            JobTimer.Instance.Push(FlushRoom, 250);
        }

        static void Main(string[] args) {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 세션매니저가 관리하도록 변경
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("listening..");

            // JobTimer가 관리하도록 함
            JobTimer.Instance.Push(FlushRoom);

            while (true) {
                JobTimer.Instance.Flush();
            }
        }
    }
}
