using System.Net;
using System.Net.Sockets;

namespace ServerCore {
    // 서버가 Listener의 반대 역할을 하는 Connecter가 있어야 하는 이유
    // 서버가 여럿일 때(예를 들어, 본 서버와 AI 동작 처리 서버 가 분리된 경우),
    // 서버끼리 통신해야 한다.
    // 한쪽은 listen, 다른 쪽은 connect해야 한다.

    public class Connecter {
        Func<Session> _sessionFactory;

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory) {
            // 소켓 생성
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            //
            args.UserToken = socket;
            RegisterConnect(args);
        }

        private void RegisterConnect(SocketAsyncEventArgs args) {
            Socket socket = args.UserToken as Socket;

            if (socket == null) {
                return;
            }

            bool pending = socket.ConnectAsync(args);

            if (pending == false) {
                OnConnectCompleted(null, args);
            }
        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs args) {
            if (args.SocketError == SocketError.Success) {
                Session session = _sessionFactory.Invoke();
                // 세션은 소켓 기반으로 동작하도록 구현됨.
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else {
                Console.WriteLine($"OnConnectCompleted failed: {args.SocketError}");
            }
        }
    }
}
