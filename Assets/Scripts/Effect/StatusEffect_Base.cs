using UnityEngine;

public enum statusEffects
{
    None = -1,
    Dot = 0,
    Slow = 1,
    Stun = 2
}

/// <summary>
/// 상태효과의 공통 기반 클래스(추상)
///     수명/갱신/만료를 위한 공통필드, 수명주기 훅
///     StatusEffectHost : 생상/부착/갱신/호출 담당
/// </summary>
public abstract class StatusEffect_Base : MonoBehaviour
{
    [Header("Common")]
    public string EffectName = "Effect";//디버그/표시용 이름
    public Sprite Icon;//UI 아이콘(선택)
    public float Duraion = 3.0f;//지속 시간(초)
    public bool RefreshOnReapply = true;//동일 효과 재적용 시 지속시간 갱신 여부

    protected float _timeLeft;//남은 시간(초)
    protected StatusEffectHost _host;//대상의 호스트

    public bool IsExpired => _timeLeft <= 0.0f;//만료 여부. _host가 제거 시점을 판단하는 값
    public float TimeLeft => _timeLeft;//UI 등에 표시할 남은 시간

    /// <summary>
    /// 호스트 부착 시 호출. 파라미터에서 호스트 주입 및 남은 시간 초기화
    /// </summary>
    public void Attach(StatusEffectHost h)
    {
        _host = h;
        _timeLeft = Duraion;

        OnAttached();
    }

    /// <summary>
    /// 매 프레임(혹은 고정 틱) 갱신.
    /// _host의 Update에서 호출
    /// </summary>
    public void Tick(float dt)
    {
        OnTick(dt);

        _timeLeft -= dt;
        if (_timeLeft < 0.0f)
        {
            _timeLeft = 0.0f;
        }
    }

    /// <summary>
    /// 동일 타입 재적용될 때 호출.
    /// 기본적으로 지속시간 갱신
    /// 파생 클래스에서 스택 증가 등 추가 동작 구현 가능
    /// </summary>
    public virtual void OnReapplied()
    {
        _timeLeft = Duraion;
    }

    /// <summary>
    /// 호스트에서 제거될 때 호출(정리 및 초기화)
    /// </summary>
    public void Detach()
    {
        OnDetached();
    }

    //이하는 미구현 => 상속받은 자식에서 오버라이드해서 내용 처리
    /// <summary> 파생 클래스 부착 시 </summary>
    protected virtual void OnAttached() { }

    /// <summary> 파생 클래스 매 프레임 갱신 </summary>
    protected virtual void OnTick(float dt) { }

    /// <summary> 파생 클래스 해제 시 </summary>
    protected virtual void OnDetached() { }
}