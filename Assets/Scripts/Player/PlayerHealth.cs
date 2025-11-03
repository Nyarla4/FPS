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
    public UnityEvent OnDeath; // 사망 이벤트(사운드나 애니메이션 호출용)

    private float _currentHealth; // 현재 체력
    public float CurrentHealth => _currentHealth;

    [SerializeField] private Slider _healthSlider;
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

        if (_currentHealth <= 0.0f)
        {
            OnDeath?.Invoke();
            Debug.Log("player dead");
        }
    }

}
