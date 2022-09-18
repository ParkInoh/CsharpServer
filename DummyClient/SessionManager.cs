namespace DummyClient {
    internal class SessionManager {
        static SessionManager _session = new();
        public static SessionManager Instance { get { return _session; } }

        List<ServerSession> _sessions = new();
        object _lock = new();

        public void SendForEach() {
            lock (_lock) {
                foreach (ServerSession session in _sessions) {
                    C_Chat chatPacket = new();
                    chatPacket.chat = $"Hello Server!";
                    ArraySegment<byte> seg = chatPacket.Write();

                    session.Send(seg);
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
