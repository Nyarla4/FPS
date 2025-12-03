using UnityEngine;
using UnityEngine.AI;

public class TestAgentControl : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;
    
    public void SetDestination(Vector3 goal)
    {
        _agent.SetDestination(goal);
    }

    public void Stop()
    {
        _agent.isStopped = true;
        _agent.ResetPath();
    }
}
