using System;
using UnityEngine;

//skrgkthrehfmf AnimationCurvefh 'cndrur rkdeh'dp aovldgo ckrwl dlavjftmfmf ej tlrkr clsghkwjrdmfh
public class LandingImpulseEffect : MonoBehaviour, ICameraEffect
{
    [Header("Refs")]
    [SerializeField] private LocomotionFeed _feed;//wjqwl, tnwlrthreh

    [Header("Mapping")]
    public float MaxConsideredFallSpeed = 12f;  //wjdrbghk tkdgks
    public AnimationCurve StrengthByFall        //skrgkthreh(0~1) => rkdeh(0~1)
        = AnimationCurve.EaseInOut(0f, 0.6f, 1f, 1.0f);//wjskrgkdptjeh whsworka dlTehfhr

    [Header("Impulse")]
    public float AmplitudeScale = 0.14f;//chleo rkdehtl Y wlsvhr(m)
    public float Damping = 10f;//rkathl rPtn
    public float OscillationHz=6.5f;//wkswlsehd qlseh
    public float TiltDegrees = 3.5f;//vlcl tnrdla
    
    private bool wasGrounded;//wjs vmfpdla wjqwl tkdxo
    private float timeSinceImpact;//dlavjftm rudrhk tlrks
    private float impactStrength;//0~1 rkdeh

    private Vector3 _posOffset;
    private Vector3 _rotOffset;

    public Vector3 CurrentPositionOffset { get { return _posOffset; } }
    public Vector3 CurrentRotationOffestEuler { get { return _rotOffset; } }
    public float CurrentFovOffsets { get { return 0f; } }

    void Update()
    {
        if(_feed == null)
        {
            return;
        }

        bool grounded = _feed.IsGrounded;

        if(grounded && !wasGrounded)
        {//rkt ckrwlgka
            float vyAbs = Mathf.Abs(_feed.VerticalVelocity);//ckrwl wlrwjs |Vy|
            float tNorm = Mathf.Clamp01(vyAbs / MaxConsideredFallSpeed);//0~1 wjdrbghk
            float mapped = StrengthByFall.Evaluate(tNorm);//rhrtjs aovld rkdeh
            impactStrength = mapped;
            timeSinceImpact = 0f;
        }

        wasGrounded = grounded;

        float dt = Time.deltaTime;
        timeSinceImpact += dt;

        float A = AmplitudeScale * impactStrength;//wlsvhr
        float decay = Mathf.Exp(-Damping * timeSinceImpact);//rkathl
        float y = A * decay * (1f - Mathf.Cos(2f * Mathf.PI * OscillationHz * timeSinceImpact));

        _posOffset = new Vector3(0f, -y, 0f);
        _rotOffset = new Vector3(TiltDegrees * impactStrength * decay, 0f, 0f);
    }
}
