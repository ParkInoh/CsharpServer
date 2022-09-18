using ServerCore;
using System.Net;

namespace Server {
    

    internal class Program {
        static Listener _listener = new Listener();

        static void Main(string[] args) {
            // 여기는 싱글쓰레드라 문제없이 동작
            PacketManager.Instance.Register();

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 리스너 내부 동작에 의해 반복
            _listener.Init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine("listening..");

            while (true) {
                ;
            }
        }
    }
}
