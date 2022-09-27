using DummyClient;
using ServerCore;
using UnityEngine;

class PacketHandler {
    public static void S_ChatHandler(PacketSession session, IPacket packet) {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        //if (chatPacket.playerId == 1) {
            Debug.Log(chatPacket.chat);
            
            // 메인 쓰레드에서 동작해야 함
            GameObject gameObject = GameObject.Find("Player");
            if (gameObject == null) {
                Debug.Log("Player not found.");
            }
            else {
                Debug.Log("Player found.");
            }
        //}
    }
}
