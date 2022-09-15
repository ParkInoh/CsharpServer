using ServerCore;
using System.Net;

namespace Server {
    class Packet {
        public ushort size;
        public ushort packetId;
    }

    // 예제 패킷
    class PlayerInfoReq : Packet {
        public long playerid;
    }

    class PlayerInfoOk : Packet {
        public int hp;
        public int attack;
    }

    // 패킷의ID 를 구분할 수 있어야 함
    public enum PacketID {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }
    // 위 정보들은 클라이언트도 알고 있어야 한다.
    // 추후에 공통된 곳에 배치할 예정

    class ClientSession : PacketSession {
        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected: {endPoint}");

            //// 더미 클라이언트 측으로 이동
            //Packet packet = new Packet() { size = 100, packetId = 10 };

            //// 버퍼 헬퍼를 사용하도록 변경
            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            //byte[] buffer = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

            //Send(sendBuff);

            Thread.Sleep(5000);

            Disconnect();
        }

        // buffer에는 완성된 패킷이 있을 것이다.
        public override void OnRecvPacket(ArraySegment<byte> buffer) {
            // ServerSession과 마찬가지로 count 변수를 둔다.
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            // id의 enum에 따라 처리를 달리 한다.
            switch ((PacketID) packetId) {
                case PacketID.PlayerInfoReq:
                    // 파싱 시작 위치가 packetId 다음이므로 + count 해준다.
                    long playerId = BitConverter.ToInt64(buffer.Array, buffer.Offset + count);
                    count += 8;
                    Console.WriteLine($"PlayerInfoReq - playerId: {playerId}");
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
