using System;
using UnityEditor.Rendering;
using UnityEngine;

/// <summary>
/// 적 시야 센서
/// 거리: ViewDistance 만큼
/// 시야각: ViewAngle/2 각도로 계산
/// 라인: Eye 위치에서 타겟 방향으로 레이캐스트 → 중간에 장애물이 없는지 확인
/// </summary>
public class EnemySenses : MonoBehaviour
{
    [Header("Vision")]
    public Transform Eye; //적 기준점(머리 등 위치)
    public float ViewDistance = 18.0f; //적 거리(m)
    public float ViewAngle = 110.0f; //적 시야각. 각도 = ViewAngle * 0.5f
    public LayerMask VisionMask; //시야 레이캐스트 충돌 마스크(벽, 지형, 플레이어 등)
    public Transform Target; //추적 대상(플레이어 등의 Transform)

    [Header("Debug")]
    public bool DrawDebug = false; //디버그 라인 출력 여부

    void Update()
    {

    }

    /// <summary>
    /// 타겟이 적에게 보이는지 판정
    /// 보이면 true 반환 및 lastKnownPos에 타겟 위치 반환
    /// </summary>
    public bool CanSeeTarget(out Vector3 lastKnownPos)
    {
        //판정값 초기화
        lastKnownPos = Vector3.zero;

        //필수 참조 확인
        if (Eye == null || Target == null)
        {
            return false;
        }

        //거리 계산
        Vector3 toTarget = Target.position - Eye.position; //타겟까지 방향(정규화 전)
        float dist = toTarget.magnitude; //거리(m)
        if (dist > ViewDistance)
        { //시야 거리 초과 시
            return false;
        }

        //시야각 계산(내적 기반 = cos(θ))
        Vector3 forward = Eye.forward; //적 정면 방향
        Vector3 dir = toTarget.normalized; //타겟까지 정규화된 방향
        float dot = Vector3.Dot(forward, dir); //cos(theta): 두 방향 벡터의 내적
        float halfRad = (ViewAngle * 0.5f) * Mathf.Deg2Rad; //절반 각도 라디안
        float cosHalf = Mathf.Cos(halfRad); //비교 기준: cos(절반각)

        //dot < cos(절반각)이면 시야 밖
        if (dot < cosHalf)
        {
            return false;
        }

        //라인캐스트로 가림 여부 확인(시야 방해물 충돌 체크)
        Ray ray = new Ray(Eye.position, dir);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, ViewDistance, VisionMask, QueryTriggerInteraction.Ignore))
        {
            //충돌한 오브젝트의 루트가 타겟 루트와 같다면 시야 내 존재
            Transform h = hit.collider.transform;
            if (IsSameRoot(h, Target))
            {
                lastKnownPos = Target.position;

                if (DrawDebug)
                {
                    Debug.DrawLine(Eye.position, hit.point, new Color(0.5f, 1.0f, 0.5f), 0.1f);
                }

                return true;
            }
            else
            {
                if (DrawDebug)
                {
                    Debug.DrawLine(Eye.position, hit.point, new Color(1.0f, 0.5f, 0.5f), 0.1f);
                }

                return false;
            }
        }

        //시야 내에 타겟이 없을 경우
        return false;
    }

    /// <summary>
    /// 두 Transform의 루트가 같은지 확인
    /// *레이캐스트 히트된 자식 오브젝트가 타겟의 일부인지 판별
    /// </summary>
    private bool IsSameRoot(Transform a, Transform b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        return a.root == b.root;
    }
}
