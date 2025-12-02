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
                Destroy(gameObject);
            }
        }
    }

    public Action OnDeath;

    [SerializeField] TestEnemyCore _core;

    public void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, Transform source)
    {
        _showTimer = 5f;
        _hpBar.gameObject.SetActive(true);
        Health -= amount;

        if (_core.Target == null)
        {
            var player = FindObjectsByType<TestPlayerControl>(FindObjectsSortMode.None)[0];
            _core.Target = player.transform;
        }
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
        if (_hpBar.gameObject.activeInHierarchy)
        {
            var camTransform = Camera.main.transform;
            var rot = _hpBar.transform.position + (camTransform.rotation * Vector3.forward);
            //rot.y = 0f;
            _hpBar.transform.LookAt(rot,Vector3.up);
        }

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
