using UnityEngine;

/// <summary>
/// 피격 가능 개체 공용 인터페이스
/// 무기에서는 당 인터페이스 인식
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 대미지 적용 함수
    /// </summary>
    /// <param name="amount">적용할 피해량</param>
    /// <param name="hitPoint">맞은 월드 좌표</param>
    /// <param name="hitNormal">맞은 표면 법선 *데칼용</param>
    /// <param name="source">공격자 트랜스폼</param>
    void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, Transform source);
}
