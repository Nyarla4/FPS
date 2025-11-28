using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 기존 FSM에서 호출해서 사용
///     OnEnter, OnUpdate, OnExit에서 아래 메서드들 호출
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentOps : MonoBehaviour
{
    public Transform Player;//추적대상(플레이어). FSM에서 처리가능
    public float LoseSightTime = 2.0f;//시야 상실 유지 시간(s)
    public float WaypointReachDistance = 0.8f;//웨이포인트 도착 판정 거리(m), 도착 위치에서 허용 오차 범위

    [SerializeField] private NavMeshAgent _agent;//NavMeshAgent 캐시
    private PatrolRoute _patrol;//순찰 경로 참조용
    private float _loseTimer;//시야 상실 누적 시간
    private Vector3 _lastSeenPos;//최종 확인 위치

    private void Awake()
    {
        _loseTimer = 0.0f;//타이머 초기화
        _lastSeenPos = Vector3.zero;//마지막 시야 위치 초기화
        if (_agent == null)
        {
            Debug.LogError("[NavAgentOps] agent 누락");
        }
    }

    /// <summary>
    /// Search OnEnter
    /// </summary>
    public void BeginPatrol(PatrolRoute route)
    {
        _patrol = route;
        if (_patrol == null)
        {
            return;
        }

        Debug.Log($"agent on navmesh {_agent.isOnNavMesh}");

        Transform p = _patrol.GetCurrent();//현재 웨이 포인트
        if (p != null)
        {
            _agent.isStopped = false;
            _agent.SetDestination(p.position);
        }
    }

    /// <summary>
    /// Search OnUpdate
    /// </summary>
    public void PatrolUpdate()
    {
        if (_patrol == null)
        {
            return;
        }

        Transform t = _patrol.GetCurrent();//현재 목적지
        if (t == null)
        {
            return;
        }

        float dist = Vector3.Distance(transform.position, t.position);//남은 거리
        if (dist <= WaypointReachDistance)
        {
            _patrol.MoveNext();
            Transform next = _patrol.GetCurrent();//다음 목적지
            if (next != null)
            {
                _agent.SetDestination(next.position);
            }
        }
    }

    /// <summary>
    /// Chase OnEnter
    /// </summary>
    public void BeginChase(Transform target)
    {
        if (target != null)
        {
            Player = target;//추격 대상 설정
        }

        NavMeshHit hit;
        if (!_agent.isOnNavMesh)
        {
            Debug.Log("!onNavMesh");
            if(NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                Debug.Log($"warp to {hit.position}");
                _agent.Warp(hit.position);
            }
        }

        _loseTimer = 0.0f;
        if (Player != null)
        {
            _agent.isStopped = false;
            _agent.SetDestination(Player.position);
            _lastSeenPos = Player.position;
        }
    }

    /// <summary>
    /// 추적 진행
    /// </summary>
    /// <param name="canSee">현재 보이는지 여부</param>
    /// <param name="observedPosition">감지된 플레이어 위치</param>
    /// <returns>true: 추적 진행, false: 추적 종료</returns>
    public bool ChaseUpdate(bool canSee, Vector3 observedPosition)
    {
        if (canSee)
        {
            _lastSeenPos = observedPosition;//마지막 본 위치 갱신
            _agent.SetDestination(_lastSeenPos);
            _loseTimer = 0.0f;
            return true;
        }

        _loseTimer += Time.deltaTime;//시간 경과

        if (_loseTimer < LoseSightTime)
        {
            _agent.SetDestination(_lastSeenPos);//일단 최종 확인된 위치로 이동
            return true;
        }

        return false;//다른 상태로 전환
    }

    /// <summary>
    /// 상태 전환시 정지
    /// </summary>
    public void StopImmediate()
    {
        _agent.isStopped = true;//정지
        _agent.ResetPath();//경로 초기화
    }

    /// <summary>
    /// 정지 거리 조정
    /// </summary>
    /// <param name="value">정지할 거리</param>
    public void SetStoppingDistance(float value)
    {
        _agent.stoppingDistance = value;//무기나 패턴에 따라 거리 조정 처리
    }
}
