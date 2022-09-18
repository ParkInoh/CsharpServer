using ServerCore;
using System.Net;

namespace Server {
    class ClientSession : PacketSession {
        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected: {endPoint}");
            Thread.Sleep(5000);

            Disconnect();
        }

        // PacketManager를 호출하는 형식으로 변경
        public override void OnRecvPacket(ArraySegment<byte> buffer) {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint) {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnSend(int numOfBytes) {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
