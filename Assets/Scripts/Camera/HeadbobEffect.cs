using UnityEngine;
using UnityEngine.Events;

public class HeadbobEffect : MonoBehaviour, ICameraEffect
{
    [Header("Refs")]
    [SerializeField] private LocomotionFeed _feed;//wjqwl, tnvudthreh

    [Header("Shape")]
    public float FrequencyPerMps = 2.2f;    //1m/sekd wnvktn wmdrkfid
    public AnimationCurve AmplitudeBySpeed  //threh wlsvhr tmzpdlf rhrtjs (0~10 m/s)
        = AnimationCurve.EaseInOut(0f, 1.2f, 6f, 1.0f);//wjthrdptj rhkrka, rhthrdptj dhksaks

    public float BaseAmplitudeX = 0.05f;    //rlvhr whkdn wlsvhr(m)
    public float BaseAmplitudeY = 0.035f;   //rlvhr tkdgk wlsvhr(m)

    public float RunSpeedThreshold = 4.5f;  //ekfflrl dlarP
    public float AirborneDamping = 6f;      //rhdwnd rkathl threh
    public float Smooth = 16f;              //wltkd tnfua threh

    [Header("Step Events")]
    public UnityEvent OnStepLeft;   //dhlsqkf wlaus wjqchr xkdlald dlqpsxm
    public UnityEvent OnStepRight;  //dhfmsqkf wlaus wjqchr xkdlald dlqpsxm

    private float _phase;   //wlsehd dnltkd(rkeldks)
    private Vector3 _offset;//Mixerfh wjsekfgkf dnlcl dhvmtpt
    private bool _leftFootNext = true;//ekdma tmxpqdl dhlsqkfdlswl duqn
    private float _lastCycle;   //wlrwjs tkdlzmf dlseprtm(tmxpq xmflrjdyd)

    public Vector3 CurrentPositionOffset { get { return _offset; } }

    public Vector3 CurrentRotationOffestEuler { get { return Vector3.zero; } }

    public float CurrentFovOffsets { get { return 0f; } }

    void Update()
    {
        if(_feed == null)
        {
            return;
        }

        float dt = Time.deltaTime;
        float speed = _feed.HorizontalSpeed;//tnvudthreh

        float freq = FrequencyPerMps * speed;//threh rlqks wnvktn
        _phase += freq * dt * Mathf.PI * 2f;

        //tkdlzmf dlseprtm: tmxpq xkdlald wkqrl
        float cycle = _phase / (Mathf.PI * 2f);//guswo snwjr tkdlzmf tn
        if (Mathf.FloorToInt(cycle) > Mathf.FloorToInt(_lastCycle))
        {
            if (_leftFootNext)
            {
                OnStepLeft?.Invoke();
            }
            else
            {
                OnStepRight?.Invoke();
            }
            _leftFootNext = !_leftFootNext;
        }
        _lastCycle = cycle;

        //threh rlqks wlsvhr tmzpdlf
        float ampScale = 1f;
        float curveEval = AmplitudeBySpeed.Evaluate(speed);//threh => tmzpdlf
                            //vkfkalxj rkqtdmf goekd zjqmrkqt qksghks
        ampScale *= curveEval;

        if (speed > RunSpeedThreshold)
        {
            ampScale *= 1.15f;//ekfflrldptj tkfWkr wmdvhr
        }

        Vector3 target = Vector3.zero;//ahrvy dhvmtpt

        if (_feed.IsGrounded)
        {
            float x = Mathf.Sin(_phase) * BaseAmplitudeX * ampScale;//whkdn
            float y = Mathf.Abs(Mathf.Sin(_phase * 2f)) * BaseAmplitudeY * ampScale;//tkdgk(dkqqkrrka)
            target = new Vector3(x, -y, 0f);
        }
        else
        {
            target = Vector3.zero;
        }

        if (_feed.IsGrounded)
        {
            float k = 1f - Mathf.Exp(-Smooth * dt);//wltkd qhrks rPtn
                                //rjemq wprhqrkqt qksghks
            _offset = Vector3.Lerp(_offset,target,k);
        }
        else
        {
            float k = 1f - Mathf.Exp(-AirborneDamping * dt);//rhdwnd rkathl rPth
            _offset = Vector3.Lerp(_offset, Vector3.zero, k);
        }
    }
}
