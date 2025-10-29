using System.Collections.Generic;
using UnityEngine;

public enum State
{
    Idle = 0,
    Chase = 1,
    Attack = 2,
    Patrol = 3,
    Dead = 4,
}

/// <summary>
/// 스페셜리스트의 기본 인공지능 제어를 담당하는 간단/직관/튼튼 FSM
///     Idle: 대기, 플레이어 Chase로 전환
///     Chase: 플레이어를 향해 이동 시작(타겟 위치 추적)
///     Attack: 공격 범위 안의 타겟을 인식하면 공격 동작을 하고 쿨타임을 기다림
///     Patrol: 시야 밖의 AI가 플레이어 위치로 이동 후 일정 시간 후 Idle로 전환
///     Dead: Health가 0이 되는 순간 사망(외부에서 Health.OnDeath로 처리)
/// </summary>
public class EnemyAIFlat : MonoBehaviour
{
    [Header("Modules")]
    [SerializeField] private EnemySenses _enemySenses;//시야 감지(플레이어/패트롤 방향)
    [SerializeField] private Health _health;//체력(사망 처리 등 상태 관리)
    [SerializeField] private Transform _player;//간단/직관 타겟 참조 변수
    [SerializeField] private PlayerHealth _playerDamageable;//타겟 IDamageable(공격 대상)
    [SerializeField] private CharacterController _controller;//캐릭터 컨트롤러 이동용 cc(경사/중력 처리용)

    [Header("Movement (flat)")]
    [SerializeField] private float _chaseSpeed = 3.0f;//플레이어 추적(이동속도 m/s). 기본 전진 속도
    [SerializeField] private float _rotateSpeed = 12.0f;//타겟 회전 속도(slerp로 보간)
    [SerializeField] private float _stoppingDistance = 1.2f;//플레이어 접근 거리(공격 범위 직전)
    [SerializeField] private float _gravity = -20.0f;//인공 중력(캐릭터컨트롤러로 이동시 하강 가속도)

    [Header("Attack")]
    [SerializeField] private float _attackRange = 1.8f;//타겟 공격(m)
    [SerializeField] private float _attackDamage = 10.0f;//공격력
    [SerializeField] private float _attackCooldown = 1.2f;//공격 쿨타임(초). 0이면 즉시

    [Header("Patrol")]
    [SerializeField] private float _patrolDuration = 3.0f;//시야 밖에서 플레이어 위치로 이동 후 대기 시간(초)

    [Header("Debug")]
    [SerializeField] private bool _drawForward = false;//전방 방향선 표시 여부

    //[SerializeField]//테스트용
    private State _state;//현재 FSM 상태
    private float _attackTimer;//공격 대기 타이머(0이면 공격 가능)
    private float _patrolTimer;//patrol 유지 시간
    private Vector3 _lastKnownPos;//플레이어를 마지막으로 본 위치(패트롤 이동 목적지)

    [SerializeField] private List<Transform> _patrolPoints;

    [SerializeField] private StatusEffectHost _host;
    private void Awake()
    {
        if (_controller == null)
        {
            _controller = GetComponent<CharacterController>();
        }

        //초기화
        _state = State.Idle;
        _attackTimer = 0.0f;
        _patrolTimer = 0.0f;
        _lastKnownPos = transform.position;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        //공격 쿨 감소
        if (_attackTimer > 0.0f)
        {
            _attackTimer -= dt;
            if (_attackTimer < 0.0f)
            {
                _attackTimer = 0.0f;
            }
        }

        //체력 상태 확인
        if (_health != null)
        {
            if (_health.CurrentHealth <= 0.0f)
            {
                if (_state != State.Dead)
                {
                    EnterDead();
                }
                return;//Dead 상태의 경우 업데이트 중단
                //외부에서 사망 이벤트로 처리됨
            }
        }

        //시야 체크, 보이면 _lastKnownPos 갱신
        bool seen = false;
        Vector3 seenPos = Vector3.zero;
        if (_enemySenses != null)
        {
            if (_enemySenses.CanSeeTarget(out seenPos))
            {
                seen = true;
                _lastKnownPos = seenPos;
            }
        }

        //상태 업데이트
        switch (_state)
        {
            case State.Idle:
                UpdateIdle(seen);
                break;
            case State.Chase:
                UpdateChase(seen);
                break;
            case State.Attack:
                UpdateAttack(seen);
                break;
            case State.Patrol:
                UpdatePatrol(seen);
                break;
        }

        //전방 방향선 표시(디버그 용도)
        if (_drawForward)
        {
            Debug.DrawRay(transform.position + Vector3.up * 1.0f, transform.forward * 1.5f, Color.yellow, 0.02f);
        }
    }

    #region 상태 전환/업데이트

    private void EnterIdle()
    {
        _state = State.Idle;
        //대기 상태로 진입, 플레이어 감지 시 Chase로 전환됨
    }

    private void UpdateIdle(bool seen)
    {
        if (seen)
        {//플레이어가 보이면 추적
            EnterChase();
            return;
        }
        //미감지 시 Idle 유지
    }

    private void EnterChase()
    {
        _state = State.Chase;
        //추적 상태 초기화
        //UpdateChase에서 방향 및 이동 처리
    }

    private void UpdateChase(bool seen)
    {
        //타겟 회전
        Vector3 targetPos = _lastKnownPos;
        Vector3 flatTarget = targetPos;
        flatTarget.y = transform.position.y;//수평만 고려 → 상하 회전 방지

        Vector3 to = flatTarget - transform.position;//방향/회전 벡터
        to.y = 0.0f;//수평만 유지

        if (to.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * _rotateSpeed);
        }

        //정지거리/전진
        float dist = Vector3.Distance(transform.position, flatTarget);

        if (dist > _stoppingDistance)
        {
            float speedMul = 1.0f;
            if (_host != null)
            {
                speedMul = _host.SpeedMultiplier;
            }

            //전진 벡터(월드 전방 기준)
            Vector3 move = transform.forward * (_chaseSpeed * speedMul);

            //cc 안정화를 위한 중력 보정(평지에서도 경계에서의 떨림 줄임 용도)
            move.y = _gravity;

            _controller.Move(move * Time.deltaTime);
        }

        if (_player != null)
        {//공격 범위 진입 시 Attack으로
            float d2 = Vector3.Distance(transform.position, _player.position);
            if (d2 <= _attackRange)
            {
                EnterAttack();
                return;
            }
        }

        //시야에서 사라지고 도착 시 Patrol로 전환
        if (!seen)
        {
            if (dist <= _stoppingDistance)
            {
                EnterPatrol();
                return;
            }
        }
    }

    private void EnterAttack()
    {
        _state = State.Attack;
        //공격 상태에서는 이동/회전/쿨타임 관리 및 실행
    }

    private void UpdateAttack(bool seen)
    {
        //스턴중이면 리턴 처리
        if (_host != null)
        {
            if (_host.IsStunned)
            {
                return;
            }
        }

        //공격 범위 밖이면 Chase로 복귀
        if (_player != null)
        {
            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist > _attackRange)
            {
                EnterChase();
                return;
            }
        }

        //시야 잃으면 Patrol로
        if (!seen)
        {
            EnterPatrol();
            return;
        }

        //공격 실행
        if (_attackTimer <= 0.0f)
        {
            DoAttack();
            _attackTimer = _attackCooldown;
        }

        //공격 상태에서도 타겟 방향 회전
        if (_player != null)
        {
            Vector3 dir = _player.position - transform.position;
            dir.y = 0.0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * _rotateSpeed);
            }
        }
    }

    /// <summary>
    /// 실제 공격을 수행, IDamageable에 데미지 전달
    /// </summary>
    private void DoAttack()
    {
        if (_playerDamageable == null)
        {
            return;
        }

        //피격 처리 예시 : hitPoint는 타겟 위치, 방향은 Vector3.up 고정
        _playerDamageable.ApplyDamage(_attackDamage, _player.position, Vector3.up, transform);
    }

    private void EnterPatrol()
    {
        _state = State.Patrol;
        _patrolTimer = _patrolDuration;
    }

    private void UpdatePatrol(bool seen)
    {
        //플레이어 위치로 이동(회전 + 속도 감소로 이동)
        Vector3 flatTarget = _lastKnownPos;
        flatTarget.y = transform.position.y;

        Vector3 to = flatTarget - transform.position;
        to.y = 0.0f;

        float dist = to.magnitude;

        if (dist > _stoppingDistance)
        {
            Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * _rotateSpeed);

            Vector3 move = transform.forward * _chaseSpeed * 0.6f;//patrol은 chase보다 느리게
            move.y = _gravity;
            _controller.Move(move * Time.deltaTime);
        }

        //플레이어 보이면 Chase로
        if (seen)
        {
            EnterChase();
            return;
        }

        //지속시간 만료 시 Idle로 전환
        _patrolTimer -= Time.deltaTime;
        if (_patrolTimer <= 0.0f)
        {
            EnterIdle();
        }
    }

    private void EnterDead()
    {
        _state = State.Dead;
    }
    #endregion
}
