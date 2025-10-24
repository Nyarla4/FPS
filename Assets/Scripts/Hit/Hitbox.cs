using UnityEngine;

/// <summary>
/// 부위별 대미지 배수 제공
/// </summary>
public class Hitbox : MonoBehaviour
{
    public float DamageMultiplier = 1.0f;//배수
    public Health Owner;//히트박스가 속한 캐릭터의 Health
    
    private void Reset()
    {
        Health h = GetComponentInParent<Health>();
        if (h != null)
        {
            Owner = h;
        }
    }
}
