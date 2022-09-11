﻿using System;
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
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        // 소켓을 인자로 받아 초기화 및 실행
        public void Start(Socket socket) {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            // 버퍼 설정
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        // 보내기
        public void Send(byte[] sendBuff) {
            lock (_lock) {
                _sendQueue.Enqueue(sendBuff);
                // _pendingList가 비었다면 버퍼를 Register
                if (_pendingList.Count == 0) {
                    RegisterSend();
                }
            }
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
            // RegisterSend 내부로 들어왔을 때는 _pendingList.Count가 0인 상태이다.
            while (_sendQueue.Count > 0) {
                byte[] buff = _sendQueue.Dequeue();
                // 실제 보낼 데이터가 있는 버퍼
                // ArraySegment: 특정 배열 일부(구조체)
                // C++과 달리 포인터가 없기에 오프셋을 전달한다.
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }

            _sendArgs.BufferList = _pendingList;

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
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        Console.WriteLine($"Transferred bytes: {_sendArgs.BytesTransferred}");

                        // 만약 처리중 큐에 다른게 생겼다면 또 처리한다.
                        if (_sendQueue.Count > 0) {
                            RegisterSend();
                            // 만약 RegisterSend를 했다면 _pendingList를 Clear했다는 뜻
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
        private void RegisterRecv() {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false) {
                OnRecvCompleted(null, _recvArgs);
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

                    RegisterRecv();
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
