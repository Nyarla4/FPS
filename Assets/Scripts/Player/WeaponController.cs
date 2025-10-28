using UnityEngine;
using System.Collections;

/// <summary>
/// 무기의 핵심 로직: 탄약, 연사 쿨다운, 재장전, ADS(FOV), 히트스캔 사격, 반동 이벤트.
/// 상태: Idle / Firing / Reloading / Ads(보조 상태 플래그).
/// </summary>
[DisallowMultipleComponent]
public class WeaponController : MonoBehaviour
{
    [Header("Refs")]
    public Camera PlayerCamera;                 // 레이캐스트/ADS FOV 기준.
    public ParticleSystem MuzzleFlash;          // 총구 화염.
    [SerializeField] private AudioSource _audioSource;             // 무기 사운드.
    public Transform RaycastOrigin;             // 레이 시작(카메라 중앙이 권장)
    public LayerMask HitMask;                   // 맞을 수 있는 레이어.
    public RecoilApplier Recoil;                // 반동 적용(카메라 킥)
    public CrosshairUI Crosshair;               // 크로스헤어 확산 반영.
    public AmmoHUD AmmoHud;                     // 탄약 표시 TMP.

    [Header("Ammo")]
    public int MagSize = 30;                    // 탄창 크기.
    public int ReserveAmmo = 90;                // 예비 탄약.
    public float ReloadTime = 1.9f;             // 재장전 시간(초)
    public bool ChamberedRound = true;          // 실탄 한 발 장전 방식(선택)

    [Header("Fire")]
    public float FireRate = 10.0f;              // 초당 발사수(10 → 0.1초 간격)
    public float Damage = 20.0f;                // 히트 데미지(데모용)
    public float MaxRange = 150.0f;             // 히트스캔 최대 사거리.
    public AudioClip ShotSfx;                   // 발사음.
    public AudioClip DrySfx;                    // 빈 탄창 클릭음.

    [Header("ADS")]
    public float AdsFov = 55.0f;                // ADS 시 FOV
    public float HipFov = 70.0f;                // 허리쏘기 FOV
    public float AdsBlendTime = 0.12f;          // FOV 전환 시간(초)
    public bool ApplyMouseSensitivityScale = false; // (선택) 마우스 감도 배율 적용.
    public float AdsMouseScale = 0.8f;          // ADS 시 감도 배율.

    [Header("VFX")]
    public ParticleSystem HitVfxPrefab;         // 피격 스파크/먼지.
    public GameObject BulletDecalPrefab;        // 데칼(선택)

    public ImpactEffectRouter ImpactRouter;//표면 임팩트 라우터
    public bool ApplyHitboxMultiplier = true;//히트박스 배수 적용 여부

    // 내부 상태
    private int _ammoInMag;                      // 현재 탄창 잔탄.
    private float _fireCooldown;                 // 발사 쿨다운 타이머.
    private bool _isReloading;                   // 재장전 중 여부.
    private bool _fireHeld;                      // 입력: 발사 누름.
    private bool _adsHeld;                       // 입력: ADS 누름.
    private float _fovVel;                       // FOV SmoothDamp 속도 캐시.

    public WeaponProjectileLauncher Launcher;//총알 Prefab 발사를 위한 참조용
    public bool UseBulletPrefab;

    private void Start()
    {
        _ammoInMag = MagSize; // 시작 시 가득 장전.
        if (PlayerCamera != null)
        {
            //fov 초기화
            PlayerCamera.fieldOfView = HipFov;
        }
        //ui 초기화
        UpdateAmmoHud();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 1) FOV(ADS) 전환.
        UpdateAdsFov(dt);//조준 상태로 처리

        // 2) 재장전 중이면 발사 금지.
        if (_isReloading == true)
        {
            // 쿨다운도 천천히 복귀(불필요한 잔상 방지)
            if (_fireCooldown > 0.0f)
            {
                _fireCooldown -= dt;
                if (_fireCooldown < 0.0f)
                {
                    _fireCooldown = 0.0f;
                }
            }
            return;
        }

        // 3) 발사 쿨다운 갱신.
        if (_fireCooldown > 0.0f)
        {
            _fireCooldown -= dt;
            if (_fireCooldown < 0.0f)
            {
                _fireCooldown = 0.0f;
            }
        }

        // 4) 발사 처리(자동사격: 버튼 유지 시 연사)
        if (_fireHeld)
        {
            TryFire();
            //if(Launcher != null && UseBulletPrefab)
            //{
            //    Launcher.FireOne();
            //}
            //else
            //{
            //    TryFire();
            //}
        }
    }

    // ===== 입력 바인딩 =====
    public void SetFireHeld(bool held)
    {
        _fireHeld = held;
    }

    public void SetAdsHeld(bool held)
    {
        _adsHeld = held;
    }

    public void RequestReload()
    {
        if (_isReloading == true)
        {
            return;
        }
        if (_ammoInMag >= MagSize)
        {
            // 이미 가득 차 있으면 무시.
            return;
        }
        if (ReserveAmmo <= 0)
        {
            return;
        }
        //재장전 중 애니메이션이 들어갈 수 있음 => 코루틴 사용 이유
        StartCoroutine(CoReload());
    }

    // ===== 핵심 로직 =====

    private void TryFire()
    {
        // 1) 쿨다운 확인.
        if (_fireCooldown > 0.0f)
        {
            return;
        }

        // 2) 탄약 확인.
        if (_ammoInMag <= 0)
        {
            PlayDryFire();
            // 쿨다운을 약간 줘서 클릭 중복 억제.
            _fireCooldown = 0.2f;
            return;
        }

        // 3) 1발 소비.
        --_ammoInMag;
        UpdateAmmoHud();//탄 소비시 HUD 갱신

        // 4) 발사 쿨다운 셋.
        float interval = 1.0f / FireRate;
        _fireCooldown = interval;

        // 5) 이펙트/사운드/반동.
        PlayMuzzle();
        PlayShotSfx();
        ApplyRecoilKick();

        // 6) 히트스캔.
        if (Launcher != null && UseBulletPrefab)
        {
            Launcher.FireOne();
        }
        else
        {
            DoHitscan();
        }

        // 7) 크로스헤어 확산 펄스.
        if (Crosshair != null)
        {
            Crosshair.PulseFireSpread();
        }
    }

    private IEnumerator CoReload()
    {
        _isReloading = true;

        // (선택) 재장전 사운드/애니메이션 훅.
        // audioSource.PlayOneShot(reloadSfx);

        yield return new WaitForSeconds(ReloadTime);

        int needed = MagSize - _ammoInMag;
        if (needed < 0)
        {
            needed = 0;
        }
        int toLoad = Mathf.Min(needed, ReserveAmmo);

        _ammoInMag += toLoad;
        ReserveAmmo -= toLoad;

        // 실탄 한 발 장전 방식이면, 빈 탄창에서 리로드 시 +1 허용.
        if (ChamberedRound == true)
        {
            if (_ammoInMag > 0)
            {
                // 이미 한 발 장전되어 있다고 가정 -> 규칙에 맞게 조정 가능.
            }
        }

        UpdateAmmoHud();
        _isReloading = false;
    }

    private void DoHitscan()
    {
        if (PlayerCamera == null)
        {
            return;
        }

        // 카메라 정중앙에서 레이캐스트.
        Ray ray = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);
        RaycastHit hit;
        bool got = Physics.Raycast(ray, out hit, MaxRange, HitMask, QueryTriggerInteraction.Ignore);

        if (got == true)
        {
            float finalDamage = Damage;

            Hitbox hb = hit.collider.GetComponent<Hitbox>();
            if (hb != null)
            {
                Debug.Log($"{hb.gameObject.name} hit");
                if (ApplyHitboxMultiplier)
                {
                    finalDamage = Damage * hb.DamageMultiplier;
                }

                if (hb.Owner != null)
                {
                    Debug.Log($"damage: {finalDamage}");
                    hb.Owner.ApplyDamage(finalDamage, hit.point, hit.normal, transform);
                }
                else
                {
                    IDamageable id = hit.collider.GetComponentInParent<IDamageable>();
                    if (id != null)
                    {
                        Debug.Log($"damage: {finalDamage}");
                        id.ApplyDamage(finalDamage, hit.point, hit.normal, transform);
                    }
                }
            }
            else
            {
                IDamageable damage = hit.collider.GetComponentInParent<IDamageable>();
                if (damage != null)
                {
                    Debug.Log($"damage: {finalDamage}");
                    damage.ApplyDamage(finalDamage, hit.point, hit.normal, transform);
                }
            }

            // 데미지 인터페이스가 있다면 TryGetComponent로 처리 가능(데모에선 VFX/데칼만)

            //표면 임팩트(오디오, VFX, 데칼)
            if (ImpactRouter != null)
            {
                ImpactRouter.SpawnImpact(hit);
            }
            else
            {//라우터 없으면 기본 호출
                //정면으로 레이 발사 => 명중하는 오브젝트 확인 => 해당 오브젝트가 맞는 VFX 처리
                SpawnHitVfx(hit.point, hit.normal);
                SpawnDecal(hit.point, hit.normal);
            }
        }
    }

    private void SpawnHitVfx(Vector3 point, Vector3 normal)
    {
        if (HitVfxPrefab == null)
        {
            return;
        }

        Quaternion rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(normal, Vector3.up), normal);
        ParticleSystem vfx = Instantiate(HitVfxPrefab, point + normal * 0.01f, rot);
        // 자동 파괴 설정이 프리팹에 있지 않다면, 수명 후 Destroy 추가 가능.
    }

    private void SpawnDecal(Vector3 point, Vector3 normal)
    {
        if (BulletDecalPrefab == null)
        {
            return;
        }

        Quaternion rot = Quaternion.LookRotation(-normal, Vector3.up);
        GameObject decal = Instantiate(BulletDecalPrefab, point + normal * 0.002f, rot);
        // 필요하면 부모를 히트한 콜라이더로 설정하여 이동에 추적.
    }

    private void PlayMuzzle()
    {
        if (MuzzleFlash != null)
        {
            MuzzleFlash.Play();
        }
    }

    private void PlayShotSfx()
    {
        if (_audioSource != null)
        {
            if (ShotSfx != null)
            {
                _audioSource.PlayOneShot(ShotSfx, 1.0f);
            }
        }
    }

    private void PlayDryFire()
    {
        if (_audioSource != null)
        {
            if (DrySfx != null)
            {
                _audioSource.PlayOneShot(DrySfx, 1.0f);
            }
        }
    }

    private void ApplyRecoilKick()
    {
        if (Recoil != null)
        {
            Recoil.Kick();
        }
    }

    private void UpdateAdsFov(float dt)
    {
        if (PlayerCamera == null)
        {
            return;
        }

        float target = HipFov;
        if (_adsHeld == true)
        {
            target = AdsFov;
        }

        // SmoothDamp 기반 FOV 전환.
        float newFov = Mathf.SmoothDamp(PlayerCamera.fieldOfView, target, ref _fovVel, AdsBlendTime);
        PlayerCamera.fieldOfView = newFov;

        // (선택) 마우스 감도 배율.
        if (ApplyMouseSensitivityScale == true)
        {
            MouseLook ml = PlayerCamera.GetComponentInParent<MouseLook>();
            if (ml != null)
            {
                if (_adsHeld == true)
                {
                    ml.SetSensitivityMultiplier(AdsMouseScale);
                }
                else
                {
                    ml.SetSensitivityMultiplier(1.0f);
                }
            }
        }
    }

    private void UpdateAmmoHud()
    {
        if (AmmoHud != null)
        {   //남은 탄량 표시
            AmmoHud.SetAmmo(_ammoInMag, ReserveAmmo);
        }
    }
}