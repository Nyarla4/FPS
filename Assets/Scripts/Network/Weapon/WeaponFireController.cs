using System;
using UnityEngine;

/// <summary>
/// 무기 발사 컨트롤러_클라이언트
///     탄창/예비탄 관리
///     발사 레이트, 재장전 시간 제어
///         재장전 중에는 발사X
///     발사 시 FIRE 명령 전송(서버 히트 판정은 그대로)
///     로컬 이펙트(머즐, 반동, 셰이크) 트리거
/// </summary>
public class WeaponFireController : MonoBehaviour
{
    [Header("Ammo")]
    public int MagSize = 30;                 // 탄창 용량
    public int MagAmmo = 30;                 // 현재 탄창 탄 수
    public int ReserveAmmo = 90;             // 예비 탄 수(소지 탄)
    public bool InfiniteReserve = false;     // 예비 탄 무한 여부(테스트용)

    [Header("Firing")]
    public float Rpm = 600.0f;               // 발사 레이트(Rounds Per Minute)
    public bool SemiAuto = false;            // 반자동 여부(true면 클릭당 1발)
    public float LastFireTime = -999.0f;     // 마지막 발사 시각(초)
    public float FireInterval = 0.1f;        // 한 발 사이 간격(초) = 60 / rpm (Start에서 계산)

    [Header("Reload")]
    public float ReloadSeconds = 1.8f;       // 재장전 소요 시간(초)
    public bool IsReloading = false;         // 재장전 중 여부
    private float ReloadEndTime = 0.0f;      // 재장전 완료 예정 시각

    [Header("Local FX")]
    public MuzzleFlash MuzzleFlash;          // 머즐 플래시
    public CameraRecoil CameraRecoil;        // 카메라 반동
    public ScreenShake ScreenShake;          // 셰이크
    public float RecoilPitch = 2.0f;         // 반동 상하 강도
    public float RecoilYaw = 0.6f;           // 반동 좌우 강도
    public float ShakeStrength = 0.08f;      // 셰이크 세기
    public float ShakeDuration = 0.10f;      // 셰이크 지속

    [Header("UI Event")]
    public Action<int, int> OnAmmoChanged;   // (magAmmo, reserveAmmo) UI 갱신 이벤트

    public ClientHitFeedback ClientHitFeedback;

    private bool _triggerHeld;                // 연사 입력 유지 상태(좌클릭 유지)
    private bool _fireThisFrame;              // 이번 프레임 발사 트리거(반자동용)

    private void Start()
    {
        // 발사 간격 계산: 초 단위
        if (Rpm <= 0.0f)
        {
            Rpm = 600.0f;
        }
        FireInterval = 60.0f / Rpm;

        // 시작 시 UI 동기
        if (OnAmmoChanged != null)
        {
            OnAmmoChanged.Invoke(MagAmmo, ReserveAmmo);
        }
    }

    private void Update()
    {
        // 1) 재장전 진행 중
        if (IsReloading == true)
        {
            //시간이 되면 완료 체크
            if (Time.time >= ReloadEndTime)
            {
                FinishReload();
            }
            // 재장전 중에는 발사 입력을 무시
            HandleReloadInputOnly();
            return;
        }

        // 2) 입력 읽기
        bool pressed = Input.GetMouseButtonDown(0);   // 좌클릭 1회 트리거
        bool holding = Input.GetMouseButton(0);       // 좌클릭 유지
        bool reloadKey = Input.GetKeyDown(KeyCode.R); // R 키 재장전

        _triggerHeld = holding;
        _fireThisFrame = pressed;

        // 3) 재장전 입력 우선
        if (reloadKey)
        {
            TryStartReload();
            return;
        }

        // 4) 발사 조건 체크
        if (SemiAuto)
        {
            // 반자동: 클릭 순간만 발사
            if (_fireThisFrame == true)
            {
                TryFireOnce();
            }
        }
        else
        {
            // 연사: 유지 중 프레임마다 시도
            if (_triggerHeld)
            {
                TryFireOnce();
            }
        }
    }

    /// <summary>
    /// 탄 발사 함수
    /// </summary>
    private void TryFireOnce()
    {
        // 탄이 없으면 재장전 유도
        if (MagAmmo <= 0)
        {
            TryStartReload();
            return;
        }

        // 레이트 체크
        float now = Time.time;            // 현재 시각(초)
        float nextTime = LastFireTime + FireInterval; // 다음 발사 가능 시각
        if (now < nextTime)
        {
            return;
        }

        // 발사 처리
        LastFireTime = now;
        --MagAmmo;

        // 로컬 이펙트
        if (MuzzleFlash != null)
        {
            MuzzleFlash.PlayOnce();
        }
        if (CameraRecoil != null)
        {
            CameraRecoil.Kick(RecoilPitch, RecoilYaw);
        }
        if (ScreenShake != null)
        {
            ScreenShake.ShakeOnce(ShakeStrength, ShakeDuration);
        }

        // UI 알림
        if (OnAmmoChanged != null)
        {
            OnAmmoChanged.Invoke(MagAmmo, ReserveAmmo);
        }

        // 서버에 발사 명령
        if (NetworkRunner.instance != null)
        {
            bool isClient = NetworkRunner.instance.IsClientConnected();
            bool isServer = NetworkRunner.instance.IsServerRunning();

            //서버에 FIRE 요청
            if (isClient)
            {
                NetworkRunner.instance.ClientSendLine("FIRE|");
            }
            else if (isServer)
            {
                NetworkRunner.instance.ServerInjectCommand(0, "FIRE", "");
            }
        }

        if (ClientHitFeedback != null)
        {
            ClientHitFeedback.TryLocalRayFeedback();
        }
    }

    /// <summary>
    /// 재장전 시도 함수
    /// </summary>
    private void TryStartReload()
    {
        // 이미 재장전 중이면 무시
        if (IsReloading)
        {
            return;
        }

        // 탄창이 이미 가득이면 무시
        if (MagAmmo >= MagSize)
        {
            return;
        }

        // 예비탄이 없고 무한 옵션도 아니면 불가
        if (ReserveAmmo <= 0 && !InfiniteReserve)
        {
            return;
        }

        // 재장전 시작
        IsReloading = true;
        ReloadEndTime = Time.time + ReloadSeconds;
    }

    /// <summary>
    /// 재장전 처리
    /// </summary>
    private void FinishReload()
    {
        IsReloading = false;

        // 옮길 탄 수 계산
        int need = MagSize - MagAmmo;         // 필요한 탄 수
        int moved = need;                     // 옮길 탄 수(기본: need)

        if (!InfiniteReserve)
        {
            if (ReserveAmmo < moved)
            {
                moved = ReserveAmmo;          // 예비가 부족하면 가능한 만큼만
            }
            ReserveAmmo -= moved;
        }

        MagAmmo += moved;

        // UI 갱신
        if (OnAmmoChanged != null)
        {
            OnAmmoChanged.Invoke(MagAmmo, ReserveAmmo);
        }
    }

    private void HandleReloadInputOnly()
    {
        // 재장전 중 입력을 막거나 애니메이션 동기 등의 훅을 둘 수 있는 자리
        // 지금은 로직 없음
    }
}
