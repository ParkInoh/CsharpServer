using ServerCore;
using System;
using System.Net;
using System.Text;

namespace DummyClient {
    public abstract class Packet {
        // 패킷의 사이즈와 Id가 굳이 필요한가?
        // 생성자에서 할당하는데, TryWriteBytes 에서 직접 넣으면?
        // size도 패킷세션이 참고만 함
        public ushort size;
        public ushort packetId;

        // 버퍼에 쓰기/읽기 수행 부분을 분리
        public abstract void Read(ArraySegment<byte> seg);
        public abstract ArraySegment<byte> Write();
    }

    // 예제 패킷
    class PlayerInfoReq : Packet {
        public long playerId;

        public PlayerInfoReq () {
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
            // 바로 변환하는 것이 아니라, 충분한 공간이 있는지 확인해야 한다.
            // ReadOnlySpan은, 지정한 범위를 집어준다. 범위가 벗어나면 예외가 처리될 것이다.
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
    // 위 정보들은 서버도 알고 있어야 한다.
    // 추후에 공통된 곳에 배치할 예정

    class ServerSession : Session {
        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected: {endPoint}");

            // 직렬화: 메모리 상의 인스턴스를 버퍼에 넣는 작업

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001 };

            // 보내기
            // Write 함수로 처리됨
            ArraySegment<byte> seg = packet.Write();

            if (seg != null) {
                Send(seg);
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
