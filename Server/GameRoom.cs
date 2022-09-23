using ServerCore;

namespace Server {
    internal class GameRoom : IJobQueue {
        List<ClientSession> _sessions = new();
        JobQueue _jobQueue = new();

        // JobQueue는 쓰레드 하나만 Flush를 실행함을 보장함.
        // 즉 내부에서 락을 잡기에, 여기서는 락을 잡을 필요가 없음.
        // Broadcast, Enter, Leave를 Action으로 넘겨주는 형식으로 변경
        public void Push(Action job) {
            _jobQueue.Push(job);
        }

        public void Broadcast(ClientSession session, string chat) {
            S_Chat packet = new();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> seg = packet.Write();
            
            // O(N^2) 이다. 왜냐면 각자 모든사람에게 보내기 때문.
            // Send 패킷을 모아서 보내면 줄일 수 있다.
            foreach (ClientSession s in _sessions) {
                s.Send(seg);
            }
        }

        public void Enter(ClientSession session) {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session) {
            _sessions.Remove(session);
        }
    }
}
