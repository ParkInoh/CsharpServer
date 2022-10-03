using Server;
using ServerCore;

class PacketHandler {
    public static void C_LeaveGameHandler(PacketSession session, IPacket packet) {
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null) {
            return;
        }

        // Action 형식으로 변경
        // Action이기에 null을 참조할 수 있기에 참조자를 바꿈
        GameRoom room = clientSession.Room;
        room.Push(() => room.Leave(clientSession));
    }
    
    public static void C_MoveHandler(PacketSession session, IPacket packet) {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null) {
            return;
        }

        //Console.WriteLine($"{movePacket.posX}, {movePacket.posY}, {movePacket.posZ}");

        // Action 형식으로 변경
        // Action이기에 null을 참조할 수 있기에 참조자를 바꿈
        GameRoom room = clientSession.Room;
        room.Push(() => room.Move(clientSession, movePacket));
    }
}
