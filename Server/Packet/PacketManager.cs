using ServerCore;

namespace Server {
    internal class PacketManager {
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
            // 패킷을 만드는 것을 등록
            _onRecv.Add((ushort)PacketID.PlayerInfoReq, MakePacket<PlayerInfoReq>);
            // 핸들러를 등록
            _handler.Add((ushort)PacketID.PlayerInfoReq, PacketHandler.PlayerInfoReqHandler);
        }

        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer) {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            // switch-case문 대신 Dictionary로 동작하도록 함
            // 실제로 핸들러 등록이 되어있다면 Invoke
            Action<PacketSession, ArraySegment<byte>> action = null;
            if (_onRecv.TryGetValue(packetId, out action)) {
                action.Invoke(session, buffer);
            }
        }

        // 제네릭 T는 IPacket을 상속하고 new 가 가능해야 함
        void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() {
            T packet = new T();
            packet.Read(buffer);

            // 핸들러 호출
            Action<PacketSession, IPacket> action = null;
            // 프로토콜을 참조해서 PacketHandler의 코드를 action으로 추출함. 있다면 Invoke
            if (_handler.TryGetValue(packet.Protocol, out action)) {
                action.Invoke(session, packet);
            }
        }
    }
}
