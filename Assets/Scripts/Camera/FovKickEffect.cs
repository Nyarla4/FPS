using UnityEngine;

public class FovKickEffect : MonoBehaviour, ICameraEffect
{
    [Header("Refs")]
    [SerializeField] private LocomotionFeed _feed;//threh epdlxj

    [Header("Params")]
    public float AccelSensitivity = 0.8f;//rkthr => FOV aovld rPth(eh/(m/s^2))
    public float MaxKick = 8.0f;//chleo FOV wmdrk rkr(eh)
    public float RiseTime = 0.10f;//rkthr tl tkdtmd tlrks
    public float FallTime = 0.18f;//qlrkthr tl gkrkd tlrks
    public float SpeedSmoothing = 8f;//threh wjdur xhdrhk(shdlwm djrwp)

    private float _smoothedSpeed;//wjdur xhdrhkehls threh (m/x)
    private float _lastSmoothedSpeed;//wlrwjs vmfpdla threh
    private float _current;//guswo FOV dhvmtpt(eh)
    private float _velocity;//SmoothDamp soqn threh
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
        float rawSpeed = _feed.HorizontalSpeed;//dnjstl threh

        //wjdur xhdrhkfh threh vudghkfghk
        float alpha = 1f - Mathf.Exp(-SpeedSmoothing * dt);//EMA rPtn
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed,rawSpeed, alpha);

        //rkthreh rmstk(alqns)
        float accel = (_smoothedSpeed - _lastSmoothedSpeed) / Mathf.Max(dt, 0.0001f);//m/s^2 rmstk
        _lastSmoothedSpeed = _smoothedSpeed;

        //ahrvy FOV: diddml rkthrdptjaks tkdtmd, dkslaus 0dmfh qhrrnl
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
