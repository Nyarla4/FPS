using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Chase: akwlakrdmfh qhs vmffpdldj dnlclfh ghlwjs/wjswls
/// tkrjfl dlsoaus Attack
/// tldi tkdtlf gn ehckr tl Search
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
        //znfekdnsdms AttackStatedptj rhksfl
        //Chasedptjsms chrlghkgkf gkdahr djqtdam
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

        //tldi rodtls
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

        //ghlwjs/wjswls
        _context.FacePosition(_context.LastKnownPos, dt);

        float distToLast = Vector3.Distance(_context.transform.position, _context.LastKnownPos);
        if(distToLast > _context.StoppingDistance)
        {
            _context.MoveForward(_context.ChaseSpeed * speedMul, dt);
        }

        //tkrjfl vkswjd => Attack
        float distToPlayer = _context.DistanceToPlayer();
        if(distToPlayer <= _context.AttackRange)
        {
            _context.RequestStateChange(_context.Attack);
            return;
        }

        //tldi tkdtlf tkdxofh ahrwjrwl ehckr => Search
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
        //djqtdma
    }
}
