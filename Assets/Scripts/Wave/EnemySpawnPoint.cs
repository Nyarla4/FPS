using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public int MaxSpawnCount = 10;
    public float PositionJitterRadius = 0.5f;//하나의 스폰 포인트에서 여러마리 스폰할 경우 위치가 겹치지 않도록 반경 설정 및 해당 반경 내 랜덤 스폰 처리

    private int _spawnedCount;

    /// <summary> 추가 스폰 가능 여부 </summary>
    public bool CanSpawnMore => (MaxSpawnCount <= 0 ||  _spawnedCount < MaxSpawnCount);

    public GameObject SpawnOne(EnemyEncounterZone owner)
    {
        if(EnemyPrefab == null || (MaxSpawnCount > 0 && _spawnedCount >= MaxSpawnCount))
        {
            return null;
        }

        Vector3 basePos = transform.position;//기반 위치
        Vector2 rand = Random.insideUnitSphere * PositionJitterRadius;//반경 내 랜덤 위치 설정
        Vector3 spawnPoint = new Vector3(basePos.x + rand.x, basePos.y, basePos.z + rand.y);//결과적인 스폰 위치

        Quaternion rot = transform.rotation;

        //스폰
        GameObject obj = Instantiate(EnemyPrefab, spawnPoint, rot);
        ++_spawnedCount;

        //리포터 처리
        EnemyLifetimeReporter reporter = obj.GetComponent<EnemyLifetimeReporter>();
        if(reporter == null)
        {
            reporter = obj.AddComponent<EnemyLifetimeReporter>();
        }
        if (reporter != null)
        {
            reporter.Initialize(owner);
        }

        return obj;
    }
}
