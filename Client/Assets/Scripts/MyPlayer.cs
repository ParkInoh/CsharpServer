using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : Player {
    private NetworkManager _networkManager;
    
    void Start()
    {
        StartCoroutine(nameof(SendPacketCo));
        _networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }
    
    IEnumerator SendPacketCo() {
        while (true) {
            yield return new WaitForSeconds(.25f);

            C_Move movePacket = new() {
                posX = Random.Range(-50, 50),
                posY = 0,
                posZ = Random.Range(-50, 50)
            };
            
            // 매니저에서 전송하도록 함
            _networkManager.Send(movePacket.Write());
        }
    }
}
