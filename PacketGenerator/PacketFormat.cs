namespace PacketGenerator {
    internal class PacketFormat {

        // 파일 전체에 대한 포맷
        // {0}: 패킷 이름, 번호 목록
        // {1}: 패킷 목록
        public static string fileFormat =
@"using ServerCore;
using System.Net;
using System.Text;

public enum PacketID {{
    {0}
}}

interface IPacket {{
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
@"class {0} : IPacket {{
    {1}

    public ushort Protocol {{
		get {{ return (ushort)PacketID.{0}; }}
	}}

    public void Read(ArraySegment<byte> seg) {{
        ushort count = 0;
        
        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        {2}
    }}

    public ArraySegment<byte> Write() {{
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);

        {3}
        success &= BitConverter.TryWriteBytes(span, count);


        if (success == false) {{
            return null;
        }}

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

    public void Read(ReadOnlySpan<byte> span, ref ushort count) {{
        {3}
    }}

    public bool Write(Span<byte> span, ref ushort count) {{
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
@"this.{0} = BitConverter.{1}(span.Slice(count, span.Length - count));
count += sizeof({2});";

        // {0}: 변수명
        // {1}: 변수 타입
        public static string readByteFormat =
@"this.{0} = ({1})seg.Array[seg.Offset + count];
count += sizeof({1});";

        // {0}: 변수명
        public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(span.Slice(count, {0}Len));
count += {0}Len;";

        // {0}: 리스트명 대문자(struct)
        // {1}: 리스트명 소문자(instance)
        public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Length = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);

for (int i = 0; i < {1}Length; i++) {{
    {0} {1} = new {0}();
    {1}.Read(span, ref count);
    {1}s.Add({1});
}}";

        // {0}: 변수명
        // {1}: 변수 타입
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.{0});
count += sizeof({1});";

        // {0}: 변수명
        // {1}: 변수 타입
        public static string writeByteFormat =
@"seg.Array[seg.Offset + count] = (byte)this.{0};
count += sizeof({1});";

        // {0}: 변수명
        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, seg.Array, seg.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";

        // {0}: 리스트명 대문자(struct)
        // {1}: 리스트명 소문자(instance)
        public static string writeListFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.{1}s.Count);
count += sizeof(ushort);

foreach ({0} {1} in this.{1}s) {{
    success &= {1}.Write(span, ref count);
}}";

    }
}
