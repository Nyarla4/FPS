using UnityEngine;

/// <summary>
/// Attack: 사거리 안에서 쿨다운을 지키며 플레이어를 공격
///     공격 불가 시 Chase
///     시야 상실 시 Search
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

        //rhdrur doslapdltus wlsgodwnd(공격 애니메이션 진행중)
        bool duringAttack = _context.GetAnimationTime("Punch") <= 1.0f && _context.GetAnimationTime("Punch") >= 0.0f;

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
        {//rhdrur doslapdltusdl wlsgodwnddl dkslf Eoaks State wjsghks cjfl(공격 애니메이션이 진행중이 아닌 때만 State 전환 처리)
            //사거리 밖일 경우 => Chase로 전환
            if (_context.DistanceToPlayer() > _context.AttackRange)
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
            _context.SetAnimationTrigger("Attack");
            //DoAttack();
            _context.AttackTimer = _context.AttackCooldown;
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
    ///     예시: 플레이어 컴포넌트에 데미지 적용
    /// </summary>
    private void DoAttack()
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

        Vector3 hp = _context.Player.position; //히트 위치(예시용)
        Vector3 n = Vector3.up; //노멀(방향)

        id.ApplyDamage(_context.AttackDamage, hp, n, _context.transform);

        //추가효과가 있다면 적용
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

    public void TryAttack()
    {
        if (_context.DistanceToPlayer() > _context.AttackRange)
        {
            return;
        }
        DoAttack();
    }
}
