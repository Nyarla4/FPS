using UnityEngine;

/// <summary>
/// DoT 상태효과. 틱 간격마다 IDamageable로 피해 전달
///     스택 최대치 지원: 재적용시 스택 증가, DPS 누적
///     RefreshOnReapply가 true인 경우 재적용 시 남은 시간 갱신
/// </summary>
public class StatusEffect_Dot : StatusEffect_Base
{
    [Header("Dot")]
    public float DpsPerStack = 5.0f;//스택당 초당 피해
    public float TickInterval = 0.5f;//피해 틱 간격(초)
    public int MaxStack = 5;//최대 스택
    public bool ExtendDurationOnReapply = true;//재적용 시 Duration 갱신 여부

    private int _stacks;//현재 스택수
    private float _tickTimer;//다음 틱까지 남은 시간
    private IDamageable _damageable;//대상의 IDamageable

    public int Stacks => _stacks;//현재 스택 수 반환(UI 표시용)

    protected override void OnAttached()
    {
        //초기화
        _stacks = 1;
        _tickTimer = TickInterval;
        _damageable = GetComponent<IDamageable>();
    }

    public override void OnReapplied()
    {
        //스택 증가(상한까지)
        _stacks = Mathf.Min(_stacks + 1, MaxStack);

        //지속시간 갱신 옵션
        if (ExtendDurationOnReapply)
        {
            _timeLeft = Duraion;
        }
    }

    protected override void OnTick(float dt)
    {
        //틱 타이머 감소
        _tickTimer -= dt;
        if (_tickTimer > 0.0f)
        {
            return;
        }

        //틱시점 도달 => 피해 계산 및 전달
        _tickTimer = TickInterval;

        float dmg = DpsPerStack * TickInterval * _stacks;

        if (_damageable != null)
        {
            //연출 좌표는 대상 중심. 
            Vector3 hp = transform.position;
            Vector3 n = Vector3.up;
            _damageable.ApplyDamage(dmg, hp, n, _host != null ? _host.transform : transform);
        }
    }

    protected override void OnDetached()
    {
        //DoT에서는 해제시 처리 없음
    }
}
