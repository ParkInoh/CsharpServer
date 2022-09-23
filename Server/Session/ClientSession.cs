using ServerCore;
using System.Net;

namespace Server {
    class ClientSession : PacketSession {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected: {endPoint}");
            // Action 형식으로 변경
            Program.Room.Push(() => Program.Room.Enter(this));
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
                // Action 형식으로 변경
                // Action이기에 null을 참조할 수 있기에 참조자를 바꿈
                GameRoom room = Room;
                room.Push(() => room.Leave(this));
                Room = null;
            }
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnSend(int numOfBytes) {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
