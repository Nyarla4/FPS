using UnityEngine;

/// <summary>
/// 발사시 카메리 pivot에 짧은 반동 부여 (위 + 좌우 랜덤)
/// </summary>
public class RecoilApplier : MonoBehaviour
{
    public Transform CameraPivot;//회전 적용 지점(카메라 Pivot)
    public float PitchKick = 1.2f;//위로 튀는 각도
    public float YawJitter = 0.4f;//좌우 랜덤 범위
    public float ReturnTime = 0.12f;//원위치 복귀 시간(초)

    private float _timeLeft;//남은 반동 시간
    private Quaternion _baseLocalRot; //시작 회전
    private Quaternion _targetRot;//목표 회전

    private void Awake()
    {
        if(CameraPivot == null)
        {
            CameraPivot = transform;
        }
        _baseLocalRot = CameraPivot.localRotation;
        _targetRot = _baseLocalRot;
    }

    void Update()
    {
        if(_timeLeft > 0.0f)
        {
            float t = 1.0f - (_timeLeft / ReturnTime);//0 -> 1
            float smooth = Mathf.SmoothStep(0.0f, 1.0f, t);
            CameraPivot.localRotation = Quaternion.Slerp(_targetRot, _baseLocalRot, smooth);
            //킥 위치 => 원래 위치

            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0.0f)
            {
                CameraPivot.localRotation = _baseLocalRot;
            }
        }
    }

    public void Kick()
    {
        float yaw = Random.Range(-YawJitter, YawJitter);
        Quaternion kick = Quaternion.Euler(-PitchKick, yaw, 0.0f);

        _baseLocalRot = CameraPivot.localRotation;
        _targetRot = _baseLocalRot * kick;
        _timeLeft = ReturnTime;
    }
}
