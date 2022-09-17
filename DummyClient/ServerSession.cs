using ServerCore;
using System.Net;
using System.Text;

namespace DummyClient {
    public enum PacketID {
        PlayerInfoReq = 1,
        Test = 2,

    }

    class PlayerInfoReq {
        public byte testByte;
        public long playerId;
        public string name;
        public class Skill {
            public int id;
            public short level;
            public float duration;
            public class Attribute {
                public int att;

                public void Read(ReadOnlySpan<byte> span, ref ushort count) {
                    this.att = BitConverter.ToInt32(span.Slice(count, span.Length - count));
                    count += sizeof(int);
                }

                public bool Write(Span<byte> span, ref ushort count) {
                    bool success = true;
                    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.att);
                    count += sizeof(int);
                    return success;
                }
            }

            public List<Attribute> attributes = new List<Attribute>();

            public void Read(ReadOnlySpan<byte> span, ref ushort count) {
                this.id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
                count += sizeof(int);
                this.level = BitConverter.ToInt16(span.Slice(count, span.Length - count));
                count += sizeof(short);
                this.duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
                count += sizeof(float);
                this.attributes.Clear();
                ushort attributeLength = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
                count += sizeof(ushort);

                for (int i = 0; i < attributeLength; i++) {
                    Attribute attribute = new Attribute();
                    attribute.Read(span, ref count);
                    attributes.Add(attribute);
                }
            }

            public bool Write(Span<byte> span, ref ushort count) {
                bool success = true;
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.duration);
                count += sizeof(float);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.attributes.Count);
                count += sizeof(ushort);

                foreach (Attribute attribute in this.attributes) {
                    success &= attribute.Write(span, ref count);
                }
                return success;
            }
        }

        public List<Skill> skills = new List<Skill>();

        public void Read(ArraySegment<byte> seg) {
            ushort count = 0;

            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);

            this.testByte = (byte)seg.Array[seg.Offset + count];
            count += sizeof(byte);
            this.playerId = BitConverter.ToInt64(span.Slice(count, span.Length - count));
            count += sizeof(long);
            ushort nameLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
            count += nameLen;
            this.skills.Clear();
            ushort skillLength = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLength; i++) {
                Skill skill = new Skill();
                skill.Read(span, ref count);
                skills.Add(skill);
            }
        }

        public ArraySegment<byte> Write() {
            ArraySegment<byte> seg = SendBufferHelper.Open(4096);
            ushort count = 0;
            bool success = true;

            Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.PlayerInfoReq);
            count += sizeof(ushort);

            seg.Array[seg.Offset + count] = (byte)this.testByte;
            count += sizeof(byte);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
            count += sizeof(long);
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, seg.Array, seg.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.skills.Count);
            count += sizeof(ushort);

            foreach (Skill skill in this.skills) {
                success &= skill.Write(span, ref count);
            }
            success &= BitConverter.TryWriteBytes(span, count);


            if (success == false) {
                return null;
            }

            return SendBufferHelper.Close(count);
        }
    }

    class ServerSession : Session {
        public override void OnConnected(EndPoint endPoint) {
            Console.WriteLine($"OnConnected: {endPoint}");

            // 직렬화: 메모리 상의 인스턴스를 버퍼에 넣는 작업

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name = "abcd" };

            var skill = new PlayerInfoReq.Skill() { id = 101, level = 3, duration = 3.0f };
            skill.attributes.Add(new PlayerInfoReq.Skill.Attribute() { att = 72 });
            packet.skills.Add(skill);

            packet.skills.Add(new PlayerInfoReq.Skill() { id = 102, level = 2, duration = 4.0f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 103, level = 6, duration = 7.0f });

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
