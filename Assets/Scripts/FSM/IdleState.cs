using UnityEngine;

/// <summary>
/// Idle: 대기
/// 플레이어를 발견 시 Chase로 전환
/// </summary>
public class IdleState : BaseState
{
    //특이사항.(주로 탐색이 끝난 후 정지) 탐색 후 일정 시간 대기 상태.
    //필요 시 타이머 설정 가능
    public IdleState()
    {

    }
    public IdleState(StateManager context)
    {
        _context = context;
    }

    public override string Name()
    {
        return "Idle";
    }

    public override void OnEnter()
    {
        //Idle 상태에서는 정지 유지
        //추후 타이머/대기 애니메이션 추가 가능
    }

    public override void OnUpdate(float dt)
    {
        //시야 체크
        bool seen = false;
        Vector3 seenPos = Vector3.zero;

        if (_context.Senses != null)
        {
            if (_context.Senses.CanSeeTarget(out seenPos))
            {
                seen = true;
            }
        }
        if (seen)
        {
            _context.LastKnownPos = seenPos;
            _context.RequestStateChange(_context.Chase);//타깃 발견 시 추적 상태로 전환
            return;
        }

        //추가 조건 없으면 Idle 유지
    }

    public override void OnExit()
    {
        //Idle 해제 시 필요한 작업은 없음
    }
}
