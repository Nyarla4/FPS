using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 탄약 UI 표시
///     MagAmmo / ReserveAmmo 형태
/// </summary>
public class AmmoCounterUI : MonoBehaviour
{
    public WeaponFireController Weapon;//참조 대상의 무기 컨트롤러
    public TMP_Text Text;//출력 UI

    private void OnEnable()
    {
        if(Weapon != null)
        {
            Weapon.OnAmmoChanged += OnAmmoChanged;
        }
    }

    private void OnDisable()
    {
        if (Weapon != null)
        {
            Weapon.OnAmmoChanged -= OnAmmoChanged;
        }
    }

    void Start()
    {
        if (Weapon != null)
        {
            OnAmmoChanged(Weapon.MagAmmo, Weapon.ReserveAmmo);
        }
    }

    private void OnAmmoChanged(int mag, int reserve)
    {
        if(Text == null)
        {
            return;
        }
        Text.text = $"{mag} / {reserve}";
    }
}
