using UnityEngine;

public class FovKickEffect : MonoBehaviour, ICameraEffect
{
    [Header("Refs")]
    [SerializeField] private LocomotionFeed _feed; // 이동 피드백

    [Header("Params")]
    public float AccelSensitivity = 0.8f; // 가속도 → FOV 변화 감도 (m/s^2)
    public float MaxKick = 8.0f; // 최대 FOV 변화 값
    public float RiseTime = 0.10f; // 가속도 시 증가 반응 시간
    public float FallTime = 0.18f; // 감속도 시 감소 반응 시간
    public float SpeedSmoothing = 8f; // 이동 속도 스무딩(평균화 강도)

    private float _smoothedSpeed; // 스무딩된 이동 속도 (m/s)
    private float _lastSmoothedSpeed; // 이전 프레임의 속도
    private float _current; // 현재 FOV 오프셋 값
    private float _velocity; // SmoothDamp용 보조 값
    public Vector3 CurrentPositionOffset { get { return Vector3.zero; } }

    public Vector3 CurrentRotationOffestEuler { get { return Vector3.zero; } }

    public float CurrentFovOffsets { get { return _current; } }

    void Update()
    {
        if (_feed == null)
        {
            return;
        }

        float dt = Time.deltaTime;
        float rawSpeed = _feed.HorizontalSpeed; // 실제 이동 속도

        // 스무딩된 이동 속도 업데이트
        float alpha = 1f - Mathf.Exp(-SpeedSmoothing * dt); // EMA 계수
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, rawSpeed, alpha);

        // 가속도 계산(차분)
        float accel = (_smoothedSpeed - _lastSmoothedSpeed) / Mathf.Max(dt, 0.0001f); // m/s^2 단위
        _lastSmoothedSpeed = _smoothedSpeed;

        // 전방 FOV: 일정한 가속도일 때 증가, 감속 시 원래값 복귀
        float target = 0f;
        if (accel > 0f)
        {
            target = Mathf.Clamp(accel * AccelSensitivity, 0f, MaxKick);
        }
        else
        {
            target = 0f;
        }

        float smoothTime = 0.15f;
        if (target > _current)
        {
            smoothTime = RiseTime;
        }
        else
        {
            smoothTime = FallTime;
        }

        _current = Mathf.SmoothDamp(_current, target, ref _velocity, smoothTime, Mathf.Infinity, dt);
    }
}
