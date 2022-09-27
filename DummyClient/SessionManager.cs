namespace DummyClient {
    internal class SessionManager {
        static SessionManager _session = new();
        public static SessionManager Instance { get { return _session; } }

        List<ServerSession> _sessions = new();
        object _lock = new();
        private Random _rand = new();

        public void SendForEach() {
            lock (_lock) {
                foreach (ServerSession session in _sessions) {
                    C_Move movePacket = new() {
                        posX = _rand.Next(-50, 50),
                        posY = 0,
                        posZ = _rand.Next(-50, 50)
                    };
                    session.Send(movePacket.Write());
                }
            }
        }

        public ServerSession Generate() {
            lock (_lock) {
                ServerSession session = new();
                _sessions.Add(session);
                return session;
            }
        }
    }
}
