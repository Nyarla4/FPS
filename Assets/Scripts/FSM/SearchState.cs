using UnityEngine;

/// <summary>
/// Search: 시야 잃은 뒤 마지막 위치 근처를 일정 탐색
///     시간이 끝나면 Idle로 전환
///     다시 발견시 Chase
/// </summary>
public class SearchState : BaseState
{
    public SearchState()
    {

    }
    public SearchState(StateManager context)
    {
        _context = context;
    }

    public override string Name()
    {
        return "Search";
    }

    public override void OnEnter()
    {
        //탐색 시간 초기화
        _context.SearchTimer = _context.SearchDuration;
    }

    public override void OnUpdate(float dt)
    {
        //시야 확인 후 보이면 Chase로
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

        if (seen)
        {
            _context.RequestStateChange(_context.Chase);
            return;
        }

        //마지막 위치쪽으로 서서히 회전
        _context.FacePosition(_context.LastKnownPos, dt);

        float distToLast = Vector3.Distance(_context.transform.position, _context.LastKnownPos);
        if (distToLast > _context.StoppingDistance)
        {
            _context.MoveForward(_context.SearchSpeed, dt);
        }

        //시간경과 => Idle
        _context.SearchTimer -= dt;

        if (_context.SearchTimer <= 0.0f)
        {
            _context.RequestStateChange(_context.Idle);
            return;
        }
    }

    public override void OnExit()
    {
        //종료시
    }
}
