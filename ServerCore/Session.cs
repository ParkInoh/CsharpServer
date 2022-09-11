using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore {
    internal class Session {
        Socket _socket;
        int disconnected = 0;

        // 소켓을 인자로 받아 초기화 및 실행
        public void Start(Socket socket) {
            _socket = socket;

            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            // 버퍼 설정
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            RegisterRecv(recvArgs);
        }

        public void Send(byte[] sendBuff) {
            // 보내기
            _socket.Send(sendBuff);
        }

        public void Disconnect() {
            // 멀티쓰레드 환경에서 여러번 호출될 수 있다.
            if (Interlocked.Exchange(ref disconnected, 1) == 1) {
                // 값이 1이라면 이미 변경된 것이므로 수행 중단
                return;
            }
            // 연결 중단
            // 연결이 중단됨을 미리 알림
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신: private
        // 리스너와 같은 방식
        private void RegisterRecv(SocketAsyncEventArgs args) {
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false) {
                OnRecvCompleted(null, args);
            }
        }

        // 받기
        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args) {
            // 상대가 연결을 중단하는 등의 이유로 받은 바이트가 0일 수 있다.
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) {
                try {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From client] {recvData}");

                    RegisterRecv(args);
                }
                catch (Exception e) {
                    Console.WriteLine($"OnRecvCompleted failed: {e}");
                }
            }
            else {

            }
        }
        #endregion
    }
}
