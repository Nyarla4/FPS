using UnityEngine;

/// <summary>
/// ������������ HP ����
///     ����/��� �̺�Ʈ�� ����
///     ���������� TakeDamage ȣ��
/// </summary>
public class Damageable : MonoBehaviour
{
    [Header("Stats")]
    public int MaxHp = 100;//�ִ� ü��
    public int CurHp = 100;//���� ü��

    public int Id = -1;
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

        Debug.Log($"{(Id==0?"Host":"Client")} damaged {amount}, {CurHp}/{MaxHp}");

        if (CurHp == 0)
        {
            OnDeath();
        }
    }

    public void OnDeath()
    {
        //������: �������� ó��
        //����/����Ʈ: Ŭ���̾�Ʈ���� STATE ���� ���� ���� ó��
    }
}
