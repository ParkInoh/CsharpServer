using System.Xml;

namespace PacketGenerator {
    internal class Program {
        // 실시간으로 관리되는 패킷 문자열
        // 패킷 처리 갯수 기억
        static string genPackets;
        static ushort packetId;
        static string packetEnums;

        static string clientRegister;
        static string serverRegister;

        static void Main(string[] args) {
            string pdlPath = "../../PDL.xml";

            // XmlReader의 옵션 설정
            XmlReaderSettings settings = new XmlReaderSettings() {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            // 실행 시에 args가 존재하면 pdlPath로 간주
            if (args.Length >= 1) {
                pdlPath = args[0];
            }

            // using으로 하면 블록이 끝날때 알아서 Dispose를 호출
            using (XmlReader reader = XmlReader.Create(pdlPath, settings)) {
                // 헤더를 건너뛰고 컨텐츠로 이동
                reader.MoveToContent();

                // stream 형태로 읽음
                while (reader.Read()) {
                    // Depth: 깊이를 지정
                    // XmlNodeType.Element: 시작 원소
                    // 반대로 닫는 원소(/>)는 EndElement
                    if (reader.Depth == 1 && reader.NodeType == XmlNodeType.Element) {
                        ParsePacket(reader);
                    }

                    //Console.WriteLine(reader.Name + " " + reader["name"]);
                }

                // {0}: 패킷 이름, 번호 목록
                // {1}: 패킷 목록
                // 자동 파싱한 결과를 파일로 씀
                string fileText = string.Format(PacketFormat.fileFormat, packetEnums, genPackets);
                File.WriteAllText("GenPackets.cs", fileText);

                // {0}: 패킷 등록 / 클라이언트, 서버 분리
                string clientManagerText = string.Format(PacketFormat.manangerFormat, clientRegister);
                File.WriteAllText("ClientPacketManager.cs", clientManagerText);
                string serverManagerText = string.Format(PacketFormat.manangerFormat, serverRegister);
                File.WriteAllText("ServerPacketManager.cs", serverManagerText);
            }
        }

        public static void ParsePacket(XmlReader reader) {
            // 잘못 온 경우
            if (reader.NodeType == XmlNodeType.EndElement) {
                return;
            }

            // 첫 노드가 packet이 아니면 오류
            if (reader.Name.ToLower() != "packet") {
                Console.WriteLine("올바르지 않은 노드");
                return;
            }

            string packetName = reader["name"];
            if (string.IsNullOrEmpty(packetName)) {
                Console.WriteLine("Packet 이름이 없음");
                return;
            }

            Tuple<string, string, string> tuple = ParseMembers(reader);
            genPackets += string.Format(PacketFormat.packetFormat,
                // PacketFormat.packetFormat의 {0}, {1}, {2}, {3}
                // {0}: 패킷 이름
                // {1}: 멤버 변수
                // {2}: 멤버 변수 Read
                // {3}: 멤버 변수 Write
                packetName, tuple.Item1, tuple.Item2, tuple.Item3);
            // {0}: 패킷 이름
            // {1}: 패킷 번호
            packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId) 
                + Environment.NewLine + "\t";

            // 서버 -> 클라이언트 패킷
            if (packetName.StartsWith("S_") || packetName.StartsWith("s_")) {
                clientRegister += string.Format(PacketFormat.managerRegisterFormat, packetName)
                    + Environment.NewLine;
            }
            // 클라이언트 -> 서버 패킷
            else {
                serverRegister += string.Format(PacketFormat.managerRegisterFormat, packetName)
                    + Environment.NewLine;
            }
        }

        // {1}: 멤버 변수
        // {2}: 멤버 변수 Read
        // {3}: 멤버 변수 Write
        // PacketFormat.packetFormat의 {1}, {2}, {3}을 튜플로 반환
        public static Tuple<string, string, string> ParseMembers(XmlReader reader) {
            string packetName = reader["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            // 최초 깊이에 1을 더한 값이 파싱하려는 정보
            int depth = reader.Depth + 1;
            
            // 패킷 내부 정보를 계속 읽음
            while (reader.Read()) {
                if (reader.Depth != depth) {
                    break;
                }

                // 멤버 이름을 파싱
                string memberName = reader["name"];
                if (string.IsNullOrEmpty(memberName)) {
                    Console.WriteLine("멤버 이름이 없음");
                    return null;
                }

                // 이미 내용이 있다면 새 줄 추가
                // 안하면 코드에 엔터가 안쳐져있음
                if (string.IsNullOrEmpty(memberCode) == false) {
                    memberCode += Environment.NewLine;
                }

                if (string.IsNullOrEmpty(readCode) == false) {
                    readCode += Environment.NewLine;
                }

                if (string.IsNullOrEmpty(writeCode) == false) {
                    writeCode += Environment.NewLine;
                }

                // 멤버 타입 파싱
                string memberType = reader.Name.ToLower();
                switch (memberType) {
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                        writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        // 위 항목들은 string, list와 다르게
                        // 정해진 코드에 넣을 수 있음. 나머지는 따로 만듦
                        // {0}: 변수 타입
                        // {1}: 변수명
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        // {0}: 변수명
                        // {1}: BitConverter.To~~ 형식
                        // {2}: 변수 타입
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        // {0}: 변수명
                        // {1}: 변수 타입
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        // {0}: 변수 타입
                        // {1}: 변수명
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        // {0}: 변수명
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        // {0}: 변수명
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> tuple = ParseList(reader);
                        memberCode += tuple.Item1;
                        readCode += tuple.Item2;
                        writeCode += tuple.Item3;
                        break;
                    default:
                        break;
                }
            }

            // 인덴트 맞추기
            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader reader) {
            string listName = reader["name"];

            if (string.IsNullOrEmpty(listName)) {
                Console.WriteLine("리스트명이 없음");
                return null;
            }

            Tuple<string, string, string> tuple = ParseMembers(reader);

            string memberCode = string.Format(PacketFormat.memberListFormat, 
                // {0}: 리스트명 대문자(struct)
                // {1}: 리스트명 소문자(instance)
                // {2}: 멤버 변수들
                // {3}: 멤버 변수 Read
                // {4}: 멤버 변수 Write
                FirstCharToUpper(listName),
                FirstCharToLower(listName),
                tuple.Item1, tuple.Item2, tuple.Item3);

            string readCode = string.Format(PacketFormat.readListFormat,
                // {0}: 리스트명 대문자(struct)
                // {1}: 리스트명 소문자(instance)
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            string writeCode = string.Format(PacketFormat.writeListFormat,
                // {0}: 리스트명 대문자(struct)
                // {1}: 리스트명 소문자(instance)
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        // BitConverter.To~~ 로 변환
        public static string ToMemberType(string memberType) {
            switch (memberType) {
                case "bool":
                    return "ToBoolean";
                // 바이트는 변환이 없음
                //case "byte":
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        // 리스트명 대문자로 변환
        public static string FirstCharToUpper(string name) {
            if (string.IsNullOrEmpty(name)) {
                return "";
            }
            return name[0].ToString().ToUpper() + name.Substring(1);
        }

        // 리스트명 소문자로 변환
        public static string FirstCharToLower(string name) {
            if (string.IsNullOrEmpty(name)) {
                return "";
            }
            return name[0].ToString().ToLower() + name.Substring(1);
        }
    }
}