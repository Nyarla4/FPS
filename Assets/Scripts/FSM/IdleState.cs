using UnityEngine;

/// <summary>
/// Idle: eorl
/// vmffpdldjfmf qhaus Chasefh wjsdl
/// </summary>
public class IdleState : BaseState
{
    //todtjdwk.(zmffotm dlfmarhk ehddlfgks gkatn) zmffotm rorcp todtjd tl wkehd ghcnf.
    //epdlxj chrlghk emddmf gkfEo tkdyd
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
        //Idle wlsdlqdptjsms chrlghk djqtdma
        //vlfdy tl eorlahtus/tkdnsem xmflrj rksmd
    }

    public override void OnUpdate(float dt)
    {
        //tldi vkswjd
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
            _context.RequestStateChange(_context.Chase);//to dlstmxjstm ghrdms wotkdyd
            return;
        }

        //qkfrus ahtgks ruddn Idle dbwl
    }

    public override void OnExit()
    {
        //Idle whdfydptjsms wjdflgkf sodyd djqtdma
    }
}
