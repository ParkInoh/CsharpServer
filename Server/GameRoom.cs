namespace Server {
    internal class GameRoom {
        List<ClientSession> _sessions = new();
        object _lock = new();

        public void Broadcast(ClientSession session, string chat) {
            S_Chat packet = new();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> seg = packet.Write();
            
            // 공유 변수 _sessions를 다루기에 lock
            lock (_lock) {
                foreach (ClientSession s in _sessions) {
                    s.Send(seg);
                }
            }
        }

        public void Enter(ClientSession session) {
            lock (_lock) {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session) {
            lock (_lock) {
                _sessions.Remove(session);
            }
        }
    }
}
