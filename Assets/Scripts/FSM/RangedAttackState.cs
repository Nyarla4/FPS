using UnityEngine;

/// <summary>
/// RangedAttackState: 사거리 안에서 쿨다운을 지키며 플레이어를 공격
///     공격 불가 시 Chase
///     시야 상실 시 Search
///     *원거리 공격
/// </summary>
public class RangedAttackState : BaseState
{
    public RangedAttackState()
    {

    }
    public RangedAttackState(StateManager context)
    {
        _context = context;
    }

    public override string Name()
    {
        return "RangedAttack";
    }

    public override void OnEnter()
    {
        //공격타이머를 리셋 및 쿨다운 상태로 초기화
        //  초기값 0으로 설정
        _context.AttackTimer = 0.0f;
    }

    public override void OnUpdate(float dt)
    {
        if (_context.StatusHost != null)
        {
            if (_context.StatusHost.IsStunned)
            {
                return;
            }
        }

        //사격 애니메이션 진행중
        bool duringAttack = _context.GetAnimationTime("Shooting") <= 1.0f && _context.GetAnimationTime("Shooting") >= 0.0f;

        //시야 확인 + LastKnownPos 갱신
        bool seen = false;
        Vector3 seenPos = Vector3.zero;

        if (_context.Senses != null)
        {
            if (_context.Senses.CanSeeTarget(out seenPos))
            {
                seen = true;
                _context.LastKnownPos = seenPos;
            }
        }

        if (!duringAttack)
        {//사격 애니메이션이 진행중이 아닌 때만 State 전환 처리
            
            //사거리 밖일 경우 => Chase로 전환
            if (_context.DistanceToPlayer() > _context.RangedAttackRange)
            {
                _context.RequestStateChange(_context.Chase);
                return;
            }

            //시야를 상실했을 경우 => Search
            if (!seen)
            {
                _context.RequestStateChange(_context.Search);
                return;
            }
        }

        //공격 쿨다운 감소
        if (_context.AttackTimer > 0.0f)
        {
            _context.AttackTimer -= dt;
        }

        //쿨다운이 0이하일 경우 공격 수행
        if (_context.AttackTimer <= 0.0f)
        {
            _context.SetAnimationTrigger("Shoot");
            //DoAttack();
            _context.AttackTimer = _context.RangedAttackCooldown;
        }

        //방향 회전: 플레이어 쪽으로 회전
        if (_context.Player != null)
        {
            _context.FacePosition(_context.Player.position, dt);
        }
    }

    public override void OnExit()
    {
        //종료됨
    }

    /// <summary>
    /// 실제 공격을 플레이어에게 수행
    /// </summary>
    private void DoShoot()
    {
        if (_context.Player == null)
        {
            return;
        }

        IDamageable id = _context.Player.GetComponent<IDamageable>();
        if (id == null)
        {
            return;
        }

        _context.FireOne();
    }

    /// <summary>
    /// 애니메이션 트리거 이벤트 
    /// </summary>
    public void TryShoot()
    {
        DoShoot();
    }
}
