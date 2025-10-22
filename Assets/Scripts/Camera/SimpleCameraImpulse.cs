using UnityEngine;

/// <summary>
/// 카메라에 임펄스를 줘서 진동 효과를 주는 컴포넌트
/// 특정 이벤트에 의해 트리거 되어 일시적으로 진동 효과를 발생시킴
/// *예: 폭발이나 충돌 등의 이벤트 발생 시 카메라 진동 효과 연출
/// </summary>
[DisallowMultipleComponent]
public class SimpleCameraImpulse : MonoBehaviour
{
    public Transform Target; // 카메라 대상(진동 효과)
    public float PositionAmplitude = 0.02f; // 최대 위치 오프셋(m)
    public float RotationAmplitude = 0.6f; // 최대 회전 오프셋(도)
    public float Duration = 0.08f; // 진동 지속시간(초)

    private float _timeLeft = 0.0f; // 남은 시간
    private Vector3 _baseLocalPos; // 기본 로컬 위치 저장
    private Quaternion _baseLocalRot; // 기본 로컬 회전 저장
    private bool _initialized = false; // 초기화 여부

    private void Awake()
    {
        if (Target == null)
        {
            Target = transform; // 자기 자신을 대상으로 설정
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

        // 진동의 남은 시간 동안 랜덤 오프셋 적용
        if (_timeLeft > 0.0f)
        {
            float t = _timeLeft / Duration; // 1 → 0
            float falloff = Mathf.SmoothStep(0.0f, 1.0f, t);

            // 위치 랜덤 오프셋 생성(프레임마다 갱신)
            Vector3 posJitter = new Vector3(
                Random.Range(-PositionAmplitude, PositionAmplitude),
                Random.Range(-PositionAmplitude, PositionAmplitude),
                Random.Range(-PositionAmplitude, PositionAmplitude)
                ) * falloff;

            // 회전 랜덤 오프셋 생성(프레임마다 갱신)
            Vector3 rotJitter = new Vector3(
                Random.Range(-RotationAmplitude, RotationAmplitude),
                Random.Range(-RotationAmplitude, RotationAmplitude),
                Random.Range(-RotationAmplitude, RotationAmplitude)
                ) * falloff;

            Target.localPosition = _baseLocalPos + posJitter;
            Target.localRotation = _baseLocalRot * Quaternion.Euler(rotJitter);

            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0.0f)
            { // 종료 시 원래 상태 복원
                Target.localPosition = _baseLocalPos;
                Target.localRotation = _baseLocalRot;
            }
        }
    }

    /// <summary>
    /// 외부 호출 시, 진동 효과 1회 실행
    /// </summary>
    public void Pulse()
    {
        // 초기화된 상태일 경우 실행
        if (_initialized)
        {
            _timeLeft = Duration;
        }
    }
}
