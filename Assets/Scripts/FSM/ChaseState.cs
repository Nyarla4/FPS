using UnityEngine;

/// <summary>
/// Chase: 마지막으로 본 플레이어 위치를 향해 이동/추적
/// 사거리 진입 시 Attack
/// 시야 상실 및 도달 시 Search
/// </summary>
public class ChaseState : BaseState
{
    public ChaseState()
    {

    }
    public ChaseState(StateManager context)
    {
        _context = context;
    }

    public override string Name()
    {
        return "Chase";
    }

    public override void OnEnter()
    {
        //공격상태로의 전환을 준비
        //Chase 상태에서는 이동 시작
    }

    public override void OnUpdate(float dt)
    {
        if (_context.RunDist > 0 && _context.StartRun())
        {//도주거리가 있을 때 체력이 10%이하면 도주
            _context.RequestStateChange(_context.Runaway);
            return;
        }

        float speedMul = 1.0f;
        if (_context.StatusHost != null)
        {
            if (_context.StatusHost.IsStunned)
            {
                return;
            }
            speedMul = _context.StatusHost.SpeedMultiplier;
        }

        //시야 체크
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

        //위치를 향해 회전
        _context.FacePosition(_context.LastKnownPos, dt);

        float distToLast = Vector3.Distance(_context.transform.position, _context.LastKnownPos);
        if (distToLast > _context.StoppingDistance)
        {
            _context.MoveForward(_context.ChaseSpeed * speedMul, dt);
        }

        //공격 사거리 진입 시 => Attack
        float distToPlayer = _context.DistanceToPlayer();
        switch (_context.Pattern)
        {
            case EnemyPattern.Hybrid:
                if (distToPlayer <= _context.RangedAttackRange && distToPlayer > _context.AttackRange)
                {
                    _context.RequestStateChange(_context.RangedAttack);
                    return;
                }
                else if (distToPlayer <= _context.AttackRange)
                {
                    _context.RequestStateChange(_context.Attack);
                    return;
                }
                break;
            case EnemyPattern.Melee:
                if (distToPlayer <= _context.AttackRange)
                {
                    _context.RequestStateChange(_context.Attack);
                    return;
                }
                break;
            case EnemyPattern.Projectile:
                if (distToPlayer <= _context.RangedAttackRange)
                {
                    _context.RequestStateChange(_context.RangedAttack);
                    return;
                }
                break;
            default:
                break;
        }

        //시야 상실 및 목표 지점 도달 시 => Search
        if (!seen)
        {
            if (distToLast <= _context.StoppingDistance)
            {
                _context.RequestStateChange(_context.Search);
                return;
            }
        }
    }

    public override void OnExit()
    {
        //상태 종료
    }
}
