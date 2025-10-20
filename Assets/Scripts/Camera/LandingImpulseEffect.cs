using System;
using UnityEngine;

// 착지 가속도를 AnimationCurve로 매핑해 '충격 강도'를 계산하고, 그에 따른 흔들림 효과를 적용하는 스크립트
public class LandingImpulseEffect : MonoBehaviour, ICameraEffect
{
    [Header("Refs")]
    [SerializeField] private LocomotionFeed _feed; // 이동, 속도 정보

    [Header("Mapping")]
    public float MaxConsideredFallSpeed = 12f;  // 고려할 최대 낙하 속도
    public AnimationCurve StrengthByFall        // 착지 속도(0~1) → 강도(0~1)
        = AnimationCurve.EaseInOut(0f, 0.6f, 1f, 1.0f); // 낮은 착지 속도일수록 완만한 강도 곡선

    [Header("Impulse")]
    public float AmplitudeScale = 0.14f;  // 최대 강도의 Y 진폭(m)
    public float Damping = 10f;           // 감쇠율 계수
    public float OscillationHz = 6.5f;    // 진동 주파수(Hz)
    public float TiltDegrees = 3.5f;      // 회전 기울기 각도

    private bool wasGrounded;       // 이전 프레임에서 지면에 닿아 있었는가
    private float timeSinceImpact;  // 착지 후 경과 시간
    private float impactStrength;   // 0~1 강도 값

    private Vector3 _posOffset;
    private Vector3 _rotOffset;

    public Vector3 CurrentPositionOffset { get { return _posOffset; } }
    public Vector3 CurrentRotationOffestEuler { get { return _rotOffset; } }
    public float CurrentFovOffsets { get { return 0f; } }

    void Update()
    {
        if (_feed == null)
        {
            return;
        }

        bool grounded = _feed.IsGrounded;

        if (grounded && !wasGrounded)
        { // 착지 순간
            float vyAbs = Mathf.Abs(_feed.VerticalVelocity);     // 착지 속도 |Vy|
            float tNorm = Mathf.Clamp01(vyAbs / MaxConsideredFallSpeed); // 0~1 정규화된 속도
            float mapped = StrengthByFall.Evaluate(tNorm);       // 곡선을 통한 강도 계산
            impactStrength = mapped;
            timeSinceImpact = 0f;
        }

        wasGrounded = grounded;

        float dt = Time.deltaTime;
        timeSinceImpact += dt;

        float A = AmplitudeScale * impactStrength;                  // 진폭 계산
        float decay = Mathf.Exp(-Damping * timeSinceImpact);        // 감쇠 계산
        float y = A * decay * (1f - Mathf.Cos(2f * Mathf.PI * OscillationHz * timeSinceImpact));

        _posOffset = new Vector3(0f, -y, 0f);
        _rotOffset = new Vector3(TiltDegrees * impactStrength * decay, 0f, 0f);
    }
}
