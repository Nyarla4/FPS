using UnityEngine;

[System.Serializable]
public class WaveEnemyEntry
{
    public GameObject EnemyPrefab;
    public int Count = 3;
    public float SpawnInterval = 0.5f;
    public EnemySpawnPoint SpawnPoint;
}

/// <summary>
/// 각 웨이브에 대한 데이터 셋팅
/// </summary>
[System.Serializable]
public class WaveDefinition
{
    public string WaveName = "Wave 1";//웨이브 명
    public WaveEnemyEntry[] Enemies;//해당 웨이브에 스폰시킬 적 종류
    public bool WaitUntilAllDead;//다음 웨이브로 넘어가려면 모든적을 죽여야만 하는지 여부
    public float DelayAfterWave = 1.0f;//어 뭐더라
}
