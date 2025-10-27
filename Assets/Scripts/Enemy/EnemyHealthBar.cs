using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private Health _health;
    [SerializeField] private Camera _cam;
    [SerializeField] private Vector3 _offSet = new Vector3(0.0f, 2.0f, 0.0f);
    [SerializeField] private Transform _headTrans;
    private void Awake()
    {
        if (_hpSlider == null || _health == null)
        {
            Debug.LogError("[EnemyHealthBar] 슬라이더나 Health 누락");
        }

        if (_cam == null)
        {
            _cam = Camera.main;
        }
        _hpSlider.minValue = 0f;
        _hpSlider.maxValue = 1f;

        _health.OnDeath.AddListener(OnDeath);
    }

    void LateUpdate()
    {
        _hpSlider.value = _health.CurrentHealth / _health.MaxHealth;
        var viewPort = _cam.WorldToViewportPoint(transform.position);
        if (viewPort.x <= 1.0f && viewPort.x >= 0.0f && viewPort.y <= 1.0f && viewPort.y >= 0.0f && viewPort.z > 0.0f)
        {
            if (!_hpSlider.gameObject.activeInHierarchy)
            {
                _hpSlider.gameObject.SetActive(true);
            }
            _hpSlider.transform.position = _cam.WorldToScreenPoint(_headTrans.position) + _offSet;
        }
        else
        {
            _hpSlider.gameObject.SetActive(false);
        }
    }

    void OnDeath()
    {
        _hpSlider.gameObject.SetActive(false);
    }
}
