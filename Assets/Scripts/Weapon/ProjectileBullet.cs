using TMPro;
using UnityEngine;
/// <summary>
/// rhthr xksghks
/// dusthr cndehf cpzmfmf wkcp rngus: fpdlzotmxm(Sweep) gn dlehd (wlsks dnlcl -> guswo dnlcl)
///     =>threhrk shvdkeh qurdlsk difqdms zhffkdlej rhksxhdgkwl dksgdma
/// glxmtl IDamageablefh eoalwl wjsekf, dlavorxm dlvprxmsms tjsxor
/// tnaud wlskrh wkehd vkrhl
/// </summary>
public class ProjectileBullet : MonoBehaviour
{
    [Header("Ballistics")]
    public float Speed = 120.0f;//xks threh(m/s). aodn Qkfmek
    public bool UseGravity = false;//skrck wjrdyd duqn
    public float Gravity = -9.81f;//wndfur rkthreh(dhqtusdla)
    public float MaxLifeTime = 4.0f;//chleo todwhs tlrks(ch)
    public LayerMask HitMask;//cndehf eotkd fpdldj(qur, wlgud, zoflrxj emd)

    [Header("Damage")]
    public float Damage = 20.0f;//rlqhs eoalwl
    public float HeadshotMultiplier = 2.0f;//gpemtitgkaus 2qo

    [Header("Effects(Optional)")]
    public GameObject ImpactVfxPrefab;//vlrur wlwja VFX
    public GameObject DecalPrefab;//epzkf(tjsxor)
    public float DecalOffset = 0.01f;//vyaus vkrhemsmsrj qkdwldyd dhvmtpt

    private Vector3 _velocity;//guswo threh(wndfur vhgka)
    private Vector3 _lastPosition;//wlsks vmfpdla dnlcl(Sweep tlwkr dnlcl)
    private float _life;//skadms tnaud

    private void Awake()
    {//chrlghksms dhlqndptj Spawngks gndp whwjd
        _velocity = transform.forward * Speed;
        _lastPosition = transform.position;
        _life = 0.0f;
    }

    /// <summary>
    /// fjscjrk qkftk wlrgn ghcnf, chrl threh tpxld(ADS/tmvmfpem qksdud gn qkdgid tkdyd rnjswkd)
    /// </summary>
    public void SetInitializeVelocity(Vector3 v)
    {
        _velocity = v;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        //wndfur wjrdyd
        if (UseGravity)
        {
            _velocity.y += Gravity * dt;
        }

        //dlehdrjfl rPtks
        Vector3 start = transform.position;//gusdnlcl(Sweep tlwkrwja)
        Vector3 displacement = _velocity * dt;//dlqjs vmfpdla dlehd dPwjd qprxj
        Vector3 end = start + displacement; //dltkdwjrdls dlehd gn dnlcl

        //dusthr cndehf cpzm(Sweep): dlwjs dnlcl, dlqjs dnlcl enf ek rhfu
        //  FixedUpdate snfkr ghrdms vmfpdla wjavm qkdwlfmf dnlgks _lastPositiondptj endRkwl tmdnlq
        Vector3 sweepStart = _lastPosition;
        Vector3 sweepDir = (end - sweepStart);
        float sweepDist = sweepDir.magnitude;

        bool hitSomething = false;
        RaycastHit hitInfo = new();

        if (sweepDist > 0.0001f)
        {
            Vector3 dir = sweepDir / sweepDist;
            bool got = Physics.Raycast(sweepStart, dir, out hitInfo, sweepDist, HitMask, QueryTriggerInteraction.Ignore);
            if (got)
            {
                hitSomething = true;
            }
        }

        //glxm cjfl/dlehd
        if (hitSomething)
        {
            OnHit(hitInfo);
            //glxmwmrtl vkrhl, rhksxhddl vlfdygks ruddn tnwjd)
            Destroy(gameObject);
        }
        else
        {
            //cndehfdl djqtdmamfh dnlcl rodtls
            transform.position = end;
            
            //xksenrk wlsgodqkdgiddmf qhehfhr ghlwjs(tlrkrwjr duscnf)
            if(_velocity.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(_velocity.normalized, Vector3.up);
                transform.rotation = look;
            }
        }

        //todwhstlrks chrhktl vkrhl cjfl
        _life += dt;
        if (_life >= MaxLifeTime)
        {
            Destroy(gameObject);
        }

        //ekdma Sweepdmf dnlgo dnlcl rldjr
        _lastPosition = transform.position;
    }

    /// <summary>
    /// eotkddp akwdkTdmf Eo
    /// </summary>
    /// <param name="hit">eotkd</param>
    private void OnHit(RaycastHit hit)
    {
        //eoalwl
        float finalDamage = Damage;

        if (hit.collider.TryGetComponent<Hitbox>(out Hitbox hb))
        {//hitBoxrk dlTekaus vlgodp rPtn rhqgka
            finalDamage *= hb.DamageMultiplier;
            if (hb.Owner != null)
            {
                hb.Owner.ApplyDamage(finalDamage, hit.point, hit.normal, transform);
            }
            else
            {
                IDamageable id = hit.collider.GetComponentInParent<IDamageable>();
                if (id != null)
                {
                    id.ApplyDamage(finalDamage, hit.point, hit.normal, transform);
                }
            }
        }
        else
        {
            IDamageable id = hit.collider.GetComponentInParent<IDamageable>();
            if (id != null)
            {
                id.ApplyDamage(finalDamage, hit.point, hit.normal, transform);
            }
        }

        //dlavorxm VFX/epzkf(tjsxorwjr dyth)
        if (ImpactVfxPrefab != null)
        {
            Quaternion rot = Quaternion.LookRotation(hit.normal);
            GameObject vfx = Instantiate(ImpactVfxPrefab, hit.point + hit.normal * DecalOffset, rot);
        }

        if (DecalPrefab != null)
        {
            Quaternion rot = Quaternion.LookRotation(-hit.normal);
            GameObject decal = Instantiate(DecalPrefab, hit.point + hit.normal * DecalOffset, rot);
        }
    }
}
