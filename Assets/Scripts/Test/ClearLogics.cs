using System.Collections.Generic;
using UnityEngine;

public class ClearLogics : MonoBehaviour
{
    [SerializeField] private GameObject _clearTrigger;

    private List<TestEnemyCore> _enemies = new();
    public int AliveEnemies => _enemies.FindAll(f=>f.IsAlive).Count;

    private void Awake()
    {
        if (_clearTrigger != null)
        {
            _clearTrigger.SetActive(false);
        }
    }

    public void AddEnemy(TestEnemyCore enemy)
    {
        _enemies.Add(enemy);
    }

    public void Check()
    {
        if (AliveEnemies <= 0)
        {
            _clearTrigger.SetActive(true);
        }
    }
}
