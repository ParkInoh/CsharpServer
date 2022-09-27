namespace PacketGenerator {
    internal class PacketFormat {
        // {0}: 패킷 등록
        public static string manangerFormat =
@"using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager {{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance {{ get {{ return _instance; }} }}
    #endregion

    public PacketManager() {{
        Register();
    }}
    
    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc =
        new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler =
        new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register() {{
{0}
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null) {{
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (_makeFunc.TryGetValue(packetId, out func)) {{
            IPacket packet = func.Invoke(session, buffer);
            if (onRecvCallback != null) {{
                onRecvCallback.Invoke(session, packet);
            }}
            else {{
                HandlePacket(session, packet);
            }}
        }}
    }}

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() {{
        T packet = new T();
        packet.Read(buffer);
        return packet;
    }}

    public void HandlePacket(PacketSession session, IPacket packet) {{
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action)) {{
            action.Invoke(session, packet);
        }}
    }}
}}
";

        // {0}: 패킷 이름
        public static string managerRegisterFormat =
@"        _makeFunc.Add((ushort)PacketID.{0}, MakePacket<{0}>);
        _handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);";

        // 파일 전체에 대한 포맷
        // {0}: 패킷 이름, 번호 목록
        // {1}: 패킷 목록
        public static string fileFormat =
@"using ServerCore;
using System;
using System.Text;

public enum PacketID {{
    {0}
}}

public interface IPacket {{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> seg);
	ArraySegment<byte> Write();
}}

{1}
";
        // {0}: 패킷 이름
        // {1}: 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";

        // 바뀌는 부분을 {}로 감싸 정한다.
        // 일반 { 는 {{ 로 하나씩 더 붙여줘야 한다.
        // {0}: 패킷 이름
        // {1}: 멤버 변수
        // {2}: 멤버 변수 Read
        // {3}: 멤버 변수 Write
        public static string packetFormat =
@"public class {0} : IPacket {{
    {1}

    public ushort Protocol {{
		get {{ return (ushort)PacketID.{0}; }}
	}}

    public void Read(ArraySegment<byte> seg) {{
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        {2}
    }}

    public ArraySegment<byte> Write() {{
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.{0}), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        {3}
        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }}
}}
";

        // {0}: 변수 타입
        // {1}: 변수명
        public static string memberFormat =
@"public {0} {1};";

        // {0}: 리스트명 대문자(class)
        // {1}: 리스트명 소문자(instance)
        // {2}: 멤버 변수들
        // {3}: 멤버 변수 Read
        // {4}: 멤버 변수 Write
        public static string memberListFormat =
@"public class {0} {{
    {2}

    public void Read(ArraySegment<byte> seg, ref ushort count) {{
        {3}
    }}

    public bool Write(ArraySegment<byte> seg, ref ushort count) {{
        bool success = true; 
        {4}
        return success;
    }}
}}

public List<{0}> {1}s = new List<{0}>();";

        // {0}: 변수명
        // {1}: BitConverter.To~~ 형식
        // {2}: 변수 타입
        public static string readFormat =
@"this.{0} = BitConverter.{1}(seg.Array, seg.Offset + count);
count += sizeof({2});";

        // {0}: 변수명
        // {1}: 변수 타입
        public static string readByteFormat =
@"this.{0} = ({1})seg.Array[seg.Offset + count];
count += sizeof({1});";

        // {0}: 변수명
        public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(seg.Array, seg.Offset + count, {0}Len);
count += {0}Len;";

        // {0}: 리스트명 대문자(struct)
        // {1}: 리스트명 소문자(instance)
        public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Length = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
count += sizeof(ushort);

for (int i = 0; i < {1}Length; i++) {{
    {0} {1} = new {0}();
    {1}.Read(seg, ref count);
    {1}s.Add({1});
}}";

        // {0}: 변수명
        // {1}: 변수 타입
        public static string writeFormat =
@"Array.Copy(BitConverter.GetBytes({0}), 0, seg.Array, seg.Offset + count, sizeof({1}));
count += sizeof({1});";

        // {0}: 변수명
        // {1}: 변수 타입
        public static string writeByteFormat =
@"seg.Array[seg.Offset + count] = (byte)this.{0};
count += sizeof({1});";

        // {0}: 변수명
        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, seg.Array, seg.Offset + count + sizeof(ushort));
Array.Copy(BitConverter.GetBytes({0}Len), 0, seg.Array, seg.Offset + count, sizeof(ushort));
count += sizeof(ushort);
count += {0}Len;";

        // {0}: 리스트명 대문자(struct)
        // {1}: 리스트명 소문자(instance)
        public static string writeListFormat =
@"Array.Copy(BitConverter.GetBytes((ushort)this.{1}s.Count), 0, seg.Array, seg.Offset + count, sizeof(ushort));
count += sizeof(ushort);

foreach ({0} {1} in this.{1}s) {{
    {1}.Write(seg, ref count);
}}";
    }
}
