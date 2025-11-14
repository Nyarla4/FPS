using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 기본 체력/사망 로직
/// IDamageable 구현체
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float MaxHealth = 1000.0f;//최대 체력
    public bool DestroyOnDeath = true;//사망시 오브젝트 파괴 여부
    public UnityEvent OnDeath;//사망시 이벤트

    [Header("Optional Feedback")]
    public ParticleSystem HitVfxPrefab; //피격시 파티클
    public AudioSource AudioSource;//사운드 재생
    public AudioClip HurtClip;//피격시 효과음

    private float _currentHealth;//현재 체력
    public float CurrentHealth => _currentHealth;

    private void Awake()
    {
        _currentHealth = MaxHealth;
    }

    public void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, Transform source)
    {
        //체력 감소 처리
        float damage = Mathf.Max(amount, 0.0f);
        _currentHealth -= damage;

        //피격 효과 처리
        if (HitVfxPrefab != null)
        {
            Quaternion rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(hitNormal, Vector3.up), hitNormal);
            ParticleSystem vfx = Instantiate(HitVfxPrefab, hitPoint + hitNormal * 0.01f, rot);
            Destroy(vfx.gameObject, 1.0f);
        }
        if(AudioSource != null)
        {
            if (HurtClip != null)
            {
                AudioSource.PlayOneShot(HurtClip, 1.0f);
            }
        }

        //사망 처리
        if (_currentHealth <= 0.0f)
        {
            OnDeath?.Invoke();

            if (DestroyOnDeath)
            {
                var animator = GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    public void Healing(float amount)
    {
        //체력 증가 처리
        float damage = Mathf.Max(amount, 0.0f);
        _currentHealth += damage;
    }
}
