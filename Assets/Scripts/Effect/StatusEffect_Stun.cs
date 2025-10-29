using UnityEngine;

/// <summary>
/// 스턴 상태효과
///     지속 시간동안 행동금지 플래그 제공
///     _host의 IsStunned()로 외부 시스템이 체크하여 행동 막음
/// </summary>
public class StatusEffect_Stun : StatusEffect_Base
{
    public bool IsStunned => TimeLeft > 0.0f;//살아있는 동안 true로 간주

    //호스트에서 처리하므로 따로 오버라이드 할 함수 없음
    protected override void OnAttached()
    {
        base.OnAttached();
    }

    protected override void OnTick(float dt)
    {
        base.OnTick(dt);
    }

    protected override void OnDetached()
    {
        base.OnDetached();
    }
}
