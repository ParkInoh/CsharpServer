using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using DummyClient;
using ServerCore;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    private ServerSession _session = new();

    public void Send(ArraySegment<byte> sendBuff) {
        _session.Send(sendBuff);
    }
    
    void Start() {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        // 커넥터를 사용하도록 연결 변경
        Connector connector = new();
        connector.Connect(endPoint, () => _session);
    }

    void Update() {
        // 각 프레임마다 들어온 모든 패킷을 작업 
        List<IPacket> list = PacketQueue.Instance.PopAll();
        foreach (IPacket packet in list) {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }
}
