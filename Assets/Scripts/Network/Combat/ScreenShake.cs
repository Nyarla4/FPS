using UnityEngine;

/// <summary>
/// 짧은 진폭으로 화면 흔드는 간단 셰이크
///     카메라 pivot의 localPosition을 약간 흔들었다가 원위치로
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public Transform ShakePivot;//흔드는 기준 피봇(카메라 부모노드 권장)
    public float ShakeReturnSpeed = 10.0f;//복귀 속도

    private float _shakeStrength;//현재 셰이크 세기
    private float _shakeTimeRemain;//남은 셰이크 시간
    private Vector3 _baseLocalPos;//원래 로컬 위치

    void Start()
    {
        if (ShakePivot != null)
        {
            _baseLocalPos = ShakePivot.localPosition;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (_shakeTimeRemain > 0.0f)
        {
            _shakeTimeRemain -= dt;

            if (ShakePivot != null)
            {
                //간단한 잡음
                Vector3 offset = new Vector3(
                    (Random.value - 0.5f) * _shakeStrength,
                    (Random.value - 0.5f) * _shakeStrength,
                    0.0f
                    );
                ShakePivot.localPosition = _baseLocalPos + offset;
            }
        }
        else
        {
            //복구
            if (ShakePivot != null)
            {
                ShakePivot.localPosition = Vector3.Lerp(ShakePivot.localPosition, _baseLocalPos, dt * ShakeReturnSpeed);
            }
        }
    }

    /// <param name="strength">세기</param>
    /// <param name="duration">지속시간</param>
    public void ShakeOnce(float strength, float duration)
    {
        _shakeStrength = strength;
        _shakeTimeRemain = duration;
    }
}
