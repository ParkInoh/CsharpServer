using ServerCore;

    class PacketManager {
    #region Singleton
    static PacketManager _instance;
    public static PacketManager Instance {
        get {
            if (_instance == null) {
                _instance = new PacketManager();
            }
            return _instance;
        }
    }
    #endregion

    // protocolId, 작업(Action)
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    public void Register() {
        _onRecv.Add((ushort)PacketID.C_PlayerInfoReq, MakePacket<C_PlayerInfoReq>);
        _handler.Add((ushort)PacketID.C_PlayerInfoReq, PacketHandler.C_PlayerInfoReqHandler);

    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer) {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(packetId, out action)) {
            action.Invoke(session, buffer);
        }
    }

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() {
        T packet = new T();
        packet.Read(buffer);

        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action)) {
            action.Invoke(session, packet);
        }
    }
}
