using System;
using System.Threading;

namespace ServerCore {
    // RecvBuffer와 달리, 보낼 때는 버퍼를 외부에 갖도록 한다.
    // RecvBuffer는 Session마다 각자 버퍼를 가지고 있다.
    // 만약 SendBuffer가 RecvBuffer와 같은 방식을 취한다면
    // 여러 사용자가 Send를 할 때마다 값을 복사해서 각자 전송해야 한다.
    // 외부에서 Send를 한다면 값을 한번 복사하고 모든 사용자에게 보낼 수 있다.

    // SendBuffer의 버퍼가 고정 크기라면 남는 공간이 있어 비효율적일 수 있다.
    // 이 부분은 RecvBuffer와 같이 일부를 잘라 사용하는 방식으로 한다.

    // 다른 Session이 SendQueue에 넣은 작업이 있을 수 있으므로,
    // SendBuffer의 경우에는 버퍼를 Clean하지 않는다.

    public class SendBufferHelper {
        // 쓰레드 간의 race condition을 방지하기 위해 ThreadLocal을 사용한다.
        // ThreadLocal은 전역이지만 자신의 쓰레드에서만 접근한다(안전).
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 65535 * 100;

        // ThreadLocal의 버퍼를 관리
        public static ArraySegment<byte> Open(int reserveSize) {
            // 현재 버퍼가 null이라면 생성
            if (CurrentBuffer.Value == null) {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }

            // 현재 버퍼의 빈 공간이 예상 크기보다 작다면 새로 생성
            if (CurrentBuffer.Value.FreeSize < reserveSize) {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }

            // 반드시 빈 공간이 있는 상태
            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize) {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer {
        private byte[] _buffer;
        private int _usedSize = 0;

        // 남은 공간
        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int chunkSize) {
            _buffer = new byte[chunkSize];
        }

        // 예상 크기를 지정
        public ArraySegment<byte> Open(int reserveSize) {
            // 빈 공간이 부족한 경우
            if (reserveSize > FreeSize) {
                return new ArraySegment<byte>(null);
            }

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize) {
            // 실제 사용한 공간
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
        // 실제 사용한 공간만큼 늘림
            _usedSize += usedSize;
            return segment;
        }
    }
}
