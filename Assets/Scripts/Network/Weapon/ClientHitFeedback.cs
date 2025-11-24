using UnityEngine;

/// <summary>
/// 로컬 히트 피드백
///     카메라에서 레이 발사 => 로컬로 표면 피격 => 데칼/스파크 재생
///     서버 판정과 불일치 가능성 있음
///     즉시성(사용 사유)
/// </summary>
public class ClientHitFeedback : MonoBehaviour
{
    public FirstPersonCameraRig CameraRig;//카메라 리그 참조
    public ImpactDecalPool DecalPool;//데칼 풀
    public HitSparkFX HitSpark;//스파크 이펙트
    public float RayMaxDistance = 150.0f;//레이 최대 거리
    public LayerMask HitMask;//맞출 레이어(월드 지오메트리)


    public void TryLocalRayFeedback()
    {
        if(CameraRig == null || CameraRig.worldCamera == null)
        {
            return;
        }

        //화면 중앙기준 레이 생성
        Camera cam = CameraRig.worldCamera;//월드 카메라
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));//중앙 레이
        //0,0   1,0
        //
        //0,1   1,1
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, RayMaxDistance, HitMask, QueryTriggerInteraction.Ignore))
        {
            //데칼 및 스파크
            if(DecalPool != null)
            {
                DecalPool.SpawnDecal(hit.point, hit.normal, hit.transform.parent);
            }
            if(HitSpark != null)
            {
                HitSpark.PlayAt(hit.point, hit.normal, hit.transform.parent);
            }
        }

    }
}
