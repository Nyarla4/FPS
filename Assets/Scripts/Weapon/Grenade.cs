using System.Threading;
using UnityEngine;

/// <summary>
/// tnfbxks: ejswlaus rnffjekslek tlrks wldus gn vhrqkf
///     vhrqkf qksrudsodptj rkathl eoalwl wjrdyd
///     Raycastfmf xhdgo wkddoanfdp akrglsms ruddn eoalwl ckeks
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Grenade : MonoBehaviour
{
    [Header("Fuse (Delay)")]
    public float FuseTime = 2.5f;//wldustlrks

    [Header("Explosion")]
    public float Radius = 6.0f;//vhrqkf qksrud
    public float MaxDamage = 120.0f;//chleo eoalwl(wndtla)
    public float MinDamage = 10.0f;//chlth eoalwl(rkwkdwkfl)
    public LayerMask DamageMask;//vlgo eotkd fpdldj
    public LayerMask OcclusionMask;//ckvP vkswjd fpdldj

    [Header("Effects (Optional)")]
    public GameObject ExplosionVfxPrefab;//vhrqkf dlvprxm
    public AudioSource AudioSource;//vhrqkf tkdnsem wotoddmf dnlgks dheldh thtm
    public AudioClip ExplosionClip;//vhrqkf tkdnsem zmfflq
    public float VfxUpOffset = 0.1f;//wlausdp dlvprxm vkrhema qkdwl dhvmtpt

    private Rigidbody _rb;//tnfbxks flwlem qkel
    private float _timer;//wldustlrks xkdlaj cpzm

    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        _timer = FuseTime;
    }

    void FixedUpdate()
    {
        _timer -= Time.fixedDeltaTime;
        if (_timer <= 0.0f)
        {
            Explode();
        }
    }

    public void Throw(Vector3 velocity, Vector3 angularVelocity)
    {
        if(_rb == null)
        {
            return;
        }

        _timer = FuseTime;
        _rb.linearVelocity = velocity;//dlehdthreh
        _rb.angularVelocity = angularVelocity; //ghlwjsthreh
    }

    private void Explode()
    {
        //dlvprxm alc tkdnsem cnffur
        if (ExplosionVfxPrefab != null)
        {
            GameObject vfx = Instantiate(ExplosionVfxPrefab, transform.position + Vector3.up * VfxUpOffset, Quaternion.identity);
        }
        if (AudioSource != null)
        {
            if (ExplosionClip != null)
            {
                AudioSource.PlayOneShot(ExplosionClip, 1.0f);
            }
        }

        //qksrud so tkdeodprp eoalwl wjrdyd
        Collider[] cols = Physics.OverlapSphere(transform.position, Radius, DamageMask, QueryTriggerInteraction.Ignore);

        for (int colIdx = 0; colIdx < cols.Length; colIdx++)
        {
            Collider c = cols[colIdx];
            if(c == null)
            {
                continue;
            }

            //IDamageable ckawh tleh
            IDamageable id = c.GetComponentInParent<IDamageable>();
            if(id == null)
            {
                continue;
            }

            //ckvPanf cpzm: tnfbxks => eotkdwndtls fpdlzotmxm
            Vector3 to = c.bounds.center - transform.position;
            float dist = to.magnitude;
            dist = Mathf.Max(dist, 0.0001f);
            Vector3 dir = to / dist;

            RaycastHit block;
            if (Physics.Raycast(transform.position, dir, out block, dist, OcclusionMask, QueryTriggerInteraction.Ignore))
            {//ckvPanfdp akrgls ruddn cjfl todfir
                continue;
            }

            //rjflrlqks tjsgud rkathl cjfl, drk 0dlaus Max, drk Radiusaus Min
            float t = Mathf.Clamp01(dist / Radius);
            float dmg = Mathf.Lerp(MaxDamage,MinDamage, t);

            //glxmwlwja shajf(qjqtjs) rPtks
            Vector3 hp = c.ClosestPoint(transform.position);
            Vector3 n = (hp-transform.position).normalized;

            id.ApplyDamage(dmg, hp, n, transform);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {//vhrqkfqksrud elqjrm
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
