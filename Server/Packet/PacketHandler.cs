using Server;
using ServerCore;

class PacketHandler {
    public static void C_ChatHandler(PacketSession session, IPacket packet) {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null) {
            return;
        }

        // Action 형식으로 변경
        // Action이기에 null을 참조할 수 있기에 참조자를 바꿈
        GameRoom room = clientSession.Room;
        room.Push(() => room.Broadcast(clientSession, chatPacket.chat));
    }
}
