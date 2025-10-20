using UnityEngine;
using UnityEngine.Events;

public class HeadbobEffect : MonoBehaviour, ICameraEffect
{
    [Header("Refs")]
    [SerializeField] private LocomotionFeed _feed;//위치, 이동피드

    [Header("Shape")]
    public float FrequencyPerMps = 2.2f;    //1m/s당 진동주기
    public AnimationCurve AmplitudeBySpeed  //이동 진폭 조절 커브 (0~10 m/s)
        = AnimationCurve.EaseInOut(0f, 1.2f, 6f, 1.0f);//걷기일때 크고, 달릴때 완만

    public float BaseAmplitudeX = 0.05f;    //좌우 흔들림 진폭(m)
    public float BaseAmplitudeY = 0.035f;   //상하 흔들림 진폭(m)

    public float RunSpeedThreshold = 4.5f;  //달리기 기준속도
    public float AirborneDamping = 6f;      //공중 감쇠율
    public float Smooth = 16f;              //보간 반응 속도

    [Header("Step Events")]
    public UnityEvent OnStepLeft;   //왼발 디딜 때 이벤트 호출
    public UnityEvent OnStepRight;  //오른발 디딜 때 이벤트 호출

    private float _phase;   //진동 위상(라디안)
    private Vector3 _offset;//현재 적용된 위치 오프셋
    private bool _leftFootNext = true;//다음 발걸음이 왼발인지 여부
    private float _lastCycle;   //이전 사이클 값(발걸음 이벤트용)

    public Vector3 CurrentPositionOffset { get { return _offset; } }

    public Vector3 CurrentRotationOffestEuler { get { return Vector3.zero; } }

    public float CurrentFovOffsets { get { return 0f; } }

    void Update()
    {
        if (_feed == null)
        {
            return;
        }

        float dt = Time.deltaTime;
        float speed = _feed.HorizontalSpeed;//이동속도

        float freq = FrequencyPerMps * speed;//이동속도 기반 주파수
        _phase += freq * dt * Mathf.PI * 2f;

        //사이클 진행: 한 사이클마다 발걸음 이벤트 발생
        float cycle = _phase / (Mathf.PI * 2f);//현재 진행된 사이클 수
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

        //이동속도에 따른 진폭 스케일
        float ampScale = 1f;
        float curveEval = AmplitudeBySpeed.Evaluate(speed);//속도 => 진폭 변환
                                                           //걷기일때는 큼, 달릴때는 작아짐
        ampScale *= curveEval;

        if (speed > RunSpeedThreshold)
        {
            ampScale *= 1.15f;//달릴때 약간 증가
        }

        Vector3 target = Vector3.zero;//목표 오프셋

        if (_feed.IsGrounded)
        {
            float x = Mathf.Sin(_phase) * BaseAmplitudeX * ampScale;//좌우 흔들림
            float y = Mathf.Abs(Mathf.Sin(_phase * 2f)) * BaseAmplitudeY * ampScale;//상하 흔들림(두배속)
            target = new Vector3(x, -y, 0f);
        }
        else
        {
            target = Vector3.zero;
        }

        if (_feed.IsGrounded)
        {
            float k = 1f - Mathf.Exp(-Smooth * dt);//보간 속도 반응
                                                   //가속도 기반 보간
            _offset = Vector3.Lerp(_offset, target, k);
        }
        else
        {
            float k = 1f - Mathf.Exp(-AirborneDamping * dt);//공중 감쇠 반응
            _offset = Vector3.Lerp(_offset, Vector3.zero, k);
        }
    }
}
