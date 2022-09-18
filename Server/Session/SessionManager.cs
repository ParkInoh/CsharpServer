namespace Server {
    internal class SessionManager {
        #region Singleton
        static SessionManager _session = new();
        public static SessionManager Instance { get { return _session; } }
        #endregion

        int _sessionId = 0;
        Dictionary<int, ClientSession> _sessions = new();
        object _lock = new();

        public ClientSession Generate() {
            lock (_lock) {
                int sessionId = ++_sessionId;

                ClientSession session = new();
                session.SessionId = sessionId;
                _sessions.Add(sessionId, session);

                Console.WriteLine($"Connected: {sessionId}");
                return session;
            }
        }

        public ClientSession Find(int id) {
            lock (_lock) {
                ClientSession session = null;
                _sessions.TryGetValue(id, out session);
                return session;
            }
        }

        public void Remove(ClientSession session) {
            lock (_lock) {
                _sessions.Remove(session.SessionId);
            }
        }
    }
}
