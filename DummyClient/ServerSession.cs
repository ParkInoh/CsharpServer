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
        public string name;

        // 아래와 같이 구조체 형식을 보내려면?
        // string과 비슷하게 List의 원소 갯수를 보내고, 원소들을 보낸다.
        public struct SkillInfo {
            public int id;
            public short level;
            public float duration;

            // Write와 마찬가지
            public void Read(ReadOnlySpan<byte> span, ref ushort count) {
                this.id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
                count += sizeof(int);
                this.level = BitConverter.ToInt16(span.Slice(count, span.Length - count));
                count += sizeof(short);
                this.duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
                count += sizeof(float);
            }

            // 외부에서 사용하던 count를 참조하고 변경하도록 함
            // span에 값을 추가하고 정상적인지 return
            public bool Write(Span<byte> span, ref ushort count) {
                bool success = true; 
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.duration);
                count += sizeof(float);

                return success;
            }
        }

        public List<SkillInfo> skills = new List<SkillInfo>();

        public PlayerInfoReq () {
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

            // skill 추출
            // 기존 정보 초기화
            skills.Clear();
            ushort skillLength = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLength; i++) {
                SkillInfo skill = new SkillInfo();
                skill.Read(span, ref count);
                skills.Add(skill);
            }
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

            // skill list
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);

            foreach (SkillInfo skill in skills) {
                // count 값을 참조하고 변경하도록 함
                success &= skill.Write(span, ref count);
            }
            
            // 결과 count 값 적용
            success &= BitConverter.TryWriteBytes(span, count);


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

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name = "abcd" };
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 101, level = 3, duration = 3.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 102, level = 2, duration = 4.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 103, level = 6, duration = 7.0f });

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
