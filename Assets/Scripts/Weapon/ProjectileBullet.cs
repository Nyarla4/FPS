using TMPro;
using UnityEngine;
/// <summary>
/// 탄환 클래스
/// 디폴트 충돌 처리방식: 피직스(Sweep) 를 이용 (이전 위치 -> 현재 위치)
///     =>프레임간 빠르게 움직일 때에도 정확하게 충돌감지가 됨
/// 맞은 IDamageable에 데미지를 주며, 이펙트 데칼까지 생성
/// 일정 시간이 지나면 삭제 처리
/// </summary>
public class ProjectileBullet : MonoBehaviour
{
    [Header("Ballistics")]
    public float Speed = 120.0f;//총 속도(m/s). 기본 값
    public bool UseGravity = false;//중력 사용 여부
    public float Gravity = -9.81f;//중력 가속도(단위/s²)
    public float MaxLifeTime = 4.0f;//최대 존재 시간(초)
    public LayerMask HitMask;//충돌 대상 레이어(벽, 캐릭터, 오브젝트 등)

    [Header("Damage")]
    public float Damage = 20.0f;//기본 데미지
    public float HeadshotMultiplier = 2.0f;//헤드샷일 경우 2배

    [Header("Effects(Optional)")]
    public GameObject ImpactVfxPrefab;//피격 위치 VFX
    public GameObject DecalPrefab;//데칼(충돌)
    public float DecalOffset = 0.01f;//표면 노멀방향으로 살짝 띄운 오프셋

    private Vector3 _velocity;//현재 속도(중력 반영)
    private Vector3 _lastPosition;//이전 프레임 위치(Sweep 시작 위치)
    private float _life;//생존 시간

    private void Awake()
    {//생성시 초기화 Spawn에서 호출됨
        _velocity = transform.forward * Speed;
        _lastPosition = transform.position;
        _life = 0.0f;
    }

    /// <summary>
    /// 외부에서 발사 요청 시, 초기 속도 설정(ADS/줌상태 등 시야 방향 따라 조정됨)
    /// </summary>
    public void SetInitializeVelocity(Vector3 v)
    {
        _velocity = v;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        //중력 적용
        if (UseGravity)
        {
            _velocity.y += Gravity * dt;
        }

        //현재위치 계산
        Vector3 start = transform.position;//현재 위치(Sweep 시작점)
        Vector3 displacement = _velocity * dt;//이번 프레임 이동 벡터
        Vector3 end = start + displacement; //다음 프레임 위치

        //디폴트 충돌 처리(Sweep): 이전 위치, 현재 위치 선을 따라 레이 발사
        //  FixedUpdate 간의 프레임 이동량이 커질 때도 정확하게 충돌 처리하도록 _lastPosition에서 end까지 검출
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

        //피격 처리/이동
        if (hitSomething)
        {
            OnHit(hitInfo);
            //피격시 삭제, 관통형 탄환이 아니라면 바로 파괴)
            Destroy(gameObject);
        }
        else
        {
            //충돌하지 않았으면 위치 갱신
            transform.position = end;

            //총알의 진행방향으로 회전 갱신(자연스럽게)
            if (_velocity.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(_velocity.normalized, Vector3.up);
                transform.rotation = look;
            }
        }

        //존재시간 초과 시 삭제 처리
        _life += dt;
        if (_life >= MaxLifeTime)
        {
            Destroy(gameObject);
        }

        //다음 Sweep을 위한 위치 기록
        _lastPosition = transform.position;
    }

    /// <summary>
    /// 충돌시 호출됨
    /// </summary>
    /// <param name="hit">충돌 정보</param>
    private void OnHit(RaycastHit hit)
    {
        //데미지 계산
        float finalDamage = Damage;

        if (hit.collider.TryGetComponent<Hitbox>(out Hitbox hb))
        {//hitBox에 데미지배수 적용
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

        //이펙트 VFX/데칼(충돌지점 표시)
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
