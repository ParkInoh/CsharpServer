using ServerCore;

class PacketHandler {
    public static void S_TestHandler(PacketSession session, IPacket packet) {
        S_Test req = packet as S_Test;

    }
}
