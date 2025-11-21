using UnityEngine;

/// <summary>
/// 서버권한으로 HP 관리
///     감소/사망 이벤트만 제공
///     서버에서만 TakeDamage 호출
/// </summary>
public class Damageable : MonoBehaviour
{
    [Header("Stats")]
    public int MaxHp = 100;//최대 체력
    public int CurHp = 100;//현재 체력

    public void ResetHp()
    {
        CurHp = MaxHp;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurHp -= amount;
        if (CurHp < 0)
        {
            CurHp = 0;
        }

        if(CurHp == 0)
        {
            OnDeath();
        }
    }

    public void OnDeath()
    {
        //리스폰: 서버에서 처리
        //사운드/이펙트: 클라이언트에서 STATE 기준 별도 연출 처리
    }
}
