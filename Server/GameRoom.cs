using ServerCore;

namespace Server {
    internal class GameRoom : IJobQueue {
        List<ClientSession> _sessions = new();
        JobQueue _jobQueue = new();
        List<ArraySegment<byte>> _pendingList = new();

        // JobQueue는 쓰레드 하나만 Flush를 실행함을 보장함.
        // 즉 내부에서 락을 잡기에, 여기서는 락을 잡을 필요가 없음.
        // Broadcast, Enter, Leave를 Action으로 넘겨주는 형식으로 변경
        public void Push(Action job) {
            _jobQueue.Push(job);
        }

        public void Flush() {
            foreach (ClientSession s in _sessions) {
                s.Send(_pendingList);
            }

            //Console.WriteLine($"Flushed {_pendingList.Count} items.");
            _pendingList.Clear();
        }

        // ArraySegment를 받아서 바로 리스트에 추가하는 형식으로 변경
        public void Broadcast(ArraySegment<byte> segment) {
            _pendingList.Add(segment);
        }

        public void Enter(ClientSession session) {
            // 플레이어 추가
            _sessions.Add(session);
            session.Room = this;
            
            // 새로 온 사람에게 모든 플레이어 목록 전송
            S_PlayerList playersList = new();
            foreach (ClientSession s in _sessions) {
                playersList.players.Add(new S_PlayerList.Player {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                });
            }
            session.Send(playersList.Write());

            // 새로 온 사람 정보를 모두에게 알림
            S_BroadcastEnterGame enter = new() {
                playerId = session.SessionId,
                posX = 0f,
                posY = 0f,
                posZ = 0f
            };
            Broadcast(enter.Write());
        }

        public void Leave(ClientSession session) {
            // 플레이어 제거
            _sessions.Remove(session);
            
            // 플레이어 제거됨을 모두에게 알림
            S_BroadcastLeaveGame leave = new() {
                playerId = session.SessionId
            };
            Broadcast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet) {
            // 좌표 이동
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            // 모두에게 알림
            S_BroadcastMove move = new() {
                playerId = session.SessionId,
                posX = session.PosX,
                posY = session.PosY,
                posZ = session.PosZ
            };
            Broadcast(move.Write());
        }
    }
}
