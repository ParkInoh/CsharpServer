using System;

namespace ServerCore {
    public class RecvBuffer {
        // 버퍼와 읽기/쓰기 시작지점
        private ArraySegment<byte> _buffer;
        private int _readPos;
        private int _writePos;

        public RecvBuffer(int bufferSize) {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);

        }

        // 데이터 범위: writePos - readPos
        public int DataSize { get { return _writePos - _readPos; } }

        // 남은 공간: 전체 - writePos
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        // 데이터 범위를 ArraySegment로 반환
        public ArraySegment<byte> ReadSegment => new(_buffer.Array, _buffer.Offset + _readPos, DataSize);

        // 데이터를 받을 때 쓸 범위를 반환
        public ArraySegment<byte> WriteSegment => new(_buffer.Array, _buffer.Offset + _writePos, FreeSize);

        // readPos, writePos를 주기적으로 초기화함
        public void Clean() {
            int dataSize = DataSize;

            // 데이터가 없는 경우
            if (dataSize == 0) {
                _readPos = _writePos = 0;
            }
            // 남은 데이터가 있는 경우 복사해야 함
            else {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        // 데이터를 제대로 읽었으면 pos 이동 후 true 반환
        public bool OnRead(int numOfBytes) {
            // 데이터 크기보다 바이트가 크면 비정상적인 상황
            if (numOfBytes > DataSize) {
                return false;
            }
            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes) {
            // 남은 공간보다 바이트가 크면 비정상적인 상황
            if (numOfBytes > FreeSize) {
                return false;
            }
            _writePos += numOfBytes;
            return true;
        }
    }
}
