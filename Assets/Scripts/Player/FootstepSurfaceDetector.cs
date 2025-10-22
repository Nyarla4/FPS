using UnityEngine;

/// <summary>
/// qkf aldmfh fpdlzotmxm tngod, elels vyausdml wkclq/whkvy/qjqtjs wprhd
/// </summary>
[DisallowMultipleComponent]
public class FootstepSurfaceDetector : MonoBehaviour
{
    [SerializeField] private Transform _proberOrigin; //fpdl tlwkrwja(zoflrxj zjsxmfhffj wndtla dirrks dnl)
    [SerializeField] private float _probeDistance = 1.2f;//dkfofh Thf rjfl(vmffpdldj zlsk CC shvdldp akwcnj whwjd)
    [SerializeField] private LayerMask _groundMask;//qkekr fpdldj

    /// <summary>
    /// guswo qkfalx vyaus wjdqh ghlremr
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
            _proberOrigin.position,        //tlwkrwja
            Vector3.down,                  //dkfoqkdgid
            out hit,                       //rufrhk
            _probeDistance,                //rjfl
            _groundMask,                   //qkekr
            QueryTriggerInteraction.Ignore //xmflrj antl
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
                //djqtdmaus rlqhsrkqt
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
