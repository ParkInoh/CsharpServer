using DummyClient;
using ServerCore;

class PacketHandler {
    public static void S_ChatHandler(PacketSession session, IPacket packet) {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        // 너무 많으면 보기 힘드니 1번만 출력하도록 함
        //if (chatPacket.playerId == 1) {
        //    Console.WriteLine(chatPacket.chat);
        //}
    }
}
