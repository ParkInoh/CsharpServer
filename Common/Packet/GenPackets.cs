using ServerCore;
using System.Net;
using System.Text;

public enum PacketID {
    C_Chat = 1,
	S_Chat = 2,
	
}

interface IPacket {
	ushort Protocol { get; }
	void Read(ArraySegment<byte> seg);
	ArraySegment<byte> Write();
}

class C_Chat : IPacket {
    public string chat;

    public ushort Protocol {
		get { return (ushort)PacketID.C_Chat; }
	}

    public void Read(ArraySegment<byte> seg) {
        ushort count = 0;
        
        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        ushort chatLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(span.Slice(count, chatLen));
		count += chatLen;
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.C_Chat);
        count += sizeof(ushort);

        ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, seg.Array, seg.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;
        success &= BitConverter.TryWriteBytes(span, count);


        if (success == false) {
            return null;
        }

        return SendBufferHelper.Close(count);
    }
}
class S_Chat : IPacket {
    public int playerId;
	public string chat;

    public ushort Protocol {
		get { return (ushort)PacketID.S_Chat; }
	}

    public void Read(ArraySegment<byte> seg) {
        ushort count = 0;
        
        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(span.Slice(count, span.Length - count));
		count += sizeof(int);
		ushort chatLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(span.Slice(count, chatLen));
		count += chatLen;
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.S_Chat);
        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
		count += sizeof(int);
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, seg.Array, seg.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;
        success &= BitConverter.TryWriteBytes(span, count);


        if (success == false) {
            return null;
        }

        return SendBufferHelper.Close(count);
    }
}

