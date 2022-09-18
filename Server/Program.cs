using ServerCore;
using System.Net;

namespace Server {
    

    internal class Program {
        static Listener _listener = new();
        public static GameRoom Room = new();

        static void Main(string[] args) {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 세션매니저가 관리하도록 변경
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("listening..");

            while (true) {
                ;
            }
        }
    }
}
