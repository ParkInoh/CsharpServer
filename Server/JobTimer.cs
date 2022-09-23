using ServerCore;

namespace Server {
    struct JobTimerElem : IComparable<JobTimerElem> {
        // 실행되어야 할 시간
        public int execTick;
        public Action action;

        // 작은 값이 나와야 함
        public int CompareTo(JobTimerElem other) {
            return other.execTick - execTick;
        }
    }

    internal class JobTimer {
        PriorityQueue<JobTimerElem> _pq = new();
        object _lock = new();

        public static JobTimer Instance { get; } = new();

        // 수행할 Action과 몇tick 뒤에 수행하는지 나타내는 tickAfter
        public void Push(Action action, int tickAfter = 0) {
            JobTimerElem job;
            job.action = action;
            job.execTick = System.Environment.TickCount + tickAfter;

            lock (_lock) {
                _pq.Push(job);
            }
        }

        public void Flush() {
            while (true) {
                int now = System.Environment.TickCount;
                JobTimerElem job;

                lock (_lock) {
                    // job이 queue에 없으면 끝냄
                    if (_pq.Count == 0) {
                        break;
                    }

                    job = _pq.Peek();
                    // 다음 job의 시간이 아직 아니면 끝냄
                    if (job.execTick > now) {
                        break;
                    }

                    // 위에 해당하지 않으면 실행시켜야 함
                    _pq.Pop();
                }
                job.action.Invoke();
            }
        }
    }
}
