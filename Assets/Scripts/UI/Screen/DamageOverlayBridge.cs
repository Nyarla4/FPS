using UnityEngine;

/// <summary>
/// PlayerHealth에서 OnDamaged 이벤트를 받아서 ScreenDamageOverlay 호출
///     인스펙터에서 PlayerHealth 연결
/// </summary>
public class DamageOverlayBridge : MonoBehaviour
{
    public PlayerHealth PlayerHealth;//대미지 이벤트 제공 컴포넌트
    public float AssumedMaxHealth = 100.0f;//오버레이 강도 계산에 사용할 최대 체력(동기화 목적)
    
    private void Awake()
    {
        if(PlayerHealth != null)
        {
            PlayerHealth = GetComponent<PlayerHealth>();
        }

        if (PlayerHealth == null)
        {
            Debug.LogError("[DamageOverlayBridge] PlayerHealth 누락");
        }
    }
    private void OnEnable()
    {
        if (PlayerHealth != null)
        {
            PlayerHealth.OnDamaged.AddListener(OnPlayerDamaged);
            PlayerHealth.OnDeath.AddListener(OnPlayerDied);
        }
    }

    private void OnDisable()
    {
        if (PlayerHealth != null)
        {
            PlayerHealth.OnDamaged.RemoveListener(OnPlayerDamaged);
            PlayerHealth.OnDeath.RemoveListener(OnPlayerDied);
        }
    }

    /// <summary>
    /// 플레이어가 대미지 받았을때 호출되는 콜백
    /// 대미지 양에 비례하여 오버레이 재생
    /// </summary>
    /// <param name="amount">대미지</param>
    private void OnPlayerDamaged(float amount)
    {
        //오버레이 인스턴스 확인
        if (ScreenDamageOverlay.Instance == null)
        {
            return;
        }

        float maxHp = AssumedMaxHealth;

        if (PlayerHealth != null)
        {//최대 체력을 얻을 수 있는 경우 사용
            maxHp = PlayerHealth.MaxHealth;
        }

        ScreenDamageOverlay.Instance.PlayDamageFlash(amount, maxHp);
    }

    private void OnPlayerDied()
    {
        if (ScreenDamageOverlay.Instance == null)
        {
            return;
        }

        ScreenDamageOverlay.Instance.DeadScreen();
    }
}
