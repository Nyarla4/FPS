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
        if (distToPlayer <= _context.AttackRange)
        {
            _context.RequestStateChange(_context.Attack);
            return;
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
