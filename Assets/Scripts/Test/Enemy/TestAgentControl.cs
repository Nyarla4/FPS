using UnityEngine;
using UnityEngine.AI;

public class TestAgentControl : MonoBehaviour
{
    [SerializeField] private Transform[] _patrolPoints;
    [SerializeField] private NavMeshAgent _agent;
    private int _index;
    private float _dist = 0.5f;

    void Start()
    {
        _index = 0;
    }

    void Update()
    {
        var curDest = _patrolPoints[_index].position;
        if(Vector3.Distance(curDest, transform.position) <= _dist)
        {
            var nextIdx = _index + 1;
            if (nextIdx >= _patrolPoints.Length)
            {
                nextIdx = 0;
            }
            _index = nextIdx;
            var nextDir = _patrolPoints[_index];
            _agent.SetDestination(nextDir.position);
        }
        else
        {
            _agent.SetDestination(curDest);
        }
    }
}
