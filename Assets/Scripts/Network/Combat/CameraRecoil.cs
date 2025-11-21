using UnityEngine;

/// <summary>
/// 발사시 카메라에 반동(pitch/yaw) 추가 후 시간이 흐름에 따라 부드럽게 복귀 처리
///     FirstPersonCameraRig의 pitch/yaw에 누적X 별도 오프셋 처리
///     피벗의 로컬회전에 작은 오프셋 합산 하는 식으로 구현
/// </summary>
public class CameraRecoil : MonoBehaviour
{
    public Transform CameraPivot;//회전을 줄 피벗(FirstPersonCameraRig.cameraPivot 권장)
    public float ReturnSpeed;//복귀 속도(높을수록 빠르게 복귀)

    private float _recoilPitch;//누적 반동(상하)
    private float _recoilYaw;//누적 반동(좌우)

    void Update()
    {
        //시간 흐름에 따라 원점으로 점진적 복귀
        float dt = Time.deltaTime;

        if(Mathf.Abs(_recoilPitch) > 0.0001f)
        {
            _recoilPitch = Mathf.Lerp(_recoilPitch, 0.0f, dt * ReturnSpeed);
        }
        else
        {
            _recoilPitch = 0.0f;
        }

        if(Mathf.Abs(_recoilYaw) > 0.0001f)
        {
            _recoilYaw = Mathf.Lerp(_recoilYaw, 0.0f, dt * ReturnSpeed);
        }
        else
        {
            _recoilYaw = 0.0f;
        }

        if (CameraPivot != null)
        {
            //기존회전+recoil 오프셋의 구조인 경우, 오프셋 전용 자식 피벗을 쓰는게 안전성이 높음
            CameraPivot.localRotation = Quaternion.Euler(_recoilPitch, _recoilYaw, 0.0f);
        }
    }

    /// <summary>
    /// 발사 시 상하/좌우 반동 누적
    /// </summary>
    /// <param name="pitchAmount">상하 반동 세기</param>
    /// <param name="yawAmount">좌우 반동 세기</param>
    public void Kick(float pitchAmount, float yawAmount)
    {
        _recoilPitch += pitchAmount;
        _recoilYaw += Random.Range(-yawAmount, yawAmount);
    }
}
