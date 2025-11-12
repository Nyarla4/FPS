using UnityEngine;

/// <summary>
/// 
/// </summary>
public class EnemyLifetimeReporter : MonoBehaviour
{
    private EnemyEncounterZone _ownerZone;
    private Health _health;
    private bool _reported;//보고되었는지 여부(중복방지용)
    
    public void Initialize(EnemyEncounterZone zone)
    {
        _ownerZone = zone;
    }

    private void Awake()
    {
        _health = GetComponent<Health>();
        if (_health != null)
        {
            _health.OnDeath.AddListener(OnDeath);
        }
    }

    void Update()
    {
        
    }

    private void OnDeath()
    {
        ReportIfNeeded();
    }

    /// <summary>
    /// 사망사실 전달용
    /// </summary>
    private void ReportIfNeeded()
    {
        if (_reported)
        {//이미 전달한 경우 return
            return;
        }

        _reported = true;
        if (_ownerZone != null)
        {
            _ownerZone.OnEnemyDead(this);
        }
    }
}
