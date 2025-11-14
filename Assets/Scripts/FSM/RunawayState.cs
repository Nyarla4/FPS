using System.Transactions;
using UnityEngine;

/// <summary>
/// Runaway: 체력이 10% 이하인 경우 도주
///     일정 거리이상 도주한 경우 정지
///     정지 후 일정 시간에 걸쳐서 50%까지 회복
/// </summary>
public class RunawayState : BaseState
{
    public RunawayState()
    {

    }
    public RunawayState(StateManager context)
    {
        _context = context;
    }

    public override string Name()
    {
        return "Runaway";
    }

    public bool IsRun => !_context.RunEnough();

    public override void OnEnter()
    {
        
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

        //종료 체크
        //체력 50% 이상인 경우 추격으로 전환
        if (_context.EndRun())
        {
            _context.RequestStateChange(_context.Chase);
            return;
        }

        //충분히 도망치지 못한 경우
        if (IsRun)
        {//도주

            if (_context.Senses != null)
            {
                if (_context.Senses.CanSeeTarget(out var seenPos))
                {
                    _context.LastKnownPos = seenPos;
                }
            }

            //반대위치를 향해 회전
            _context.BackPosition(_context.LastKnownPos, dt);

            //도주
            _context.MoveForward(_context.ChaseSpeed * speedMul, dt);
        }
        else//충분히 도망친 경우
        {//회복
            _context.SetAnimationIdle();
            _context.Healing();
            return;
        }
    }

    public override void OnExit()
    {
        //상태 종료
    }
}
