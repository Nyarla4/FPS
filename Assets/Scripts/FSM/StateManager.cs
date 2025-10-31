using UnityEngine;

/// <summary>
/// 적 FSM 시스템의 중심
///     행동 모듈(시야시스템, 체력시스템, 상태이상 등) 관리
///     현재 상태의 Enter/Update/Exit 호출
///     상태 전환을 전체적으로 제어
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class StateManager : MonoBehaviour
{
    [Header("Modules")]
    public EnemySenses Senses;//시야 센서(보기/듣기/냄새)
    [SerializeField] private Health _health;//체력(현재 생명치)
    public Transform Player;//플레이어(공격 타겟) 트랜스폼
    [SerializeField] private CharacterController _controller;//캐릭터 컨트롤러 CC

    [Header("Movement (flat)")]
    public float ChaseSpeed = 3.0f;//추적 속도(m/s)
    public float SearchSpeed = 1.8f;//탐색 속도(m/s)
    [SerializeField] private float _rotateSpeed = 12.0f;//회전 관련 속도
    public float StoppingDistance = 1.2f;//정지 거리
    [SerializeField] private float _gravity = -20.0f;//중력 가속도(단위)

    [Header("Attack")]
    public float AttackRange = 1.8f;//공격 사거리
    public float AttackDamage = 10.0f;//공격 데미지
    public float AttackCooldown = 1.2f;//공격 쿨다운(초)
    public statusEffects AttackEffect;

    [Header("Search")]
    public float SearchDuration = 3.0f;//탐색 지속(초)

    [Header("Debug")]
    [SerializeField] private bool _drawForward = false;//전방 디버그 표시

    [HideInInspector] public Vector3 LastKnownPos;//마지막 플레이어 위치
    [HideInInspector] public float AttackTimer;//공격 쿨다운 남은 시간
    [HideInInspector] public float SearchTimer;//탐색 남은 시간

    [HideInInspector] public BaseState _currentState;//현재상태
    [HideInInspector] public IdleState Idle;//Idle 상태 인스턴스
    [HideInInspector] public ChaseState Chase;//Chase 상태 인스턴스
    [HideInInspector] public AttackState Attack;//Attack 상태 인스턴스
    [HideInInspector] public SearchState Search;//Search 상태 인스턴스
    [HideInInspector] public DeadState Dead;//Dead 상태 인스턴스

    public StatusEffectHost StatusHost;

    [SerializeField] private Animator _animator;

    private void Awake()
    {
        if (_controller == null)
        {
            _controller = GetComponent<CharacterController>();
        }

        if (_health == null)
        {
            _health = GetComponent<Health>();
        }

        if (Player == null)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            float closestDist = float.PositiveInfinity;
            GameObject closestPl = null;
            for (int plIdx = 0; plIdx < players.Length; plIdx++)
            {
                var curPl = players[plIdx];
                var dist = Vector3.Distance(transform.position, curPl.transform.position);
                if (closestDist > dist)
                {
                    closestDist = dist;
                    closestPl = curPl;
                }
            }
            GameObject go = closestPl;

            if (go != null)
            {
                Player = go.transform;
            }
        }

        if (Senses.Target == null)
        {
            Senses.Target = Player;
        }

        if (StatusHost == null)
        {
            StatusHost = GetComponent<StatusEffectHost>();
        }

        //상태 인스턴스 초기 생성 및 시스템 연결
        //여기서 작성한 State 개체들은  monobehavior가 없으므로 직접 생성해야함
        Idle = new(this);
        Chase = new(this);
        Attack = new(this);
        Search = new(this);
        Dead = new(this);

        //현재 상태이상 위치로 초기화
        LastKnownPos = transform.position;
        AttackTimer = 0.0f;
        SearchTimer = 0.0f;

        //최초 상태 설정: Idle
        RequestStateChange(Idle);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        //체력 상태를 확인 후 사망(상태 진입)
        if (_health != null)
        {
            if (_health.CurrentHealth <= 0.0f)
            {
                if (_currentState != Dead)
                {
                    RequestStateChange(Dead);
                    return;
                }
            }
        }

        //전방 디버그 표시선
        if (_drawForward)
        {
            Debug.DrawRay(transform.position + Vector3.up * 1.0f, transform.forward * 1.5f, Color.yellow, 0.02f);
        }

        //현재 상태 업데이트
        if (_currentState != null)
        {
            _currentState.OnUpdate(dt);
        }
    }

    /// <summary>
    /// 상태간 전환
    /// Exit => 상태 종료 => Enter 호출
    /// null 입력되면 무시
    /// </summary>
    public void RequestStateChange(BaseState next)
    {
        if (next == null)
        {
            return;
        }

        if (_currentState != null)
        {
            _currentState.OnExit();
        }

        _currentState = next;
        _currentState.OnEnter();

        SyncAnimatorWithState(_currentState);
    }

    #region 회전 관련(또는 이동 관련)
    /// <summary>
    /// 캐릭터 전방방향을 타겟쪽으로 회전(지면 평면 기준)
    /// </summary>
    public void FacePosition(Vector3 target, float dt)
    {
        Vector3 flatTarget = target;
        flatTarget.y = transform.position.y;

        Vector3 to = flatTarget - transform.position;//지면 기준 벡터
        to.y = 0.0f;

        if (to.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, dt * _rotateSpeed);
        }
    }

    /// <summary>
    /// 캐릭터 전방으로 이동(중력 포함 기준)
    /// </summary>
    public void MoveForward(float speed, float dt)
    {
        Vector3 move = transform.forward * speed;//전방 속도
        move.y = _gravity;//지속적으로 중력 적용

        _controller.Move(move * dt);
    }

    /// <summary>
    /// 플레이어와의 현재 거리 반환
    /// </summary>
    public float DistanceToPlayer()
    {
        if (Player == null)
        {
            return float.PositiveInfinity;
        }

        float d = Vector3.Distance(transform.position, Player.position);
        return d;
    }

    /// <summary>
    /// 현재 상태명을 반환(디버그/HUD 용)
    /// </summary>
    /// <returns></returns>
    public string CurrentStateName()
    {
        if (_currentState == null)
        {
            return "None";
        }

        return _currentState.Name();
    }
    #endregion

    /// <summary>
    /// Set Animator parameter by current State
    /// </summary>
    private void SyncAnimatorWithState(BaseState state)
    {
        if(_animator == null)
        {
            return;
        }

        string name = state.Name();

        //Default Initialize
        _animator.SetFloat("Speed", 0.0f);
        _animator.ResetTrigger("Attack");
        _animator.SetBool("IsDead", false);

        switch (name)
        {
            case "Idle":
                //nothing at Idle
                break;
            case "Chase":
                _animator.SetFloat("Speed", 1.0f);//To Run at BlendTree
                break;
            case "Search":
                _animator.SetFloat("Speed", 0.5f);//slowly
                break;
            case "Attack":
                //Trigger
                break;
            case "Dead":
                _animator.SetBool("IsDead", true);
                break;
        }
    }

    public void SetAnimationTrigger(string triggerName)
    {
        if(_animator == null)
        {
            return;
        }
        _animator.SetTrigger(triggerName);
    }

    //0: not playing, 0~1: playing, 1<: end
    public float GetAnimationTime(string animationName)
    {
        var info = _animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(animationName))
        {
            return -1;
        }

        return info.normalizedTime;
    }

    public void KillEnemy()
    {
        if (_health.DestroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}
