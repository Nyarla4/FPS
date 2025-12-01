using System;
using UnityEngine;
using UnityEngine.UI;

public class TestEnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] Slider _hpBar;

    private float _showTimer;

    [SerializeField] private float _maxHealth;

    private float _health;
    public float Health
    {
        get { return _health; }
        private set { 
            _health = value;
            _hpBar.value = _health;
            if (_health <= 0)
            {
                OnDeath?.Invoke();
            }
        }
    }

    public Action OnDeath;

    public void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, Transform source)
    {
        _showTimer = 5f;
        _hpBar.gameObject.SetActive(true);
        Health -= amount;
    }

    private void Awake()
    {
        _health = _maxHealth;
        _hpBar.minValue = 0;
        _hpBar.value = _health;
        _hpBar.maxValue = _maxHealth;
        _hpBar.gameObject.SetActive(false);
        _showTimer = 0f;
    }

    void Update()
    {
        if (_showTimer > 0)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0)
            {
                _showTimer = 0f;
                _hpBar.gameObject.SetActive(false);
            }
        }
    }
}
