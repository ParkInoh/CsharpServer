using ServerCore;
using System.Net;
using System.Text;

namespace DummyClient {
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
    // 위 정보들은 서버도 알고 있어야 한다.
    // 추후에 공통된 곳에 배치할 예정

    class ServerSession : Session {
        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected: {endPoint}");

            // 직렬화: 메모리 상의 인스턴스를 버퍼에 넣는 작업

            PlayerInfoReq packet = new PlayerInfoReq() { 
                //size = ,  // 사이즈는 계산이 끝난 후에 알 수 있다.
                packetId = (ushort) PacketID.PlayerInfoReq,
                playerid = 1001
            };

            // 보내기
            // 자동화하고 싶음
            // 몇 바이트를 넣었는지 추적하는 변수 count를 둔다.

            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            // 한번이라도 실패하면 false가 되도록 success 변수를 & 연산한다.
            ushort count = 0;
            bool success = true;

            // 이전의 바이트 배열 세 줄은 새로 만든다. 대신 segment에 바로 넣어주면 어떨지?
            // BitConverter.TryWriteBytes를 쓸 수 있다.
            // 쓸 위치의 오프셋이 계속 바뀌어야 한다. 또한 버퍼의 크기도 그만큼 줄어야 한다.
            //success &= BitConverter.TryWriteBytes(new Span<byte>(segment.Array, segment.Offset, segment.Count), packet.size);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(segment.Array, segment.Offset + count, segment.Count - count), packet.packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(segment.Array, segment.Offset + count, segment.Count - count), packet.playerid);
            count += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(segment.Array, segment.Offset, segment.Count), count);

            ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);  // 12 = 2 + 2 + 8

            if (success) {
                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint) {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer) {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes) {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
