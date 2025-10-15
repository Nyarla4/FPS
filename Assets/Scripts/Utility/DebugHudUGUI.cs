using System.Text;
using TMPro;
using UnityEngine;

public class DebugHudUGUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI _hudText;
    [SerializeField] private Transform _playerTransform;

    [Header("Toggle")]
    [SerializeField] private bool _showHud = true;
    [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;//`

    [Header("Smoothing")]
    [SerializeField] private float _emaFactor = 0.1f;//0~1

    private float _smoothedDt = 0.0f; // inner EMA
    private StringBuilder _sb; //answkduf snwjr qjvj(String accumulating buffer)

    private void Awake()
    {
        if (_hudText == null)
        {
            Debug.LogWarning("[DebugHudUGUI] hudText not set");
        }

        if (_playerTransform == null)
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                _playerTransform = camera.transform;
            }
        }

        _sb = new(256);

        if (_hudText != null)
        {
            _hudText.gameObject.SetActive(_showHud);
        }
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            _showHud = !_showHud;
            if (_hudText != null)
            {
                _hudText.gameObject.SetActive(_showHud);
            }
        }

        float dt = Time.unscaledDeltaTime;
        if (_smoothedDt <= 0.0f)
        {
            _smoothedDt = dt;//initialize
        }
        else
        {
            _smoothedDt = Mathf.Lerp(_smoothedDt, dt, _emaFactor);
        }

        if (_showHud && _hudText != null)
        {
            //frame per second
            float fps = 1.0f / Mathf.Max(_smoothedDt, 0.0001f);
            _sb.Length = 0;//buffer recycle => less GC
            _sb.Append("FPS: ").Append(fps.ToString("F1")).AppendLine();
            //deltaTime : miliSecond, *1000 => second
            _sb.Append("Frame Time: ").Append((_smoothedDt * 1000.0f).ToString("F2")).Append(" ms").AppendLine();
            
            //string + string => memory use big(rlwhs answkduf + answkduf qkdtlrdms apahfl tkdyddl zjwla)

            if (_playerTransform != null)
            {
                Vector3 p = _playerTransform.position;
                Vector3 e = _playerTransform.eulerAngles;
                _sb.Append($"Pos (m): X {p.x:F2}, Y {p.y:F2}, Z {p.z:F2}").AppendLine();
                _sb.Append($"Rot (deg): X {e.x:F1}, Y {e.y:F1}, Z {e.z:F1}").AppendLine();
            }

            _hudText.text = _sb.ToString();
        }
    }
}
