using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 발사 원리: 총알/투사 방향에서 ProjectileBullet 생성/발사 처리
///     ADS/줌상태일 때는 보다 정밀하게 발사방향 계산해야 함
///     퍼짐각과 랜덤함수로 각 탄환의 방향을 약간씩 흩어지게 조정
/// </summary>
public class WeaponProjectileLauncher : MonoBehaviour
{
    [Header("Spawn")]
    public Transform Muzzle; //발사 위치(총구 또는 총알 위치)
    public Camera PlayerCamara; //플레이어 시야
    public ProjectileBullet ProjectilePrefab; //총알 프리팹
    public LayerMask FireMask; //발사시 충돌 제외 마스크(카메라용 레이어): 플레이어는 피격대상에서 제외됨

    [Header("Ballistics")]
    public float ProjectileSpeed = 120.0f; //총알 속도(m/s). 디버그 테스트용으로는 10 정도에서도 잘 작동함,
    public bool ProjectileUseGravity = false;

    [Header("Spread (Optional)")]
    public float SpreadDeg = 0.0f; //퍼짐각도. 0이면 완전 직선 발사
    public bool UseConeCosineBias = true; //랜덤 분포 조절용 옵션

    /// <summary>
    /// 단발 발사
    /// </summary>
    public void FireOne()
    {
        if (ProjectilePrefab == null || PlayerCamara == null)
        {
            return;
        }

        //발사 위치/시야 방향 계산
        Vector3 origin = Muzzle != null ? Muzzle.position : PlayerCamara.transform.position;
        Vector3 forward = PlayerCamara.transform.forward;

        //퍼짐이 있는 경우 약간의 랜덤 방향으로 조정
        Vector3 shotDir = forward;
        if (SpreadDeg > 0.0001f)
        {
            shotDir = SampleDirectionInCone(forward, SpreadDeg, UseConeCosineBias);
        }

        //총알 생성 후 초기 속도 설정
        ProjectileBullet p = Instantiate(ProjectilePrefab, origin, Quaternion.LookRotation(shotDir));
        p.UseGravity = ProjectileUseGravity;
        p.SetInitializeVelocity(shotDir * ProjectileSpeed);
    }

    /// <summary>
    /// 랜덤한 시야 방향 계산 함수
    /// </summary>
    private Vector3 SampleDirectionInCone(Vector3 forward, float coneAngleDeg, bool cosineBias)
    {
        float halfRad = coneAngleDeg * 0.5f * Mathf.Deg2Rad;
        float tan = Mathf.Tan(halfRad);

        //0~1 랜덤 값 두 개 생성
        float u = Random.value;
        float v = Random.value;

        float r = u;
        if (cosineBias)
        {
            //랜덤 분포를 조금 더 중심으로 몰리게 조정
            r = Mathf.Pow(u, 0.35f);
        }
        float theta = 2.0f * Mathf.PI * v;

        float x = Mathf.Cos(theta) * r * tan;
        float y = Mathf.Sin(theta) * r * tan;

        Vector3 right = Vector3.Cross(forward, Vector3.up); //방향의 오른쪽 => 만약 방향이 수직인 경우 대체 방향 계산
        if (right.sqrMagnitude < 0.000001f)
        {
            right = Vector3.Cross(forward, Vector3.forward);
        }
        right.Normalize();

        Vector3 up = Vector3.Cross(right, forward).normalized;

        Vector3 dir = (forward + right * x + up * y).normalized;
        return dir;
    }
}
