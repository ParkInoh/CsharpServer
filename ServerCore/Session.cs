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

        // 전송하는 것을 큐로 저장
        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _pending = false;
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        // 소켓을 인자로 받아 초기화 및 실행
        public void Start(Socket socket) {
            _socket = socket;

            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            // 버퍼 설정
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(recvArgs);
        }

        // 보내기
        public void Send(byte[] sendBuff) {
            lock (_lock) {
                _sendQueue.Enqueue(sendBuff);
                if (_pending == false) {
                    RegisterSend();
                }
            }
            
            //SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
            //// 버퍼 설정
            //_sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);
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

        #region 네트워크 통신
        // 보내는 시점이 정해져 있지 않음
        private void RegisterSend() {
            _pending = true;
            byte[] buff = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buff, 0, buff.Length);

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false) {
                OnSendCompleted(null, _sendArgs);
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs args) {
            // 콜백 방식으로 동작하기에 lock이 필요하다.
            lock (_lock) {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) {
                    try {
                        // 만약 처리중 큐에 다른게 생겼다면 처리한다.
                        if (_sendQueue.Count > 0) {
                            RegisterSend();
                        }
                        else {
                            // 아니라면 pending을 false로 한다.
                            _pending = false;
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine($"OnSendCompleted failed: {e}");
                    }
                }
                else {
                    Disconnect();
                }
            }
        }


        // 리스너와 같은 방식
        private void RegisterRecv(SocketAsyncEventArgs args) {
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false) {
                OnRecvCompleted(null, args);
            }
        }

        // 받기
        // 현재 구현의 SocketAsyncEventArgs가 하나이므로 리시버가 두번 호출될 수 없기에 안전
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
                Disconnect();
            }
        }
        #endregion
    }
}
