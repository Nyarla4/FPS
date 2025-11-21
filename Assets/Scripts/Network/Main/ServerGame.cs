using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 서버 권한 이동 시뮬레이션과 STATE 방송을 담당.
/// - 클라이언트의 INPUT을 받아 서버에서 좌표/회전을 계산.
/// - 일정 주기로 STATE를 브로드캐스트.
/// </summary>
public class ServerGame : MonoBehaviour
{
    [Header("Settings")]
    public float TickRate = 20.0f;         // 서버 틱(초당 20회 -> dt=0.05)
    public float MoveSpeed = 4.5f;         // 이동 속도(m/s)
    public Transform[] SpawnPoints;        // 스폰 위치 목록

    private float tickAccumulator;         // 틱 누적 시간
    private Dictionary<int, PlayerSim> sims;  // 각 플레이어의 시뮬레이션 상태

    // 클라이언트가 보낸 최근 입력을 저장
    private class PendingInput
    {
        public float worldX;   // 월드 기준 이동 X(우+ 좌-)
        public float worldZ;   // 월드 기준 이동 Z(앞+ 뒤-)
        public float yaw;      // 시선 각(도)
        public float pitch;    // 필요 시 사용
    }

    private class PlayerSim
    {
        public int id;                 // 플레이어 ID
        public string name;            // 닉네임(옵션)
        public Vector3 position;       // 월드 좌표
        public float yaw;              // 도(수평)
        public float pitch;            // 도(수직)
        public int hp;                 // 체력
        public PendingInput input;     // 최신 입력(없으면 정지)

        public float LastFireTime;     // 마지막 발사 시각(쿨다운 제어용)
        public Damageable Damageable;  // 해당 플레이어의 Damageable(아바타 루트에 붙이거나 참조 처리)
        public Transform Root;
    }

    [Header("Fire Settings")]
    public float FireCooldown = 0.12f;//발사 쿨다운(초), ex:500RPM 0.12
    public float RayMaxDistance = 150.0f;//히트스캔 레이 최대 거리
    public int DamagePerShot = 20;//한 발 당 대미지
    public float EyeHeight = 1.6f;//시야 높이(서버에서 레이 생성 기준)
    public LayerMask HitMask;//피격 레이어 마스크, ex: Hitbox

    public bool IgnoreSpawnPointRotation = true;//회전 무시(항상 +Z 방향)

    private void Awake()
    {
        sims = new Dictionary<int, PlayerSim>();
    }

    private void OnEnable()
    {
        // 서버만 동작
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
    /// 게임 씬 시작 시, 로비에 있던 참가자들을 스폰한다.
    /// </summary>
    public void SpawnPlayersInitial(IReadOnlyList<int> playerIds)
    {
        int count = playerIds.Count;
        for (int i = 0; i < count; i = i + 1)
        {
            int id = playerIds[i];

            Vector3 pos = Vector3.zero;
            if (SpawnPoints != null && SpawnPoints.Length > 0)
            {
                int idx = i % SpawnPoints.Length;
                if (SpawnPoints[idx] != null)
                {
                    pos = SpawnPoints[idx].position;
                }
            }

            PlayerSim sim = new PlayerSim();
            sim.id = id;
            sim.name = $"Player{id}";
            sim.position = pos;
            sim.yaw = 0.0f;
            sim.pitch = 0.0f;
            sim.hp = 100;
            sim.input = new PendingInput();
            sims.Add(id, sim);
        }
    }

    private void Update()
    {
        // 서버 아닌 경우 동작 안 함
        if (NetworkRunner.instance == null)
        {
            return;
        }
        if (NetworkRunner.instance.IsServerRunning() == false)
        {
            return;
        }

        float dt = Time.deltaTime;
        tickAccumulator = tickAccumulator + dt;

        float tickDelta = 1.0f / TickRate;
        while (tickAccumulator >= tickDelta)
        {
            ServerTick(tickDelta);
            tickAccumulator = tickAccumulator - tickDelta;
        }
    }

    private void ServerTick(float dt)
    {
        // 입력 적용 -> 위치/회전 갱신
        foreach (var kv in sims)
        {
            PlayerSim sim = kv.Value;
            if (sim == null)
            {
                continue;
            }

            PendingInput input = sim.input;
            if (input == null)
            {
                continue;
            }

            // 1) 이미 월드 방향으로 온 값 사용
            Vector3 wish = new Vector3(input.worldX, 0.0f, input.worldZ);

            if (wish.sqrMagnitude > 0.0001f)
            {
                wish = wish.normalized;
                Vector3 delta = wish * MoveSpeed * dt;
                sim.position = sim.position + delta;
            }

            // 2) 각도 유지(시야 연출용)
            sim.yaw = input.yaw;
            sim.pitch = input.pitch;
        }

        // 주기적으로 STATE 방송
        BroadcastState();
    }

    private void BroadcastState()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("{\"players\":[");
        bool first = true;
        foreach (var kv in sims)
        {
            PlayerSim p = kv.Value;
            if (p == null)
            {
                continue;
            }

            if (first == false)
            {
                sb.Append(",");
            }
            first = false;

            sb.Append("{");
            sb.AppendFormat("\"id\":{0},\"x\":{1:F3},\"y\":{2:F3},\"z\":{3:F3},\"yaw\":{4:F1},\"hp\":{5}",
                p.id, p.position.x, p.position.y, p.position.z, p.yaw, p.hp);
            sb.Append("}");
        }
        sb.Append("]}");

        string json = sb.ToString();
        if (NetworkRunner.instance != null)
        {
            NetworkRunner.instance.ServerBroadcastLinePublic("STATE|" + json);
        }
    }

    private void OnServerCommand(int fromClientId, string cmd, string payload)
    {
        if (cmd == "INPUT")
        {
            HandleInputWorld(fromClientId, payload);
            return;
        }

        if (cmd == "FIRE")
        {
            HandleFire(fromClientId);
        }
    }

    private void HandleInputWorld(int fromClientId, string payload)
    {
        // payload: "wx,wz,yaw,pitch" (InvariantCulture 로 들어옴)
        string[] parts = payload.Split(',');
        if (parts == null)
        {
            return;
        }
        if (parts.Length < 4)
        {
            return;
        }

        // Culture 안전 파싱
        System.Globalization.CultureInfo inv = System.Globalization.CultureInfo.InvariantCulture;

        float wx = 0.0f;
        float wz = 0.0f;
        float yawDeg = 0.0f;
        float pitchDeg = 0.0f;

        float.TryParse(parts[0], System.Globalization.NumberStyles.Float, inv, out wx);
        float.TryParse(parts[1], System.Globalization.NumberStyles.Float, inv, out wz);
        float.TryParse(parts[2], System.Globalization.NumberStyles.Float, inv, out yawDeg);
        float.TryParse(parts[3], System.Globalization.NumberStyles.Float, inv, out pitchDeg);

        if (sims.ContainsKey(fromClientId) == true)
        {
            PlayerSim sim = sims[fromClientId];
            if (sim != null && sim.input != null)
            {
                sim.input.worldX = wx;
                sim.input.worldZ = wz;
                sim.input.yaw = yawDeg;
                sim.input.pitch = pitchDeg;
            }
        }
    }

    /// <summary>
    /// 서버 기준 사격 관련 처리
    /// </summary>
    private void HandleFire(int fromClientId)
    {
        PlayerSim sim;
        if (!sims.TryGetValue(fromClientId, out sim))
        {
            return;
        }
        if (sim == null)
        {
            return;
        }

        //쿨다운 확인
        float now = Time.time;
        if (now < sim.LastFireTime + FireCooldown)
        {
            return;
        }
        sim.LastFireTime = now;

        //레이 원점
        Vector3 eye = sim.position + Vector3.up * EyeHeight;

        //yaw/pitch => 전방 벡터
        float yawRad = sim.yaw * Mathf.Deg2Rad;
        float pitchRad = sim.pitch * Mathf.Deg2Rad;

        //카메라 기준 전방 => yaw 회전 후 pitch 경사 적용

        //sin yaw, 0, cos yaw에서 pitch로 상하 분해
        Vector3 forwardHorizontal = new Vector3(
            Mathf.Sin(yawRad),
            0.0f,
            Mathf.Cos(yawRad)
            );

        //pitch로 상하 분해
        float cosPitch = Mathf.Cos(pitchRad);
        float sinPitch = Mathf.Sin(pitchRad);
        
        Vector3 dir = new Vector3(
            forwardHorizontal.x * cosPitch,
            -sinPitch,
            forwardHorizontal.z * cosPitch
            );
        dir = dir.normalized;

        Debug.DrawRay(eye, dir * RayMaxDistance, Color.green, 1.0f);//방향 레이 표시

        //레이캐스트 처리
        RaycastHit hit;
        bool isHit = Physics.Raycast(eye, dir, out hit, RayMaxDistance, HitMask, QueryTriggerInteraction.Ignore);
        if( isHit)
        {
            //피격 대상 Damageable 확인
            Damageable dmg = hit.collider.GetComponentInParent<Damageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(DamagePerShot);

                //죽은 경우 서버에서 리스폰 처리
                if(dmg.CurHp == 0)
                {
                    Respawn(dmg.transform);
                }
            }
        }

        //사격 이벤트를 클라이언트에 알리는 경우 이 위치에서 방송가능(로컬말고 전체 서버가 볼 수 있도록 할 경우)
    }

    private void Respawn(Transform avatarRoot)
    {
        if(avatarRoot == null)
        {
            return;
        }
        //리스폰 위치가 없으면 원점, 있으면 위치 중 랜덤 한 곳
        if(SpawnPoints == null || SpawnPoints.Length == 0)
        {
            avatarRoot.position = Vector3.zero;
            avatarRoot.rotation = Quaternion.identity;
        }
        else
        {
            Transform sp = SpawnPoints[UnityEngine.Random.Range(0,SpawnPoints.Length)];
            if(sp != null)
            {
                avatarRoot.position = sp.position;
                if (IgnoreSpawnPointRotation)
                {
                    avatarRoot.rotation = Quaternion.identity;//항상 월드 +Z 방향
                }
                else
                {
                    avatarRoot.rotation = sp.rotation;
                }
            }
        }

        //Damageable 초기화
        if(avatarRoot.TryGetComponent<Damageable>(out var dmg))
        {
            dmg.ResetHp();
        }

        //서버 시뮬의 위치/각도 리셋
        foreach (var kv in sims)
        {
            PlayerSim sim = kv.Value;
            if(sim!=null && sim.Root == avatarRoot)
            {
                sim.position = avatarRoot.position;
                sim.yaw = 0.0f;
                sim.pitch = 0.0f;
            }
        }

        //STATE 방송해서 클라이언트 갱신
        BroadcastState();
    }

    public void SetRoot(int id, Transform root)
    {
        PlayerSim sim;
        if(!sims.TryGetValue(id, out sim))
        {
            return;
        }
        if(sim == null)
        {
            return;
        }

        sim.Root = root;
    }
}