using UnityEngine;

// 카메라 회전 기울기(Lean) 효과. 가속도/감속도에 따라 카메라가 기울어지는 효과를 줌
public class LeanEffect : MonoBehaviour, ICameraEffect
{
    [Header("Refs")]
    [SerializeField] private LocomotionFeed _feed; // 이동 정보 제공 컴포넌트

    [Header("Params")]
    public float DegreesPerMps2 = 0.9f;   // 1m/s^2당 기울어지는 각도(도)
    public float MaxDegrees = 14.0f;      // 최대 기울기 각도(도)
    public float AccelSmooth = 10f;       // 가속도 변화 부드럽게 처리
    public float Response = 14f;          // 회전 반응 속도 (EMA 보간 계수)

    private Vector3 _lastVel;        // 이전 프레임의 이동 속도
    private Vector3 _smoothedAccel;  // 부드럽게 보정된 가속도 벡터
    private float _currentDeg;       // 현재 기울어진 각도(도)

    public Vector3 CurrentPositionOffset { get { return Vector3.zero; } }

    public Vector3 CurrentRotationOffestEuler { get { return new Vector3(0f, 0f, -_currentDeg); } }

    public float CurrentFovOffsets { get { return 0f; } }

    void Update()
    {
        if (_feed == null)
        {
            return;
        }

        float dt = Time.deltaTime;
        Vector3 v = _feed.HorizontalVelocity; // 현재 이동 속도
        Vector3 a = Vector3.zero;             // 가속도 벡터 초기화

        a = (v - _lastVel) / Mathf.Max(dt, 0.0001f);
        _lastVel = v;

        // 가속도 부드럽게 처리
        float alpha = 1f - Mathf.Exp(-AccelSmooth * dt); // EMA 계수
        _smoothedAccel = Vector3.Lerp(_smoothedAccel, a, alpha);

        // 좌우 방향 가속도 계산
        Vector3 right = _feed.PlayerRight; // 플레이어의 오른쪽 방향 벡터
        float lateralAccel = Vector3.Dot(_smoothedAccel, right); // 수평 가속도 추출

        float targetDeg = DegreesPerMps2 * lateralAccel; // 가속도 → 회전각 변환
        targetDeg = Mathf.Clamp(targetDeg, -MaxDegrees, MaxDegrees);

        float k = 1f - Mathf.Exp(-Response * dt); // EMA 계수
        _currentDeg = Mathf.Lerp(_currentDeg, targetDeg, k);
    }
}
