using System.Net;
using DummyClient;
using ServerCore;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    private ServerSession _session = new ServerSession();
    
    void Start() {
        // DNS는 도메인으로부터 IP를 찾는다.
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        // 도메인에 ip가 여럿일 수 있다. 이 중 0번째를 사용하기로 한다.
        IPAddress ipAddr = ipHost.AddressList[0];
        // 최종 주소를 만들고 클라이언트가 접속할 포트를 지정한다.
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        // 커넥터를 사용하도록 연결 변경
        Connector connector = new Connector();
        connector.Connect(endPoint, () => _session);

    }

    // Update is called once per frame
    void Update() {
        
    }
}
