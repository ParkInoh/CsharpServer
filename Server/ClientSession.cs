using ServerCore;
using System.Net;

namespace Server {
    public abstract class Packet {
        public ushort size;
        public ushort packetId;

        // 버퍼에 쓰기/읽기 수행 부분을 분리
        public abstract void Read(ArraySegment<byte> seg);
        public abstract ArraySegment<byte> Write();
    }

    // 예제 패킷
    class PlayerInfoReq : Packet {
        public long playerId;

        public PlayerInfoReq() {
            packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> seg) {
            ushort count = 0;

            // Read 함수로 들어왔다는 것은 size, packetId를 이미 추출했다는 것
            //ushort size = BitConverter.ToUInt16(segment.Array, segment.Offset);
            count += 2;
            //ushort packetId = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
            count += 2;

            // 파싱 시작 위치가 packetId 다음이므로 + count 해준다.
            this.playerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(seg.Array, seg.Offset + count, seg.Count - count));
            count += 8;
        }

        public override ArraySegment<byte> Write() {
            ArraySegment<byte> seg = SendBufferHelper.Open(4096);
            // 한번이라도 실패하면 false가 되도록 success 변수를 & 연산한다.
            ushort count = 0;
            bool success = true;

            // 이전의 바이트 배열 세 줄은 새로 만든다. 대신 segment에 바로 넣어주면 어떨지?
            // BitConverter.TryWriteBytes를 쓸 수 있다.
            // 쓸 위치의 오프셋이 계속 바뀌어야 한다. 또한 버퍼의 크기도 그만큼 줄어야 한다.
            //success &= BitConverter.TryWriteBytes(new Span<byte>(segment.Array, segment.Offset, segment.Count), packet.size);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), this.packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), this.playerId);
            count += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset, seg.Count), count);

            if (success == false) {
                return null;
            }

            return SendBufferHelper.Close(count);  // 12 = 2 + 2 + 8
        }
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

            // size가 실제 패킷 정보와 일치하는지 판별하기 어렵다.
            // size를 임의로 조작하여 보내면 읽을 때 조작된 값이 읽힌다.
            // 즉 값을 믿지 말고 참고만 해야 한다.
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
                    Console.WriteLine($"PlayerInfoReq - playerId: {req.playerId}");
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
