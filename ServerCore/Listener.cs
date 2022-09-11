using System.Net;
using System.Net.Sockets;

namespace ServerCore {
    public class Listener {
        Socket _listenSocket;
        // Session이 abstract이기에 어떤 Session이 필요한지 알 수 없다.
        // 그래서 Func로 어떤 Session을 만들 것인지를 인자로 받는다.
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory) {
            // 소켓 생성
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            // 소켓에 주소 연동
            _listenSocket.Bind(endPoint);
            // 소켓을 listen 상태로 만듦.
            // backlog: 접속 가능한 최대 연결 수
            _listenSocket.Listen(10);

            // 이벤트를 발생시켜 알려준다.
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            // 델리게이트를 추가한다.
            // RegisterAccept에서 pending이 true라면 콜백으로 호출한다.
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }

        // 소켓 연결(비동기)
        private void RegisterAccept(SocketAsyncEventArgs args) {
            // 이전 연결에서 사용되었을 수 있기에 초기화
            args.AcceptSocket = null;

            // AcceptAsync는 IO 동작이 pending 상태인지를 반환한다.
            bool pending = _listenSocket.AcceptAsync(args);
            // pending이 false라면 바로 끝났다는 것이다.
            if (pending == false) {
                OnAcceptCompleted(null, args);
            }
        }

        // 소켓이 연결되었다면 동작
        // 콜백 함수는 별도의 쓰레드에서 실행된다.
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args) {
            if (args.SocketError == SocketError.Success) {
                // 소켓이 제대로 연결되었다면 동작
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else {
                Console.WriteLine(args.SocketError.ToString());
            }

            // 연결이 모두 끝났으면 다시 register를 반복한다.
            RegisterAccept(args);
        }

        public Socket Accept() {
            // blocking 계열 함수를 사용하면, 진행 중에 멈출 수 있다.
            // 그래서 non-blocking으로 변경해야 멈추지 않고 계속 진행할 수 있다.
            return _listenSocket.Accept();
        }
    }
}
