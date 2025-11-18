using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// 서버/클라이언트 네트워킹 총괄
/// 런타임 매니저
///     -Host(서버) 시작/중지
///     -Client(클라) 접속/종료
///     -Update에서 수신 폴링(Non-Blocking)
///     -간단 이벤트(Action)으로 UI에 알림 처리
/// 라인 프로토콜(LineProtocol)을 이용해 문자열 메시지 송수신
/// </summary>
public class NetworkRunner : MonoBehaviour
{
    public static NetworkRunner instance;//전역 접근(씬에 1개)

    [Header("Def")]
    public int DefaultPort = 7777;//기본 포트
    public float RoomBroadcastInterval = 0.5f;//서버 룸 방송 주기(초)

    //서버 필드
    private TcpListener _serverListener;//서버 리스너 소켓
    private bool _serverRunning;//서버 동작 여부
    private Dictionary<int, ClientConn> _clients;//접속중인 클라이언트 맵
    private int _nextClientId;//다음에 부여할 클라이언트 ID
    private float _roomBroadcastTimer;//룸 주기 타이머

    //클라이언트 필드
    private TcpClient _client;//클라이언트 소켓
    private NetworkStream _clientStream;//클라이언트 스트림
    private LineProtocol _clientLp;//클라이언트 라인 프로토콜
    private bool _clientConnected;//클라이언트 접속 여부

    //서버를 룸에 표시하기 위한 플레이어 엔트리(HOST는 클라이언트 겸직함)
    private PlayerInfo _hostPlayer;//ID=0으로 사용할 호스트 플레이어
    public bool IncludeHostInRoom = true;//호스트를 룸 목록에 포함할지 여부

    //로비 상태(서버 Authoritative)
    private class PlayerInfo
    {
        public int Id;//고유 ID
        public string Name;//닉네임
        public bool Ready;//준비 여부
    }

    private class ClientConn
    {
        public int Id;//클라 ID
        public TcpClient Socket;//TCP 소켓
        public NetworkStream Stream;//스트림
        public LineProtocol Lp;//라인 프로토콜
        public PlayerInfo Info;//플레이어 정보
    }

    private Dictionary<int, PlayerInfo> _players; //서버가 관리중인 플레이어 목록
    private List<int> _pendingRemoveClientIds = new();//클라이언트 제거 지연 처리용(담아놓고 추후 제거)

    //UI 연동 이벤트
    public Action<string> OnStatus;//상태 로그 출력
    public Action<string> OnRoomText;//룸 스냅샷 텍스트
    public Action<bool> OnHostModeChanged;//호스트 여부 알림
    public Action<bool> OnClientConnectedChanged;//클라접속 여부 알림
    public Action OnStartSignal; //START 수신 알림

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _clients = new();
        _players = new();
        _nextClientId = 1;
    }

    void Update()
    {
        //서버 수신/수락
        if (_serverRunning)
        {
            Server_AcceptPending();
            Server_PollReceive();
            Server_RoomBroadcastTick();
        }

        //클라이언트 수신
        if (_clientConnected)
        {
            Client_PoolReceive();
        }
    }

    //서버: 시작/중지 처리
    public void HostStart(int port)
    {
        if (_serverRunning)
        {
            OnStatus?.Invoke("Server already running");
            //서버 이미 돌아가는 중이면 return
            return;
        }

        try
        {
            _serverListener = new(IPAddress.Any, port);
            _serverListener.Start();
            _serverRunning = true;

            _roomBroadcastTimer = 0.0f;

            //Host를 0번 ID 플레이어로 등록 처리
            if (IncludeHostInRoom)
            {
                _hostPlayer = new();
                _hostPlayer.Id = 0;
                _hostPlayer.Name = "Host";
                _hostPlayer.Ready = false;

                if (!_players.ContainsKey(0))
                {
                    _players.Add(0, _hostPlayer);
                }
            }

            OnStatus?.Invoke($"Server listening on port{port}");
            OnHostModeChanged?.Invoke(true);
        }
        catch (Exception e)
        {
            OnStatus?.Invoke($"Server start failed: {e.Message}");
        }
    }

    public void HostStop()
    {
        if (!_serverRunning)
        {//시작도 안했으면 종료도 없다
            return;
        }

        foreach (var kv in _clients)
        {
            try
            {
                kv.Value.Socket.Close();
            }
            catch
            {

            }
        }
        _clients.Clear();
        _players.Clear();

        try
        {
            _serverListener.Stop();
            //리스너 정지로 다른 것들 안받도록 끊음
        }
        catch
        {

        }

        _serverRunning = false;

        OnHostModeChanged?.Invoke(false);

        OnStatus?.Invoke("Server stopped");

        //서버를 끄면 클라도 끊어지는 상황이 많으므로 추가 정리
        if (_clientConnected)
        {
            ClientDisconnect();
        }
    }

    //서버: 수락/수신/방송 처리
    private void Server_AcceptPending()
    {
        if (_serverListener == null)
        {
            return;
        }

        bool pending = _serverListener.Pending();
        if (!pending)
        {
            return;
        }

        try
        {
            //클라의 접속 받은 부분
            TcpClient sock = _serverListener.AcceptTcpClient();
            sock.NoDelay = true;

            //접속된 클라세팅
            ClientConn cc = new();
            cc.Id = _nextClientId;
            _nextClientId++;
            cc.Socket = sock;
            cc.Stream = sock.GetStream();
            cc.Lp = new(cc.Stream);

            PlayerInfo pi = new();
            pi.Id = cc.Id;
            pi.Name = $"Player {cc.Id}";
            pi.Ready = false;
            cc.Info = pi;

            //컨테이너에 추가
            _clients.Add(cc.Id, cc);
            _players.Add(cc.Id, pi);

            OnStatus?.Invoke($"Client {cc.Id} connected");
        }
        catch (Exception e)
        {
            //접속중 오류발생 시
            OnStatus?.Invoke($"Accept failed: {e.Message}");
        }
    }

    /// <summary>
    /// 서버측에서 데이터 수신/처리
    /// </summary>
    private void Server_PollReceive()
    {
        foreach (var kv in _clients)
        {
            ClientConn cc = kv.Value;
            if (cc == null || cc.Socket == null)
            {
                continue;
            }

            NetworkStream st = cc.Stream;
            if (st == null)
            {
                continue;
            }

            List<string> lines = cc.Lp.ReadAvailableLines();
            if (lines == null)
            {
                continue;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                Server_HandleLine(cc, line);
            }

            //간단한 연결 확인: 소켓 닫혔는지 체크 불가 => 예외 발생 시 제거 로직으로 처리
            //※여기서는 생략
        }

        //루프 후 한꺼번에 제거 처리
        if (_pendingRemoveClientIds != null && _pendingRemoveClientIds.Count > 0)
        {
            for (int i = 0; i < _pendingRemoveClientIds.Count; i++)
            {
                int id = _pendingRemoveClientIds[i];
                Server_RemoveClient(id);
            }
            _pendingRemoveClientIds.Clear();

            //실제 삭제 반영된 상태를 한번만 방송
            Server_BroadcastRoom();
        }
    }

    /// <summary>
    /// //UI에서 선택한 행동에 따라 함수 처리
    /// </summary>
    private void Server_HandleLine(ClientConn cc, string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return;
        }

        //"CMD|payload" 형태
        int bar = line.IndexOf('|');
        string cmd;
        string payload;

        if (bar >= 0)
        {
            cmd = line.Substring(0, bar);
            payload = line.Substring(bar + 1);
        }
        else
        {
            cmd = line;
            payload = string.Empty;
        }

        if (cmd == "JOIN")
        {//JOIN 눌렀을때

            //연결 요청
            if (!string.IsNullOrEmpty(payload))
            {
                cc.Info.Name = payload;
            }
            cc.Info.Ready = false;

            OnStatus?.Invoke($"JOIN from {cc.Id} as {cc.Info.Name}");

            Server_BroadcastRoom();
        }
        else if(cmd == "READY")
        {//READY 눌렀을때

            if(payload == "1")
            {
                cc.Info.Ready = true;
            }
            else
            {
                cc.Info.Ready = false;
            }

            OnStatus?.Invoke($"READY {cc.Id} = {cc.Info.Ready}");

            Server_BroadcastRoom();
        }
        else if (cmd == "LEAVE")
        {//나갔을때

            //열거 중 컬렉션 수정 에러를 막기 위해, 즉시삭제 않고 삭제 대기열에 추가
            _pendingRemoveClientIds.Add(cc.Id);//삭제할 플레이어를 담아놓는 처리
            //룸 방송은 실제 삭제 적용 후 1번만(아래에서 처리)
        }
        else if( cmd =="START")
        {//시작 눌렀을 때

            //호스트 전용으로 가정(실습에서는 에디터=호스트 버튼으로만 전송)
            Server_BroadcastLine("START");
            OnStatus?.Invoke("START broadcasted");
        }
        else
        {
            OnStatus?.Invoke($"Unknown cmd from {cc.Id}: {cmd}");
        }
    }

    /// <summary>
    /// 서버에다 한 줄 방송 처리
    /// </summary>
    private void Server_BroadcastLine(string line)
    {
        foreach (var kv in _clients)
        {
            ClientConn cc = kv.Value;
            if (cc == null)
            {
                continue;
            }
            if (cc.Lp == null)
            {
                continue;
            }
            cc.Lp.WriteLine(line);
        }
    }

    /// <summary>
    /// ID를 받아서 해당 플레이어를 컨테이너에서 삭제처리
    /// </summary>
    private void Server_RemoveClient(int id)
    {
        if (_clients.ContainsKey(id))
        {
            try
            {
                _clients[id].Socket.Close();
            }
            catch
            {

            }

            _clients.Remove(id);
        }

        if (_players.ContainsKey(id))
        {
            _players.Remove(id);
        }

        OnStatus?.Invoke($"Client {id} removed");
    }

    /// <summary>
    /// 현재 방 상태 방송
    ///     준비상태, 들어가거나 나간 상태 등
    /// </summary>
    private void Server_BroadcastRoom()
    {
        // 간단 JSON 구성
        // {"players":[{"id":1,"name":"Alice","ready":true}, ...]}
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("{\"players\":[");
        bool first = true;
        foreach (var kv in _players)
        {
            PlayerInfo p = kv.Value;
            if (first == false)
            {
                sb.Append(",");
            }
            first = false;
            sb.Append("{");
            sb.AppendFormat("\"id\":{0},\"name\":\"{1}\",\"ready\":{2}",
                p.Id, EscapeJson(p.Name), p.Ready == true ? "true" : "false");
            sb.Append("}");
        }
        sb.Append("]}");
        string json = sb.ToString();

        Server_BroadcastLine("ROOM|" + json);
    }

    /// <summary>
    /// 시간마다 상태 틱 처리 후 방송
    /// </summary>
    private void Server_RoomBroadcastTick()
    {
        _roomBroadcastTimer += Time.deltaTime;
        if (_roomBroadcastTimer < RoomBroadcastInterval)
        {
            return;
        }
        _roomBroadcastTimer = 0.0f;
        Server_BroadcastRoom();
    }

    /// <summary>
    /// 클라이언트 접속
    /// </summary>
    /// <param name="address">ip</param>
    /// <param name="port">포트</param>
    public void ClientConnect(string address, int port)
    {
        if (_clientConnected == true)
        {
            OnStatus?.Invoke("Client already connected.");
            return;
        }

        try
        {
            _client = new TcpClient();
            _client.NoDelay = true;
            _client.Connect(address, port);
            _clientStream = _client.GetStream();
            _clientLp = new LineProtocol(_clientStream);
            _clientConnected = true;

            OnClientConnectedChanged?.Invoke(true);

            OnStatus?.Invoke($"Connected to {address}:{port}");
        }
        catch (Exception e)
        {
            OnStatus?.Invoke($"Connect failed: {e.Message}");
        }
    }

    /// <summary>
    /// 클라이언트가 서버로 송신
    /// </summary>
    public void ClientSendLine(string line)
    {
        if (_clientConnected == false)
        {
            return;
        }
        if (_clientLp == null)
        {
            return;
        }
        _clientLp.WriteLine(line);
    }

    /// <summary>
    /// 클라이언트가 서버로부터 수신
    /// </summary>
    private void Client_PoolReceive()
    {
        if (_clientConnected == false)
        {
            return;
        }
        if (_clientLp == null)
        {
            return;
        }

        List<string> lines = _clientLp.ReadAvailableLines();
        if (lines == null)
        {
            return;
        }

        for (int i = 0; i < lines.Count; i = i + 1)
        {
            string line = lines[i];
            Client_HandleLine(line);
        }
    }

    /// <summary>
    /// 수신한 데이터를 데이터별로 텍스트 창에 찍어줌
    /// </summary>
    private void Client_HandleLine(string line)
    {
        if (string.IsNullOrEmpty(line) == true)
        {
            return;
        }

        int bar = line.IndexOf('|');
        string cmd;
        string payload;

        if (bar >= 0)
        {
            cmd = line.Substring(0, bar);
            payload = line.Substring(bar + 1);
        }
        else
        {
            cmd = line;
            payload = string.Empty;
        }

        if (cmd == "ROOM")
        {
            // UI 텍스트로 그대로 보여준다(파싱 생략 가능)
            if (OnRoomText != null)
            {
                OnRoomText.Invoke(payload);
            }
        }
        else if (cmd == "START")
        {
            if (OnStartSignal != null)
            {
                OnStartSignal.Invoke();
            }
        }
        else
        {
            // 기타 메시지는 상태 로그로 출력
            OnStatus?.Invoke("SAYS: " + line);
        }
    }

    /// <summary>
    /// 접속 끊을때 호출
    /// </summary>
    public void ClientDisconnect()
    {
        if (_clientConnected == false)
        {
            return;
        }

        try
        {
            if (_client != null)
            {
                _client.Close();
            }
        }
        catch
        {
        }

        _client = null;
        _clientStream = null;
        _clientLp = null;
        _clientConnected = false;

        OnClientConnectedChanged?.Invoke(false);

        OnStatus?.Invoke("Disconnected.");
    }

    /// <summary>
    /// 문자열 대체 처리
    /// </summary>
    private string EscapeJson(string s)
    {
        if (s == null)
        {
            return "";
        }

        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    public bool IsServerRunning()
    {
        return _serverRunning == true;
    }

    public bool IsClientConnected()
    {
        return _clientConnected == true;
    }

    public void HostBroadcastStart()
    {
        if (_serverRunning == false)
        {
            return;
        }

        Server_BroadcastLine("START");
        OnStatus?.Invoke("START broadcasted by host.");
    }

    public void HostSetName(string name)
    {
        if (_serverRunning == false)
        {
            return;
        }
        if (IncludeHostInRoom == false)
        {
            return;
        }
        if (_hostPlayer == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(name) == false)
        {
            _hostPlayer.Name = name;
        }
        Server_BroadcastRoom();
    }

    public void HostSetReady(bool ready)
    {
        if (_serverRunning == false)
        {
            return;
        }
        if (IncludeHostInRoom == false)
        {
            return;
        }
        if (_hostPlayer == null)
        {
            return;
        }

        _hostPlayer.Ready = ready;
        Server_BroadcastRoom();
    }
}
