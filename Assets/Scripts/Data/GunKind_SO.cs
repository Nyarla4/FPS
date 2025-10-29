using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon/Gun Kind", fileName = "GunKind")]
public class GunKind_SO : ScriptableObject
{
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
}
