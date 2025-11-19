using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// ����/Ŭ���̾�Ʈ ��Ʈ��ŷ �Ѱ�
/// ��Ÿ�� �Ŵ���
///     -Host(����) ����/����
///     -Client(Ŭ��) ����/����
///     -Update���� ���� ����(Non-Blocking)
///     -���� �̺�Ʈ(Action)���� UI�� �˸� ó��
/// ���� ��������(LineProtocol)�� �̿��� ���ڿ� �޽��� �ۼ���
/// </summary>
public class NetworkRunner : MonoBehaviour
{
    public static NetworkRunner instance;//���� ����(���� 1��)

    [Header("Def")]
    public int DefaultPort = 7777;//�⺻ ��Ʈ
    public float RoomBroadcastInterval = 0.5f;//���� �� ��� �ֱ�(��)

    //���� �ʵ�
    private TcpListener _serverListener;//���� ������ ����
    private bool _serverRunning;//���� ���� ����
    private Dictionary<int, ClientConn> _clients;//�������� Ŭ���̾�Ʈ ��
    private int _nextClientId;//������ �ο��� Ŭ���̾�Ʈ ID
    private float _roomBroadcastTimer;//�� �ֱ� Ÿ�̸�

    //Ŭ���̾�Ʈ �ʵ�
    private TcpClient _client;//Ŭ���̾�Ʈ ����
    private NetworkStream _clientStream;//Ŭ���̾�Ʈ ��Ʈ��
    private LineProtocol _clientLp;//Ŭ���̾�Ʈ ���� ��������
    private bool _clientConnected;//Ŭ���̾�Ʈ ���� ����

    //������ �뿡 ǥ���ϱ� ���� �÷��̾� ��Ʈ��(HOST�� Ŭ���̾�Ʈ ������)
    private PlayerInfo _hostPlayer;//ID=0���� ����� ȣ��Ʈ �÷��̾�
    public bool IncludeHostInRoom = true;//ȣ��Ʈ�� �� ��Ͽ� �������� ����

    //�κ� ����(���� Authoritative)
    private class PlayerInfo
    {
        public int Id;//���� ID
        public string Name;//�г���
        public bool Ready;//�غ� ����
    }

    private class ClientConn
    {
        public int Id;//Ŭ�� ID
        public TcpClient Socket;//TCP ����
        public NetworkStream Stream;//��Ʈ��
        public LineProtocol Lp;//���� ��������
        public PlayerInfo Info;//�÷��̾� ����
    }

    private Dictionary<int, PlayerInfo> _players; //������ �������� �÷��̾� ���
    private List<int> _pendingRemoveClientIds = new();//Ŭ���̾�Ʈ ���� ���� ó����(��Ƴ��� ���� ����)

    //UI ���� �̺�Ʈ
    public Action<string> OnStatus;//���� �α� ���
    public Action<string> OnRoomText;//�� ������ �ؽ�Ʈ
    public Action<bool> OnHostModeChanged;//ȣ��Ʈ ���� �˸�
    public Action<bool> OnClientConnectedChanged;//Ŭ������ ���� �˸�
    public Action OnStartSignal; //START ���� �˸�

    public Action<int, string, string> OnServerCommand;
    public Action<string, string> OnClientCommand;

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

        // Runner를 씬 전환 후에도 유지
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        //���� ����/����
        if (_serverRunning)
        {
            Server_AcceptPending();
            Server_PollReceive();
            Server_RoomBroadcastTick();
        }

        //Ŭ���̾�Ʈ ����
        if (_clientConnected)
        {
            Client_PoolReceive();
        }
    }

    //����: ����/���� ó��
    public void HostStart(int port)
    {
        if (_serverRunning)
        {
            OnStatus?.Invoke("Server already running");
            //���� �̹� ���ư��� ���̸� return
            return;
        }

        try
        {
            _serverListener = new(IPAddress.Any, port);
            _serverListener.Start();
            _serverRunning = true;

            _roomBroadcastTimer = 0.0f;

            //Host�� 0�� ID �÷��̾�� ��� ó��
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
        {//���۵� �������� ���ᵵ ����
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
            //������ ������ �ٸ� �͵� �ȹ޵��� ����
        }
        catch
        {

        }

        _serverRunning = false;

        OnHostModeChanged?.Invoke(false);

        OnStatus?.Invoke("Server stopped");

        //������ ��� Ŭ�� �������� ��Ȳ�� �����Ƿ� �߰� ����
        if (_clientConnected)
        {
            ClientDisconnect();
        }
    }

    //����: ����/����/��� ó��
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
            //Ŭ���� ���� ���� �κ�
            TcpClient sock = _serverListener.AcceptTcpClient();
            sock.NoDelay = true;

            //���ӵ� Ŭ����
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

            //�����̳ʿ� �߰�
            _clients.Add(cc.Id, cc);
            _players.Add(cc.Id, pi);

            OnStatus?.Invoke($"Client {cc.Id} connected");
        }
        catch (Exception e)
        {
            //������ �����߻� ��
            OnStatus?.Invoke($"Accept failed: {e.Message}");
        }
    }

    /// <summary>
    /// ���������� ������ ����/ó��
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

            //������ ���� Ȯ��: ���� �������� üũ �Ұ� => ���� �߻� �� ���� �������� ó��
            //�ؿ��⼭�� ����
        }

        //���� �� �Ѳ����� ���� ó��
        if (_pendingRemoveClientIds != null && _pendingRemoveClientIds.Count > 0)
        {
            for (int i = 0; i < _pendingRemoveClientIds.Count; i++)
            {
                int id = _pendingRemoveClientIds[i];
                Server_RemoveClient(id);
            }
            _pendingRemoveClientIds.Clear();

            //���� ���� �ݿ��� ���¸� �ѹ��� ���
            Server_BroadcastRoom();
        }
    }

    /// <summary>
    /// //UI���� ������ �ൿ�� ���� �Լ� ó��
    /// </summary>
    private void Server_HandleLine(ClientConn cc, string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return;
        }

        //"CMD|payload" ����
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
        {//JOIN ��������

            //���� ��û
            if (!string.IsNullOrEmpty(payload))
            {
                cc.Info.Name = payload;
            }
            cc.Info.Ready = false;

            OnStatus?.Invoke($"JOIN from {cc.Id} as {cc.Info.Name}");

            Server_BroadcastRoom();
        }
        else if(cmd == "READY")
        {//READY ��������

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
        {//��������

            //���� �� �÷��� ���� ������ ���� ����, ��û��� �ʰ� ���� ��⿭�� �߰�
            _pendingRemoveClientIds.Add(cc.Id);//������ �÷��̾ ��Ƴ��� ó��
            //�� ����� ���� ���� ���� �� 1����(�Ʒ����� ó��)
        }
        else if( cmd =="START")
        {//���� ������ ��

            //ȣ��Ʈ �������� ����(�ǽ������� ������=ȣ��Ʈ ��ư���θ� ����)
            Server_BroadcastLine("START");
            OnStatus?.Invoke("START broadcasted");
        }
        else
        {
            // 로비 외 커맨드는 게임 모듈로 전달
            OnServerCommand?.Invoke(cc.Id, cmd, payload);

            OnStatus?.Invoke($"Unknown cmd from {cc.Id}: {cmd}");
        }
    }

    /// <summary>
    /// �������� �� �� ��� ó��
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
    /// ID�� �޾Ƽ� �ش� �÷��̾ �����̳ʿ��� ����ó��
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
    /// ���� �� ���� ���
    ///     �غ����, ���ų� ���� ���� ��
    /// </summary>
    private void Server_BroadcastRoom()
    {
        // ���� JSON ����
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
    /// �ð����� ���� ƽ ó�� �� ���
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
    /// Ŭ���̾�Ʈ ����
    /// </summary>
    /// <param name="address">ip</param>
    /// <param name="port">��Ʈ</param>
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
    /// Ŭ���̾�Ʈ�� ������ �۽�
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
    /// Ŭ���̾�Ʈ�� �����κ��� ����
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
    /// ������ �����͸� �����ͺ��� �ؽ�Ʈ â�� �����
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
            // UI �ؽ�Ʈ�� �״�� �����ش�(�Ľ� ���� ����)
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
            OnClientCommand.Invoke(cmd, payload);

            // ��Ÿ �޽����� ���� �α׷� ���
            OnStatus?.Invoke("SAYS: " + line);
        }
    }

    /// <summary>
    /// ���� ������ ȣ��
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
    /// ���ڿ� ��ü ó��
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

        // 호스트 자신에게도 즉시 START 신호 발생 (중요!)
        OnStartSignal?.Invoke();
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

    public void ServerBroadcastLinePublic(string line)
    {
        // 1) 네트워크로 클라이언트들에게 방송
        if (_serverRunning == true)
        {
            Server_BroadcastLine(line);
        }

        // 2) 호스트(서버) 로컬에도 같은 내용을 전달해 호스트 화면에서도 적용되게 함
        //    (ClientGame.OnClientCommand -> ApplyStateJson 이 호출되도록)
        int bar = line.IndexOf('|');
        string cmd = bar >= 0 ? line.Substring(0, bar) : line;
        string payload = bar >= 0 ? line.Substring(bar + 1) : string.Empty;

        // STATE 외에도 필요하면 다른 메시지도 로컬 반영 가능
        if (cmd == "STATE")
        {
            OnClientCommand?.Invoke(cmd, payload);
        }
    }

    // 실제 참가자 id를 반환
    public List<int> GetCurrentPlayerIdsSnapshot()
    {
        List<int> ids = new List<int>();

        // players 딕셔너리는: id=0 (호스트; includeHostInRoom==true일 때) + 클라들(1..N)
        if (_players != null)
        {
            foreach (var kv in _players)
            {
                ids.Add(kv.Key);
            }
        }

        // 정렬은 선택 사항(보기 좋게)
        ids.Sort();

        return ids;
    }

    public void ServerInjectCommand(int fromClientId, string cmd, string payload)
    {
        // 서버가 켜져 있을 때, 네트워크 경유 없이
        // 서버 콜백(onServerCommand)을 직접 호출해 로컬 입력을 주입한다.
        if (_serverRunning == false)
        {
            return;
        }

        OnServerCommand?.Invoke(fromClientId, cmd, payload);
    }
}
