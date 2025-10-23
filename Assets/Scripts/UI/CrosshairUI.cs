using UnityEngine;

/// <summary>
/// 4분할 이미지(상/하/좌/우)를 사용하여 확산 시각화
/// 이동속도와 사격 이벤트에 반응하여 확산값 증감
/// </summary>
public class CrosshairUI : MonoBehaviour
{
    [Header("Refs")]
    public CharacterController Controller;//이동속도 참조
    [SerializeField] private RectTransform _topPart;
    [SerializeField] private RectTransform _bottomPart;
    [SerializeField] private RectTransform _leftPart;
    [SerializeField] private RectTransform _rightPart;

    [Header("Spread")]
    public float BaseSpread = 6f;//기본간격(단위 : px)
    public float MoveSpreadFactor = 10f;//이동 속도당 가산(px per m/s)
    public float FireKick = 12f;//발사시 즉시 가산(px)
    public float DecaySpeed = 40f;//초당 감쇠(px/s)
    public float MaxSpread = 40f;//최대 확산(px)

    private float _currentSpread;// 현재 확산(px)

    private void Awake()
    {
        if(Controller == null)
        {
            Controller = FindAnyObjectByType<CharacterController>();
        }

        _currentSpread = BaseSpread;
    }

    void Update()
    {
        //이동속도 기반으로 목표 확산
        float planarSpeed = 0.0f;
        if (Controller != null)
        {
            Vector3 v = Controller.velocity;
            v.y = 0.0f;
            planarSpeed = v.magnitude;
        }

        float targetSpread = BaseSpread + MoveSpreadFactor * planarSpeed;

        //현재 확산으로 목표 확산으로 감쇠 이동
        if(_currentSpread > targetSpread)
        {
            _currentSpread = Mathf.Max(_currentSpread - (DecaySpeed * Time.deltaTime), targetSpread);
        }
        else
        {
            _currentSpread = Mathf.Min(_currentSpread + (DecaySpeed * Time.deltaTime), targetSpread);
        }

        _currentSpread = Mathf.Min(_currentSpread, MaxSpread);

        //UI에 최종 확산 반영
        ApplySpread(_currentSpread);
    }

    public void PulseFireSpread()
    {
        _currentSpread += FireKick;
        _currentSpread = Mathf.Min(_currentSpread, MaxSpread);
    }

    private void ApplySpread(float spread)
    {
        if (_topPart != null)
        {
            _topPart.anchoredPosition = Vector2.up * spread;
        }
        if (_bottomPart != null)
        {
            _bottomPart.anchoredPosition = Vector2.down * spread;
        }
        if (_leftPart != null)
        {
            _leftPart.anchoredPosition = Vector2.left * spread;
        }
        if (_rightPart != null)
        {
            _rightPart.anchoredPosition = Vector2.right * spread;
        }
    }
}
