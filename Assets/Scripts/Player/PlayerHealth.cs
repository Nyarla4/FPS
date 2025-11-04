using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 플레이어의 체력 관리
/// 외부에서 호출 시 IDamageable 인터페이스 사용
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] float _maxHealth = 100.0f; // 최대체력
    public float MaxHealth => _maxHealth;
    public UnityEvent OnDeath; // 사망 이벤트(사운드나 애니메이션 호출용)

    private float _currentHealth; // 현재 체력
    public float CurrentHealth => _currentHealth;

    [SerializeField] private Slider _healthSlider;

    public UnityEvent<float> OnDamaged;//입은 대미지량 전달
    private void Awake()
    {
        _currentHealth = _maxHealth;
        _healthSlider.minValue = 0;
        _healthSlider.maxValue = _maxHealth;
        _healthSlider.value = _maxHealth;
    }

    void Update()
    {

    }

    public void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, Transform source)
    {
        float damage = Mathf.Max(amount, 0);
        Debug.Log($"player attacked {damage} damage");

        _currentHealth -= damage;

        _healthSlider.value = _currentHealth;
        OnDamaged?.Invoke(damage);//피격 이벤트

        if (_currentHealth <= 0.0f)
        {
            OnDeath?.Invoke();
            Debug.Log("player dead");
        }
    }

    /// <summary>
    /// 현재 체력을 0~1 사이의 값으로 반환
    /// 현재 체력 / 최대 체력
    /// </summary>
    /// <returns></returns>
    public float GetHealthRatio()
    {
        if (_maxHealth <= 0.0f)
        {
            return 0.0f;
        }

        return Mathf.Clamp01(_currentHealth / _maxHealth);
    }
}
