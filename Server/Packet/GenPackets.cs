using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

public enum PacketID {
    S_BroadcastEnterGame = 1,
	C_LeaveGame = 2,
	S_BroadcastLeaveGame = 3,
	S_PlayerList = 4,
	C_Move = 5,
	S_BroadcastMove = 6,
	
}

public interface IPacket {
	ushort Protocol { get; }
	void Read(ArraySegment<byte> seg);
	ArraySegment<byte> Write();
}

public class S_BroadcastEnterGame : IPacket {
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol {
		get { return (ushort)PacketID.S_BroadcastEnterGame; }
	}

    public void Read(ArraySegment<byte> seg) {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(seg.Array, seg.Offset + count);
		count += sizeof(int);
		this.posX = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posY = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posZ = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastEnterGame), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(playerId), 0, seg.Array, seg.Offset + count, sizeof(int));
		count += sizeof(int);
		Array.Copy(BitConverter.GetBytes(posX), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(posY), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(posZ), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}
public class C_LeaveGame : IPacket {
    

    public ushort Protocol {
		get { return (ushort)PacketID.C_LeaveGame; }
	}

    public void Read(ArraySegment<byte> seg) {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_LeaveGame), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        
        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}
public class S_BroadcastLeaveGame : IPacket {
    public int playerId;

    public ushort Protocol {
		get { return (ushort)PacketID.S_BroadcastLeaveGame; }
	}

    public void Read(ArraySegment<byte> seg) {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(seg.Array, seg.Offset + count);
		count += sizeof(int);
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastLeaveGame), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(playerId), 0, seg.Array, seg.Offset + count, sizeof(int));
		count += sizeof(int);
        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}
public class S_PlayerList : IPacket {
    public class Player {
	    public bool isSelf;
		public int playerId;
		public float posX;
		public float posY;
		public float posZ;
	
	    public void Read(ArraySegment<byte> seg, ref ushort count) {
	        this.isSelf = BitConverter.ToBoolean(seg.Array, seg.Offset + count);
			count += sizeof(bool);
			this.playerId = BitConverter.ToInt32(seg.Array, seg.Offset + count);
			count += sizeof(int);
			this.posX = BitConverter.ToSingle(seg.Array, seg.Offset + count);
			count += sizeof(float);
			this.posY = BitConverter.ToSingle(seg.Array, seg.Offset + count);
			count += sizeof(float);
			this.posZ = BitConverter.ToSingle(seg.Array, seg.Offset + count);
			count += sizeof(float);
	    }
	
	    public bool Write(ArraySegment<byte> seg, ref ushort count) {
	        bool success = true; 
	        Array.Copy(BitConverter.GetBytes(isSelf), 0, seg.Array, seg.Offset + count, sizeof(bool));
			count += sizeof(bool);
			Array.Copy(BitConverter.GetBytes(playerId), 0, seg.Array, seg.Offset + count, sizeof(int));
			count += sizeof(int);
			Array.Copy(BitConverter.GetBytes(posX), 0, seg.Array, seg.Offset + count, sizeof(float));
			count += sizeof(float);
			Array.Copy(BitConverter.GetBytes(posY), 0, seg.Array, seg.Offset + count, sizeof(float));
			count += sizeof(float);
			Array.Copy(BitConverter.GetBytes(posZ), 0, seg.Array, seg.Offset + count, sizeof(float));
			count += sizeof(float);
	        return success;
	    }
	}
	
	public List<Player> players = new();

    public ushort Protocol {
		get { return (ushort)PacketID.S_PlayerList; }
	}

    public void Read(ArraySegment<byte> seg) {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.players.Clear();
		ushort playerLength = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
		count += sizeof(ushort);
		
		for (int i = 0; i < playerLength; i++) {
		    Player player = new();
		    player.Read(seg, ref count);
		    players.Add(player);
		}
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_PlayerList), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)this.players.Count), 0, seg.Array, seg.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		
		foreach (Player player in this.players) {
		    player.Write(seg, ref count);
		}
        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}
public class C_Move : IPacket {
    public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol {
		get { return (ushort)PacketID.C_Move; }
	}

    public void Read(ArraySegment<byte> seg) {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.posX = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posY = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posZ = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_Move), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(posX), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(posY), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(posZ), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}
public class S_BroadcastMove : IPacket {
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol {
		get { return (ushort)PacketID.S_BroadcastMove; }
	}

    public void Read(ArraySegment<byte> seg) {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(seg.Array, seg.Offset + count);
		count += sizeof(int);
		this.posX = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posY = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posZ = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
    }

    public ArraySegment<byte> Write() {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastMove), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(playerId), 0, seg.Array, seg.Offset + count, sizeof(int));
		count += sizeof(int);
		Array.Copy(BitConverter.GetBytes(posX), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(posY), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(posZ), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

