using UnityEngine;

public class TestEnemyCore : MonoBehaviour
{
    public enum FSM
    {
        patrol,
        chase,
        attack,
    }

    [SerializeField] private ClearLogics _clear;

    public Transform Target;
    private TestAgentControl _agent;
    [SerializeField] private Transform[] _patrolPoints;
    private int _patrolIndex = 0;
    private float _patrolDist = 0.5f;
    private TestEnemyHealth _health;
    public bool IsAlive => _health.Health > 0f;
    [SerializeField] private float _testFloat;

    private FSM _curState;

    [Header("Vision")]
    public Transform Eye; //적 기준점(머리 등 위치)
    public float ViewDistance = 18.0f; //적 거리(m)
    public float ViewAngle = 110.0f; //적 시야각. 각도 = ViewAngle * 0.5f
    public LayerMask VisionMask; //시야 레이캐스트 충돌 마스크(벽, 지형, 플레이어 등)

    [SerializeField] private float _attackDist;
    private float _attackTimer;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _attackCooldown;
    private float _rotateSpeed = 50f;

    private void Awake()
    {
        _agent = GetComponent<TestAgentControl>();
        _health = GetComponent<TestEnemyHealth>();
        _clear.AddEnemy(this);
    }

    private void Start()
    {
        _patrolIndex = 0;
        _health.OnDeath += OnDeath;

        OnPatrolEnter();
    }

    void Update()
    {
        switch (_curState)
        {
            case FSM.patrol:
                Patrol();
                break;
            case FSM.chase:
                Chase();
                break;
            case FSM.attack:
                Attack();
                break;
            default:
                break;
        }
    }

    #region FSM logic
    public void StateChange(FSM state)
    {
        if(state == _curState)
        {
            return;
        }

        switch (_curState)
        {
            case FSM.patrol:
                OnPatrolExit();
                break;
            case FSM.chase:
                OnChaseExit();
                break;
            case FSM.attack:
                OnAttackExit();
                break;
            default:
                break;
        }

        switch (state)
        {
            case FSM.patrol:
                OnPatrolEnter();
                break; 
            case FSM.chase:
                OnChaseEnter();
                break;
            case FSM.attack:
                OnAttackEnter();
                break;
        }
        _curState = state;
        return;
    }

    public void OnPatrolEnter()
    {
        int firstIndex = -1;
        float firstDist = float.PositiveInfinity;
        for (int i = 0; i < _patrolPoints.Length; i++)
        {
            var point = _patrolPoints[i];
            var dist = Vector3.Distance(point.position, transform.position);
            if (dist < firstDist)
            {
                firstIndex = i;
                firstDist = dist;
            }
        }

        _patrolIndex = firstIndex;
    }

    public void Patrol()
    {
        SightCheck();

        if (Target != null)
        {
            StateChange(FSM.chase);
            return;
        }

        var curDest = _patrolPoints[_patrolIndex].position;
        if (Vector3.Distance(curDest, transform.position) <= _patrolDist)
        {
            var nextIdx = _patrolIndex + 1;
            if (nextIdx >= _patrolPoints.Length)
            {
                nextIdx = 0;
            }
            _patrolIndex = nextIdx;
            var nextDir = _patrolPoints[_patrolIndex];
            _agent.SetDestination(nextDir.position);
        }
        else
        {
            _agent.SetDestination(curDest);
        }


    }

    public void OnPatrolExit()
    {
        
    }

    public void OnChaseEnter()
    {
        
    }

    public void Chase()
    {
        if(Vector3.Distance(transform.position, Target.position) <= _attackDist)
        {
            StateChange(FSM.attack);
            return;
        }
        _agent.SetDestination(Target.position);
    }

    public void OnChaseExit()
    {

    }

    public void OnAttackEnter()
    {
        //공격 관련 함수 초기화 처리
        _agent.Stop();
        _attackTimer = 0.0f;
    }

    public void Attack()
    {
        var dt = Time.deltaTime;

        if (Vector3.Distance(transform.position, Target.position) > _attackDist)
        {
            StateChange(FSM.chase);
            return;
        }

        _attackTimer -= dt;

        if (_attackTimer <= 0.0f)
        {
            DoAttack();
            _attackTimer = _attackCooldown;
        }

        FacePosition(Target.position, dt);
    }

    public void OnAttackExit()
    {

    }
    #endregion

    private void OnDeath()
    {
        _clear.Check();
    }

    private void SightCheck()
    {
        var player = FindObjectsByType<TestPlayerControl>(FindObjectsSortMode.None)[0].transform;

        //거리 계산
        Vector3 toTarget =player.position - Eye.position; //타겟까지 방향(정규화 전)
        float dist = toTarget.magnitude; //거리(m)
        if (dist > ViewDistance)
        { //시야 거리 초과 시
            return;
        }

        //시야각 계산(내적 기반 = cos(θ))
        Vector3 forward = Eye.forward; //적 정면 방향
        Vector3 dir = toTarget.normalized; //타겟까지 정규화된 방향
        float dot = Vector3.Dot(forward, dir); //cos(theta): 두 방향 벡터의 내적
        float halfRad = (ViewAngle * 0.5f) * Mathf.Deg2Rad; //절반 각도 라디안
        float cosHalf = Mathf.Cos(halfRad); //비교 기준: cos(절반각)

        //dot < cos(절반각)이면 시야 밖
        if (dot < cosHalf)
        {
            return;
        }

        //라인캐스트로 가림 여부 확인(시야 방해물 충돌 체크)
        Ray ray = new Ray(Eye.position, dir);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, ViewDistance, VisionMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(Eye.position, hit.point, Color.green);

            //충돌한 오브젝트의 루트가 타겟 루트와 같다면 시야 내 존재
            Transform h = hit.collider.transform;
            if (IsSameRoot(h, player))
            {
                if (h.root.TryGetComponent<PlayerHealth>(out var hp))
                {//죽었으면 못찾은걸로 처리
                    if (hp.CurrentHealth <= 0)
                    {
                        return;
                    }
                }
                Target = player;
            }
        }
    }

    private bool IsSameRoot(Transform a, Transform b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        return a.root == b.root;
    }

    private void DoAttack()
    {
        if (Target == null)
        {
            return;
        }

        IDamageable id = Target.GetComponent<IDamageable>();
        if (id == null)
        {
            return;
        }

        Vector3 hp = Target.position; //히트 위치(예시용)
        Vector3 n = Vector3.up; //노멀(방향)

        id.ApplyDamage(_attackDamage, hp, n, transform);
    }

    private void FacePosition(Vector3 target, float dt)
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
}
