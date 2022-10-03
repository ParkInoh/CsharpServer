using System.Collections;
using System.Collections.Generic;

public class PacketQueue {
    public static PacketQueue Instance { get; } = new();
    private Queue<IPacket> _packetQueue = new();
    private object _lock = new();

    public void Push(IPacket packet) {
        lock (_lock) {
            _packetQueue.Enqueue(packet);
        }
    }

    // 패킷 하나 작업
    public IPacket Pop() {
        lock (_lock) {
            if (_packetQueue.Count == 0) {
                return null;
            }

            return _packetQueue.Dequeue();
        }
    }

    // 모든 패킷 작업
    public List<IPacket> PopAll() {
        List<IPacket> list = new();

        lock (_lock) {
            while (_packetQueue.Count > 0) {
                list.Add(_packetQueue.Dequeue());
            }
        }

        return list;
    }
}
