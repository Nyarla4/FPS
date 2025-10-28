using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// qkftk anrl: zkapfk/chdrn qkdgiddmfh ProjectileBullet todtjd/qkftk cjfl
///     ADS/tmvmfpem dlTsms ruddn dhlqndptj qkftkqkdgid rPtksgotj sjarlf tn dlTdma
///     ekstnsgks qjwjsdmfh ekd tmzmflqxm soqndptj vjwla toavmffld rksmd
/// </summary>
public class WeaponProjectileLauncher : MonoBehaviour
{
    [Header("Spawn")]
    public Transform Muzzle;//qkftk dnjswja(djqtsms ruddn zkapfk dnlcl)
    public Camera PlayerCamara;//whwns rlwns
    public ProjectileBullet ProjectilePrefab;//xks vmflvoq
    public LayerMask FireMask;//tkrurtl fpdlfh cjt akwcnawja(tjsxorwjr dyth): whwnstjs wjdfufdydeh

    [Header("Ballistics")]
    public float ProjectileSpeed = 120.0f;//chrl thrfur(m/s). elqjrm ghkrdlsdyddmfhsms 10 wjdehfhaks goeh ehlsek, 
    public bool ProjectileUseGravity = false;

    [Header("Spread (Optional)")]
    public float SpreadDeg = 0.0f;//rkseksgks vjwla. 0dlaus wjdghkrgkrp wjsqkd
    public bool UseConeCosineBias = true;//wndtla alfeh shvdms toavmffld duqn

    /// <summary>
    /// eksqkf tkrur
    /// </summary>
    public void FireOne()
    {
        if (ProjectilePrefab == null || PlayerCamara == null)
        {
            return;
        }

        //qkftk dnjswja/rlwns qkdgid
        Vector3 origin = Muzzle != null ? Muzzle.position : PlayerCamara.transform.position;
        Vector3 forward = PlayerCamara.transform.forward;

        //vjwladl dlTsms ruddn dnjsQnf sodptj goekd qkdgiddmfh toavmf
        Vector3 shotDir = forward;
        if(SpreadDeg > 0.0001f)
        {
            shotDir = SampleDirectionInCone(forward, SpreadDeg, UseConeCosineBias);
        }

        //xksghks todtjd alc chrl threh tpxld
        ProjectileBullet p = Instantiate(ProjectilePrefab, origin, Quaternion.LookRotation(shotDir));
        p.UseGravity = ProjectileUseGravity;
        p.SetInitializeVelocity(shotDir * ProjectileSpeed);
    }

    /// <summary>
    /// dnjsqnf so qkdgid toavmffld
    /// </summary>
    private Vector3 SampleDirectionInCone(Vector3 forward, float coneAngleDeg, bool cosineBias)
    {
        float halfRad = coneAngleDeg * 0.5f * Mathf.Deg2Rad;
        float tan = Mathf.Tan(halfRad);

        //0~1 qnehd thtn skstn
        float u = Random.value;
        float v = Random.value;

        float r = u;
        if (cosineBias)
        {
            //wndtla alfeh tkdtmddmf dnlgks wltn qhwjd
            r = Mathf.Pow(u, 0.35f);
        }
        float theta = 2.0f * Mathf.PI * v;

        float x = Mathf.Cos(theta) * r * tan;
        float y = Mathf.Sin(theta) * r * tan;

        Vector3 right = Vector3.Cross(forward, Vector3.up);//qprxjdml dhlwjr => en qprxjdp tnwlrdls tofhdns qprxj qksghks
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
