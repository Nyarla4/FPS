using System.Data;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>
/// Attack: tkrjfl sodptj znfekdnsdmf wlzlau vmffpdldjfmf rhdrur
///     tkrjfl dlxkf tl Chase
///     tldi dhksjs tkdtlftl Search
/// </summary>
public class AttackState : BaseState
{
    public AttackState()
    {

    }
    public AttackState(StateManager context)
    {
        _context = context;
    }

    public override string Name()
    {
        return "Attack";
    }

    public override void OnEnter()
    {
        //rhdrurtkdxo wlsdlq tl znfekdnsdmf wmrtl 0dmfh cjfl
        //  wmrtl 1ghl rhdrur
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

        //tldi vkseks + LastKnownPos rodtls
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

        //tkrjfl dbwl cpzm => dlxkf tl Chasefh qhrrnl
        if(_context.DistanceToPlayer() > _context.AttackRange)
        {
            _context.RequestStateChange(_context.Chase);
            return;
        }

        //tldirk dhkswjsgl Rmsgrlaus => Search
        if (!seen)
        {
            _context.RequestStateChange(_context.Search);
            return;
        }

        //rhdrur znfekdns xkdlaj
        if (_context.AttackTimer > 0.0f)
        {
            _context.AttackTimer -= dt;
        }

        //znfekdnsdl 0dlaus rhdrur tlfgod
        if (_context.AttackTimer <= 0.0f)
        {
            DoAttack();
            _context.AttackTimer = _context.AttackCooldown;
        }

        //tlrkr wjdfuf: vmffpdldj qkfkqhrp ghlwjs
        if(_context.Player != null)
        {
            _context.FacePosition(_context.Player.position, dt);
        }
    }

    public override void OnExit()
    {
        //djqtdma
    }

    /// <summary>
    /// tlfwp eoalwlfmf vmffpdldjdprp wjsekf
    ///     ekstns: vmffpdldj dnlclfmf vlrurdnlclfh tkdyd
    /// </summary>
    private void DoAttack()
    {
        if (_context.Player == null)
        {
            return;
        }

        IDamageable id = _context.Player.GetComponent<IDamageable>();
        if (id==null)
        {
            return;
        }

        Vector3 hp = _context.Player.position;//glxm vhdlsxm(ekstnscjfl)
        Vector3 n = Vector3.up;//qjqtjs(duscnfdyd)

        id.ApplyDamage(_context.AttackDamage, hp, n, _context.transform);

        //tkdxodltkd cjflsms skwnddp
        switch (_context.AttackEffect)
        {
            case statusEffects.None:
                break;
            case statusEffects.Dot:
                StatusEffect_Dot dotPreset = GameObject.FindFirstObjectByType<StatusEffect_Dot>();
                if (dotPreset != null)
                {
                    StatusEffectApplier.ApplyTo(_context.Player.gameObject, dotPreset);
                }
                break;
            case statusEffects.Slow:
                StatusEffect_Slow slow = GameObject.FindFirstObjectByType<StatusEffect_Slow>();
                if (slow != null)
                {
                    StatusEffectApplier.ApplyTo(_context.Player.gameObject, slow);
                }
                break;
            case statusEffects.Stun:
                StatusEffect_Stun stun = GameObject.FindFirstObjectByType<StatusEffect_Stun>();
                if (stun != null)
                {
                    StatusEffectApplier.ApplyTo(_context.Player.gameObject, stun);
                }
                break;
            default:
                break;
        }
    }

}
