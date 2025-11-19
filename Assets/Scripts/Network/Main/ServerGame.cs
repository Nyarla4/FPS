using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� ���� �̵� �ùķ��̼� �� STATE ��� ���
///     Ŭ���̾�Ʈ INPUT�� �޾� �������� ��ǥ/ȸ�� ���
///     ���� �ֱ�� STATE�� ��� ó��
/// ���: ������ ���� �������� ��� Ŭ���̾�Ʈ���� ������ ��Ŷ���·� ����
/// </summary>
public class ServerGame : MonoBehaviour
{
    [Header("Settings")]
    public float TickRate = 20.0f;//����ƽ(20ȸ/s => dt: 0.05f)
    public float MoveSpeed = 4.5f;//�̵� �ӵ�(m/s)
    public Transform[] SpawnPoints;//���� ��ġ ���

    private float _tickAccumulator;//ƽ ���� �ð�
    private Dictionary<int, PlayerSim> _sims;//�� �÷��̾� �ùķ��̼� ����

    //Ŭ���̾�Ʈ�� ���� �ֱ� �Է� ����
    private class PendingInput
    {
        public float Mx;//�¿� �Է�(-1~1)
        public float My;//���� �Է�(-1~1)
        public float Yaw;//���� ��(��)
        public float Pitch;//���� ��(��, �̵����� �̻��, ���� ����)
        public bool Fire;//�߻�(�̻��)
    }

    private class PlayerSim
    {
        public int Id;//�÷��̾� ID
        public string Name;//�г���(�ɼ�)
        public Vector3 Position;//���� ��ǥ
        public float Yaw;//��(����)
        public float Pitch;//��(����)
        public int Hp;//ü��
        public PendingInput Input;//�ֽ� �Է�(������ ����)

    }

    private void Awake()
    {
        _sims = new ();
    }

    private void OnEnable()
    {
        //������ ��ɸ� ���۽�Ű�� ����
        if (NetworkRunner.instance != null)
        {
            NetworkRunner.instance.OnServerCommand += OnServerCommand;
        }
    }

    private void OnDisable()
    {
        if (NetworkRunner.instance != null)
        {
            NetworkRunner.instance.OnServerCommand -= OnServerCommand;
        }
    }

    /// <summary>
    /// ���� �� ���� �� �κ� �ִ� �����ڵ� ����
    /// </summary>
    public void SpawnPlayerInitial(IReadOnlyList<int> playerIds)
    {
        //����ŭ for�� �ݺ�=>SpawnPoint�� ���� ó��
        int count = playerIds.Count;
        for (int i = 0; i < count; i++)
        {
            int id = playerIds[i];

            Vector3 pos = Vector3.zero;
            if(SpawnPoints != null && SpawnPoints.Length > 0)
            {
                int idx = i%SpawnPoints.Length;
                if(SpawnPoints[idx] != null)
                {
                    pos = SpawnPoints[idx].position;
                }
            }

            PlayerSim sim = new();
            sim.Id = id;
            sim.Name = $"Player{id}";
            sim.Position = pos;
            sim.Yaw = sim.Pitch = 0.0f;
            sim.Hp = 100;
            sim.Input = new PendingInput();
            _sims.Add(id, sim);
        }
    }

    private void Update()
    {
        //������ �ƴ� ��� ����X
        if(NetworkRunner.instance == null || !NetworkRunner.instance.IsServerRunning())
        {
            return;
        }

        float dt = Time.deltaTime;
        _tickAccumulator += dt;

        float tickDelta = 1.0f / TickRate;
        while (_tickAccumulator >= tickDelta)
        {
            ServerTick(tickDelta);
            _tickAccumulator -= tickDelta;
        }
    }

    private void ServerTick(float dt)
    {
        //�Է� ����=>��ġ/ȸ�� ����
        foreach (var kv in _sims)
        {
            PlayerSim sim = kv.Value;
            if (sim == null)
            {
                continue;
            }

            PendingInput input = sim.Input;
            if(input == null)
            {
                continue;
            }

            //����� �̵� ���� ���
            //yaw(��) => ���� => ����/���� ����
            float yawRad = input.Yaw * Mathf.Deg2Rad;
            Vector3 forward = new Vector3(Mathf.Sin(yawRad),0.0f,Mathf.Cos(yawRad));
            Vector3 right = new Vector3(forward.z * -1.0f, 0.0f, forward.x);//Vector3.Cross(Vector3.up, forward)�� ������ ��� �� ����

            Vector3 wish = (right*input.Mx)+(forward*input.My);

            if (wish.sqrMagnitude > 0.0001f)
            {
                wish = wish.normalized;
            }

            Vector3 delta = wish * MoveSpeed * dt;
            sim.Position += delta;

            //ȸ�� �� ������Ʈ(�ü� ó��)
            sim.Yaw = input.Yaw;
            sim.Pitch = input.Pitch;
        }

        //�ֱ��� STATE ��� ó��
        BroadCastState();
    }

    private void BroadCastState()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("{\"players\":[}");
        bool first = true;
        foreach (var kv in _sims)
        {
            PlayerSim p = kv.Value;
            if (p == null)
            {
                continue;
            }

            if (!first)
            {
                sb.Append(',');
            }
            first = false;

            sb.Append("{");
            sb.AppendFormat("\"id\": {0}, \"x\": {1:F3}, \"y\": {2:F3}, \"z\": {3:F3}, \"yaw\": {4:F1}, \"hp\": {5}"
                , p.Id, p.Position.x, p.Position.y, p.Position.z, p.Yaw, p.Hp);
            sb.Append("}");
        }
        sb.Append("]}");

        string json = sb.ToString();
        if (NetworkRunner.instance != null)
        {
            NetworkRunner.instance.ServerBroadcastLinePublic("STATE|" + json);
        }
    }

    /// <summary>
    /// ���������� ����� ������ ���� �о �÷��̾��� �ùķ��̼� ������ ó��
    /// </summary>
    private void OnServerCommand(int fromClientId, string cmd, string payload)
    {
        if(cmd == "INPUT")
        {
            //payload: mx, my, yaw, pitch, fire
            string[] parts = payload.Split(",");
            if (parts==null || parts.Length < 5)
            {
                return;
            }

            float mx = 0.0f;
            float my = 0.0f;
            float yaw = 0.0f;
            float pitch = 0.0f;
            int fireInt = 0;

            float.TryParse(parts[0], out mx);
            float.TryParse(parts[1], out my);
            float.TryParse(parts[2], out yaw);
            float.TryParse(parts[3], out pitch);
            int.TryParse(parts[4], out fireInt);

            bool fire = fireInt == 1;

            if (_sims.ContainsKey(fromClientId))
            {
                PlayerSim sim = _sims[fromClientId];
                if(sim!=null && sim.Input != null)
                {
                    sim.Input.Mx = mx;
                    sim.Input.My = my;
                    sim.Input.Yaw = yaw;
                    sim.Input.Pitch = pitch;
                    sim.Input.Fire = fire;
                }
            }
        }
        //FIRE ������ ���� �߰�
    }
}
