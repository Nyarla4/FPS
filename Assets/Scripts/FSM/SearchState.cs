using UnityEngine;

/// <summary>
/// Search: tldi dlfgdms gn akwlakr whkvy rmsqkd Wkfqrp tntor
///     xkdladkdnt tl Idle qhrrnl
///     ektl qkfrus tl Chase
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
        //tntor xkdlaj chrlghk
        _context.SearchTimer = _context.SearchDuration;
    }

    public override void OnUpdate(float dt)
    {
        //tldi ghlqhr tl wmrtl Chasefh
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

        //akwlakr whkvyWhrdmfh smflrp wjqrms
        _context.FacePosition(_context.LastKnownPos, dt);

        float distToLast = Vector3.Distance(_context.transform.position, _context.LastKnownPos);
        if (distToLast > _context.StoppingDistance)
        {
            _context.MoveForward(_context.SearchSpeed, dt);
        }

        //xkdladkdnt => Idle
        _context.SearchTimer -= dt;

        if (_context.SearchTimer <= 0.0f)
        {
            _context.RequestStateChange(_context.Idle);
            return;
        }
    }

    public override void OnExit()
    {
        //djqtdma
    }
}
