using ServerCore;
using System.Net;
using System.Text;

namespace Server {
    class ClientSession : PacketSession {
        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected: {endPoint}");
            Thread.Sleep(5000);

            Disconnect();
        }

        // buffer에는 완성된 패킷이 있을 것이다.
        public override void OnRecvPacket(ArraySegment<byte> buffer) {
            // ServerSession과 마찬가지로 count 변수를 둔다.
            ushort count = 0;

            // size가 실제 패킷 정보와 일치하는지 판별하기 어렵다.
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            // id의 enum에 따라 처리를 달리 한다.
            switch ((PacketID) packetId) {
                case PacketID.PlayerInfoReq:
                    // 인스턴스를 생성하고 버퍼를 읽음
                    PlayerInfoReq req = new PlayerInfoReq();
                    req.Read(buffer);
                    Console.WriteLine($"PlayerInfoReq - playerId: {req.playerId}, name: {req.name}");

                    foreach (PlayerInfoReq.Skill skill in req.skills) {
                        Console.WriteLine($"Skill - id: {skill.id}, level: {skill.level}, duration: {skill.duration}");
                    }

                    break;
            }

            Console.WriteLine($"RecvPacketId: {packetId}, Size: {size}");
        }

        public override void OnDisconnected(EndPoint endPoint) {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnSend(int numOfBytes) {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
