using System.Threading;
using UnityEngine;

/// <summary>
/// 수류탄: 일정 시간 후 폭발하며 피해를 입힘
///     폭발 반경을 기준으로 주변 오브젝트에 데미지를 줌
///     Raycast를 통해 가시성(시야 차단)을 검사하여 실제로 피해가 닿는지 판별
/// Grenade: Explodes after a delay, dealing area damage.
///     Checks line-of-sight with Raycast to apply damage only to visible targets.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Grenade : MonoBehaviour
{
    [Header("Fuse (Delay)")]
    public float FuseTime = 2.5f; // 폭발까지의 지연 시간 (Delay before explosion)

    [Header("Explosion")]
    public float Radius = 6.0f; // 폭발 반경 (Explosion radius)
    public float MaxDamage = 120.0f; // 최대 데미지 (Max damage at center)
    public float MinDamage = 10.0f; // 최소 데미지 (Min damage at edge)
    public LayerMask DamageMask; // 피해를 입을 수 있는 레이어 (Damageable layers)
    public LayerMask OcclusionMask; // 시야 차단 레이어 (Occlusion/obstacle layers)

    [Header("Effects (Optional)")]
    public GameObject ExplosionVfxPrefab; // 폭발 VFX 프리팹 (Explosion visual effect prefab)
    public AudioSource AudioSource; // 폭발 사운드 재생용 AudioSource (Audio source for explosion)
    public AudioClip ExplosionClip; // 폭발 사운드 클립 (Explosion sound clip)
    public float VfxUpOffset = 0.1f; // 폭발 VFX의 상단 오프셋 (Visual offset upward)

    private Rigidbody _rb; // 수류탄의 Rigidbody (Rigidbody reference)
    private float _timer; // 폭발까지 남은 시간 (Remaining fuse timer)

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
        if (_rb == null)
        {
            return;
        }

        _timer = FuseTime;
        _rb.linearVelocity = velocity; // 선속도 설정 (Set linear velocity)
        _rb.angularVelocity = angularVelocity; // 회전속도 설정 (Set angular velocity)
    }

    private void Explode()
    {
        // VFX 및 사운드 재생 (Spawn visual and audio effects)
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

        // 폭발 반경 내의 피해 대상 검색 (Find targets in explosion radius)
        Collider[] cols = Physics.OverlapSphere(transform.position, Radius, DamageMask, QueryTriggerInteraction.Ignore);

        for (int colIdx = 0; colIdx < cols.Length; colIdx++)
        {
            Collider c = cols[colIdx];
            if (c == null)
            {
                continue;
            }

            // IDamageable 인터페이스 검사 (Check for damageable object)
            IDamageable id = c.GetComponentInParent<IDamageable>();
            if (id == null)
            {
                continue;
            }

            // 가시성 검사: 수류탄 → 대상까지 레이캐스트 (Check occlusion between grenade and target)
            Vector3 to = c.bounds.center - transform.position;
            float dist = to.magnitude;
            dist = Mathf.Max(dist, 0.0001f);
            Vector3 dir = to / dist;

            RaycastHit block;
            if (Physics.Raycast(transform.position, dir, out block, dist, OcclusionMask, QueryTriggerInteraction.Ignore))
            {
                // 시야가 막혀 있으면 피해 없음 (Skip if line of sight is blocked)
                continue;
            }

            // 거리 기반 데미지 계산 (Damage falls off with distance)
            float t = Mathf.Clamp01(dist / Radius);
            float dmg = Mathf.Lerp(MaxDamage, MinDamage, t);

            // 실제 충돌 지점 계산 (Compute nearest hit point)
            Vector3 hp = c.ClosestPoint(transform.position);
            Vector3 n = (hp - transform.position).normalized;

            id.ApplyDamage(dmg, hp, n, transform);
        }

        Destroy(gameObject); // 폭발 후 오브젝트 삭제 (Destroy grenade after explosion)
    }

    private void OnDrawGizmosSelected()
    {
        // Scene 뷰에서 폭발 반경 표시 (Draw explosion radius in Scene view)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
