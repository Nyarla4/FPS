using UnityEngine;

/// <summary>
/// 발 밑의 표면을 탐지, 해당 표면의 종류/위치/법선을 반환
/// </summary>
[DisallowMultipleComponent]
public class FootstepSurfaceDetector : MonoBehaviour
{
    [SerializeField] private Transform _proberOrigin; //표면 탐지용(보통 캐릭터 발밑에 위치한 트랜스폼)
    [SerializeField] private float _probeDistance = 1.2f;//레이 길이 (기본적으로 지면 체크 용도)
    [SerializeField] private LayerMask _groundMask;//표면 레이어

    /// <summary>
    /// 현재 발밑 표면 정보를 가져옴
    /// </summary>
    public bool TryGetSurface(out SurfaceType surface, out Vector3 point, out Vector3 normal)
    {
        surface = SurfaceType.Concrete;
        point = Vector3.zero;
        normal = Vector3.up;

        if (_proberOrigin == null)
        {
            return false;
        }

        RaycastHit hit;
        bool got = Physics.Raycast(
            _proberOrigin.position,        //탐지원
            Vector3.down,                  //레이방향
            out hit,                       //결과값
            _probeDistance,                //길이
            _groundMask,                   //표면
            QueryTriggerInteraction.Ignore //트리거 무시
            );

        if (got)
        {
            point = hit.point;
            normal = hit.normal;

            SurfaceMaterial s = hit.collider.GetComponent<SurfaceMaterial>();
            if (s != null)
            {
                surface = s.surfaceType;
                return true;
            }
            else
            {
                //컴포넌트가 없을때
                surface = SurfaceType.Concrete;
                return true;
            }
        }

        return false;
    }
    void Update()
    {

    }
}
