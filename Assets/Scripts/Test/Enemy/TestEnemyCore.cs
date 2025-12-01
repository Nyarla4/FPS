using UnityEngine;

public class TestEnemyCore : MonoBehaviour
{
    public enum FSM
    {
        patrol,
        chase,
        attack,
    }

    [SerializeField] private ClearLogics _clear;

    public Transform Target;
    private TestAgentControl _agent;
    [SerializeField] private Transform[] _patrolPoints;
    private int _patrolIndex = 0;
    private float _patrolDist = 0.5f;
    private TestEnemyHealth _health;
    public bool IsAlive => _health.Health > 0f;
    [SerializeField] private float _testFloat;

    private FSM _curState;

    private void Awake()
    {
        _agent = GetComponent<TestAgentControl>();
    }

    private void Start()
    {
        _patrolIndex = 0;
        _clear.AddEnemy(this);
        _health.OnDeath += OnDeath;

        OnPatrolEnter();
    }

    void Update()
    {
        Patrol();
    }

    public void OnPatrolEnter()
    {
        int firstIndex = -1;
        float firstDist = float.PositiveInfinity;
        for (int i = 0; i < _patrolPoints.Length; i++)
        {
            var point = _patrolPoints[i];
            var dist = Vector3.Distance(point.position, transform.position);
            if (dist < firstDist)
            {
                firstIndex = i;
                firstDist = dist;
            }
        }

        _patrolIndex = firstIndex;
    }

    public void Patrol()
    {
        var curDest = _patrolPoints[_patrolIndex].position;
        if (Vector3.Distance(curDest, transform.position) <= _patrolDist)
        {
            var nextIdx = _patrolIndex + 1;
            if (nextIdx >= _patrolPoints.Length)
            {
                nextIdx = 0;
            }
            _patrolIndex = nextIdx;
            var nextDir = _patrolPoints[_patrolIndex];
            _agent.SetDestination(nextDir.position);
        }
        else
        {
            _agent.SetDestination(curDest);
        }
    }

    private void OnDeath()
    {
        _clear.Check();
    }
}
