﻿using ServerCore;
using System.Net;
using System.Text;

namespace Server {
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
        // 가변적인 크기의 패킷을 적용한다(string 등).
        public long playerId;
        public string name;

        public PlayerInfoReq() {
            packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> seg) {
            ushort count = 0;

            // ReadOnlySpan은, 지정한 범위를 집어준다. 범위가 벗어나면 예외가 처리될 것이다.
            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);

            // Read 함수로 들어왔다는 것은 size, packetId를 이미 추출했다는 것
            count += sizeof(ushort);
            count += sizeof(ushort);
            this.playerId = BitConverter.ToInt64(span.Slice(count, span.Length - count));
            count += sizeof(long);

            // string 추출
            // 1. string byte 배열 길이
            // 2. string byte 배열
            ushort nameLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
            count += nameLen;
        }

        public override ArraySegment<byte> Write() {
            ArraySegment<byte> seg = SendBufferHelper.Open(4096);
            // 한번이라도 실패하면 false가 되도록 success 변수를 & 연산한다.
            ushort count = 0;
            bool success = true;

            // span을 한번만 선언하고, Slice로 일부를 지정해서 넘겨준다.
            // 지정한 부분은 Span<> 으로 추출된다.
            Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.packetId);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
            count += sizeof(long);

            // 수신자 입장에서 받는 string은 길이를 알 수 없다.
            // 그래서 보낼 때 1. string의 길이를 보내고, 2. byte 배열을 보낸다.
            // string.Length를 하면 길이가 나오지만 byte 배열로 바꾸면 길이가 달라질 수 있다.
            // 그래서 Encoding.Unicode.GetByteCount를 사용한다.
            //ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(name);
            //success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
            //count += sizeof(ushort);
            // Encoding.Unicode.GetBytes를 사용하면 문자열을 바이트 배열로 바꾸어 준다.
            //Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, seg.Array, count, nameLen);

            // 복사를 바로 하고, 길이를 나중에 넣는 방법
            // byte배열 길이를 넣을 공간을 미리 만들어 둔다.
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, seg.Array, seg.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            // 결과 count 값 적용
            success &= BitConverter.TryWriteBytes(span, count);


            if (success == false) {
                return null;
            }

            return SendBufferHelper.Close(count);  // 12 = 2 + 2 + 8
        }
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
