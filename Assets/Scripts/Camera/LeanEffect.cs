using UnityEngine;

//wjl/dn rkthreh rlqks Lean(fhf). zhsjfld/rmqtjsghldptj cprka wkf ehla
public class LeanEffect : MonoBehaviour, ICameraEffect
{
    [Header("Refs")]
    [SerializeField] private LocomotionFeed _feed;//tnvud threh qprxj wprhd

    //Params
    public float DegreesPerMps2 = 0.9f;//1m/s^2 ekd rldnfdla rkr(eh)
    public float MaxDegrees = 14.0f;//chleo rldnfdla(eh)
    public float AccelSmooth = 10f;//rkthreh wjdur xhdrhk
    public float Response = 14f;//chlwhd rkreh EMA tnfua threh

    private Vector3 _lastVel;//wlrwjs vmfpdla tnvud threh qprxj
    private Vector3 _smoothedAccel;//vudghkfghkrehls rkthreh qprxj
    private float _currentDeg;//guswo fhf rkr(eh)

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
        Vector3 v = _feed.HorizontalVelocity;//tnvud threh qprxj
        Vector3 a = Vector3.zero; //rkthreh rmstk

        a = (v - _lastVel) / Mathf.Max(dt, 0.0001f);
        _lastVel = v;

        //rkthreh wjdur xhdrhk
        float alpha = 1f - Mathf.Exp(-AccelSmooth * dt);//EMA rPtn
        _smoothedAccel = Vector3.Lerp(_smoothedAccel, a, alpha);

        //dncmr cnrdmfh xndud
        Vector3 right = _feed.PlayerRight;//vmffpdldj dncmr cnr
        float lateralAccel = Vector3.Dot(_smoothedAccel, right);//whkdn rkthr tjdqns

        float targetDeg = DegreesPerMps2 * lateralAccel;//rkthr=>rkreh aovld
        targetDeg = Mathf.Clamp(targetDeg, -MaxDegrees, MaxDegrees);

        float k = 1f - Mathf.Exp(-Response * dt);//EMA rPtn
        _currentDeg = Mathf.Lerp(_currentDeg, targetDeg, k);
    }
}
