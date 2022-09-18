using ServerCore;

class PacketHandler {
    public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet) {
        C_PlayerInfoReq req = packet as C_PlayerInfoReq;

        Console.WriteLine($"PlayerInfoReq - playerId: {req.playerId}, name: {req.name}");

        foreach (C_PlayerInfoReq.Skill skill in req.skills) {
            Console.WriteLine($"Skill - id: {skill.id}, level: {skill.level}, duration: {skill.duration}");
        }
    }
}
