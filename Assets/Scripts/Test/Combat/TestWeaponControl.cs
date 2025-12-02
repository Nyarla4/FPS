using System.Collections;
using TMPro;
using UnityEngine;

public class TestWeaponControl : MonoBehaviour
{
    [SerializeField] private TestWeaponInput _input;
    [SerializeField] private GunKind_SO _gun;
    [SerializeField] private ProjectileBullet _bullet;

    private int _ammoInMag;                      // 현재 탄창 잔탄.
    private float _fireCooldown;                 // 발사 쿨다운 타이머.
    private bool _isReloading;                   // 재장전 중 여부.
    private bool _fireHeld => _input.Triggered;                      // 입력: 발사 누름.
    private bool _reload => _input.Reload;

    [SerializeField] private TMP_Text _hudText;

    [Header("Ballistics")]
    [SerializeField] private float _projectileSpeed = 120.0f; //총알 속도(m/s). 디버그 테스트용으로는 10 정도에서도 잘 작동함,
    [SerializeField] private bool _projectileUseGravity = false;

    [SerializeField] private Camera _playerCamera;

    private void Awake()
    {
        _fireCooldown = 0.0f;
        _isReloading = false;
    }

    private void Start()
    {
        _ammoInMag = _gun.MagSize;
        UpdateHud();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (_reload)
        {
            _input.ReleaseReload();
            if (_isReloading)
            {
                return;
            }
            if (_ammoInMag >= _gun.MagSize)
            {
                // 이미 가득 차 있으면 무시.
                return;
            }
            if (_gun.ReserveAmmo <= 0)
            {
                return;
            }
            StartCoroutine(CoReload());
        }

        if (_isReloading)
        {
            return;
        }

        if (_fireCooldown > 0.0f)
        {
            _fireCooldown -= dt;
            if (_fireCooldown < 0.0f)
            {
                _fireCooldown = 0.0f;
            }
        }

        if (_fireHeld)
        {
            TryFire();
        }
    }

    IEnumerator CoReload()
    {
        _isReloading = true;
        yield return new WaitForSeconds(_gun.ReloadTime);

        int needed = _gun.MagSize - _ammoInMag;
        if (needed < 0)
        {
            needed = 0;
        }
        int toLoad = Mathf.Min(needed, _gun.ReserveAmmo);

        _ammoInMag += toLoad;
        _gun.ReserveAmmo -= toLoad;

        // 실탄 한 발 장전 방식이면, 빈 탄창에서 리로드 시 +1 허용.
        if (_gun.ChamberedRound)
        {
            if (_ammoInMag > 0)
            {
                // 이미 한 발 장전되어 있다고 가정 -> 규칙에 맞게 조정 가능.
            }
        }

        UpdateHud();

        _isReloading = false;
    }

    public void UpdateHud()
    {
        var gun = _gun;
        _hudText.text = $"{_ammoInMag}/{gun.ReserveAmmo}";
    }

    public void TryFire()
    {
        if (_fireCooldown > 0.0f || _ammoInMag <= 0)
        {
            return;
        }

        --_ammoInMag;
        UpdateHud();//탄 소비시 HUD 갱신

        float interval = 1.0f / _gun.FireRate;
        _fireCooldown = interval;

        FireOne();
    }

    public void GetMagazine(int ammo)
    {
        _gun.ReserveAmmo += ammo;
    }

    /// <summary>
    /// 단발 발사
    /// </summary>
    public void FireOne()
    {
        if (_bullet == null || _playerCamera == null)
        {
            return;
        }

        //발사 위치/시야 방향 계산
        Vector3 origin = _playerCamera.transform.position;
        Vector3 forward = _playerCamera.transform.forward;

        //퍼짐이 있는 경우 약간의 랜덤 방향으로 조정
        Vector3 shotDir = forward;

        //총알 생성 후 초기 속도 설정
        ProjectileBullet p = Instantiate(_bullet, origin, Quaternion.LookRotation(shotDir));
        p.UseGravity = _projectileUseGravity;
        p.SetInitializeVelocity(shotDir * _projectileSpeed);
    }
}
