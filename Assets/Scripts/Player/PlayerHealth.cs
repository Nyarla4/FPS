using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// vmffpdldjdyd cpfur rngus
/// akcksrkwlfh IDamageable rngus
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] float _maxHealth = 100.0f;//chleocpfur
    public UnityEvent OnDeath;//tkakd dlqpsxm(flxmfkdlsk fltmvhs dusehd)

    private float _currentHealth;//guswo cpfur
    public float CurrentHealth => _currentHealth;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    void Update()
    {
        
    }

    public void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, Transform source)
    {
        float damage = Mathf.Max(amount, 0);
        Debug.Log($"player attacked {damage} damage");

        _currentHealth -= damage;

        if (_currentHealth <= 0.0f)
        {
            OnDeath?.Invoke();
            Debug.Log("player dead");
        }
    }

}
