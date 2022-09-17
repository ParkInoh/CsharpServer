﻿using ServerCore;
using System.Net;
using System.Text;

namespace Server {
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
