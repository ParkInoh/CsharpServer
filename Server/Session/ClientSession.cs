using ServerCore;
using System.Net;

namespace Server {
    class ClientSession : PacketSession {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected: {endPoint}");
            Program.Room.Enter(this);
        }

        // PacketManager를 호출하는 형식으로 변경
        public override void OnRecvPacket(ArraySegment<byte> buffer) {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint) {
            // 세션매니저에서 해제
            SessionManager.Instance.Remove(this);
            // 방에서 나감
            if (Room != null) {
                Room.Leave(this);
                Room = null;
            }
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnSend(int numOfBytes) {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
