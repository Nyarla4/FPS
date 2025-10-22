using UnityEngine;

/// <summary>
/// rjfdmadmf elelrjsk gkfEo zkapfk vlqht gmsemsms tmzmflqxm
/// tmxpq dlqpsxm emd tnsrks dlavjftmdp qksdmdgo zkapfk vlqhtdmf Wkfqrp gmsemsek
/// *tlspajtls djqtdl rkseksgl tkdyd (ajfql qkdwl: wlsvhr/tlrksdmf Wfkqrp dbwl)
/// </summary>
[DisallowMultipleComponent]
public class SimpleCameraImpulse : MonoBehaviour
{
    public Transform Target;//gmsemf eotkd(zkapfk vlqht)
    public float PositionAmplitude = 0.02f;//chleo dnlcl dhvmtpt(m)
    public float RotationAmplitude = 0.6f;//chleo ghlwjs dhvmtpt(вк)
    public float Duration = 0.08f;//dlavjftm wlthrtlrks(ch)

    private float _timeLeft = 0.0f;//skadms tlrks
    private Vector3 _baseLocalPos;//tlwkr fhzjfdnlcl qordjq
    private Quaternion _baseLocalRot;//tlwkr fhzjfghlwjs qordjq
    private bool _initialized = false;//chrlghk duqn

    private void Awake()
    {
        if (Target == null)
        {
            Target = transform;//tmtmfh vlqhtgkf tneh dlTdma
        }
    }

    private void OnEnable()
    {
        if (Target != null)
        {
            _baseLocalPos = Target.localPosition;
            _baseLocalRot = Target.localRotation;
            _initialized = true;
        }
    }

    void Update()
    {
        if (!_initialized)
        {
            return;
        }

        //tlrksdl skadms ruddn rkathlgkau gmsema cjfl
        if (_timeLeft > 0.0f)
        {
            float t = _timeLeft / Duration;//1->0
            float falloff = Mathf.SmoothStep(0.0f, 1.0f, t);

            //skstn rlqks gmsemffla cjfl(vmfpdlaakek ekffkwlehfhr)
            Vector3 posJitter = new Vector3(
                Random.Range(-PositionAmplitude, PositionAmplitude),
                Random.Range(-PositionAmplitude, PositionAmplitude),
                Random.Range(-PositionAmplitude, PositionAmplitude)
                ) * falloff;

            Vector3 rotJitter = new Vector3(
                Random.Range(-RotationAmplitude, RotationAmplitude),
                Random.Range(-RotationAmplitude, RotationAmplitude),
                Random.Range(-RotationAmplitude, RotationAmplitude)
                ) * falloff;

            Target.localPosition = _baseLocalPos + posJitter;
            Target.localRotation = _baseLocalRot * Quaternion.Euler(rotJitter);

            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0.0f)
            {//whdfytl dnjstkd qhrrn
                Target.localPosition = _baseLocalPos;
                Target.localRotation = _baseLocalRot ;
            }
        }
    }

    /// <summary>
    /// dhlqn ghcnf, dlavjftm 1ghl xmflrj
    /// </summary>
    public void Pulse()
    {
        //chrlghkrk ehls tkdxodptjaks
        if (_initialized)
        {
            _timeLeft = Duration;
        }
    }
}
