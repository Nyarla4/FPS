using System.Collections.Generic;
using UnityEngine;

public class MagazineSpawner : MonoBehaviour
{
    [SerializeField] private Magazine _magazinePrefab;
    public Transform[] SpawnPoints;
    private float _spawnTimer;
    [SerializeField] private float _spawnTime;

    private void Awake()
    {
        _spawnTimer = 0;
    }

    void Update()
    {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer > _spawnTime)
        {
            SpawnMagazine();
        }
    }

    private void SpawnMagazine()
    {
        if(_magazinePrefab == null || SpawnPoints.Length == 0)
        {
            return;
        }

        int idx = Random.Range(0, SpawnPoints.Length);
        if (SpawnPoints[idx].childCount > 0)
        {
            return;
        }

        var magazine = Instantiate(_magazinePrefab, SpawnPoints[idx].transform);
        _spawnTimer = 0f;
    }
}
