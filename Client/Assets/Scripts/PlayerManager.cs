using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager {
    // 내 플레이어는 하나, 다른 플레이어들은 playerId, Player 형식으로 저장
    private MyPlayer _myPlayer;
    private Dictionary<int, Player> _players = new();

    public static PlayerManager Instance { get; } = new();

    // 접속 시 플레이어 리스트 받아서 추가
    public void Add(S_PlayerList packet) {
        Object obj = Resources.Load("Player");
        
        foreach (var p in packet.players) {
            GameObject go = Object.Instantiate(obj) as GameObject;
            if (p.isSelf) {  // 내 플레이어 오브젝트라면 _myPlayer에 등록
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.PlayerId = p.playerId;
                myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _myPlayer = myPlayer;
            }
            else {  // 아니라면 플레이어 리스트에 등록
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.playerId;
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _players.Add(p.playerId, player);
            }
        }
    }

    // 나는 접속해있는데 누군가가 새로 접속(내가 아님)
    public void EnterGame(S_BroadcastEnterGame packet) {
        // 내 패킷이라면 위치 갱신 X(아니면 두번 처리하게 됨)
        if (packet.playerId == _myPlayer.PlayerId) {
            return;
        }
        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;
        
        Player player = go.AddComponent<Player>();
        player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
        _players.Add(packet.playerId, player);
    }
    
    // 누군가가 접속 해제(내가 나갈수도 있음)
    public void LeaveGame(S_BroadcastLeaveGame packet) {
        if (_myPlayer.PlayerId == packet.playerId) {
            Object.Destroy(_myPlayer.gameObject);
            _myPlayer = null;
        }
        else {
            // playerId가 존재하는지 확인. 존재하면 player 객체 받음
            if (_players.TryGetValue(packet.playerId, out var player)) {
                Object.Destroy(player.gameObject);
                _players.Remove(packet.playerId);
            }
        }
    }
    
    public void Move(S_BroadcastMove packet) {
        // 내가 이동하는지, 다른 사람이 이동하는지 구분
        if (_myPlayer.PlayerId == packet.playerId) {
            _myPlayer.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
        }
        else {
            // playerId가 존재하는지 확인. 존재하면 player 객체 받음
            if (_players.TryGetValue(packet.playerId, out var player)) {
                player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
            }
        }
    }
}
