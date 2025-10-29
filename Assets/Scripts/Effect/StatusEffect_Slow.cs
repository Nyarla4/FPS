using UnityEngine;

/// <summary>
/// 슬로우 상태 효과.
/// 이동속도에 배율(0~1) 제공
///     _host는 모든 슬로우 중 가장 낮은 배율(가장 강한 슬로우)만 반영
/// </summary>
public class StatusEffect_Slow : StatusEffect_Base
{
    [Header("Slow")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _speedMultiplier = 0.6f;//40%감소 => ×0.6
    public float SpeedMultiplier => _speedMultiplier;

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
