using ServerCore;

namespace Server {
    internal class PacketHandler {
        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet) {
            PlayerInfoReq req = packet as PlayerInfoReq;

            Console.WriteLine($"PlayerInfoReq - playerId: {req.playerId}, name: {req.name}");

            foreach (PlayerInfoReq.Skill skill in req.skills) {
                Console.WriteLine($"Skill - id: {skill.id}, level: {skill.level}, duration: {skill.duration}");
            }
        }
    }
}
