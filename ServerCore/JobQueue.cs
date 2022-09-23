namespace ServerCore {
    public interface IJobQueue {
        void Push(Action job);
    }

    public class JobQueue : IJobQueue {
        Queue<Action> _jobQueue = new();
        object _lock = new();
        bool _flush = false;

        public void Push(Action job) {
            // Flush 작업이 필요한지
            bool flush = false;

            lock (_lock) {
                _jobQueue.Enqueue(job);
                if (_flush == false) {
                    flush = _flush = true;
                }
            }

            if (flush) {
                Flush();
            }
        }

        // 락이 잡혀있으므로 쓰레드 하나만 Flush를 실행함을 보장함.
        // 큐에 작업이 있으면 반복수행
        void Flush() {
            while (true) {
                Action action = Pop();
                if (action == null) {
                    return;
                }
                action.Invoke();
            }
        }

        // 큐에 작업이 들어가는 상황도 고려해서 락 잡음
        Action Pop() {
            lock (_lock) {
                if (_jobQueue.Count == 0) {
                    // 작업 끝
                    _flush = false;
                    return null;
                }
                return _jobQueue.Dequeue();
            }
        }
    }
}
