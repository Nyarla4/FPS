using UnityEditor.Connect;
using UnityEngine;

/// <summary>
/// 경사면, 계단, 지면 보정 관련 안정화 처리
/// CharacterController 이동중 Slope/계단/스텝 보정 등의 기능을 제공하는 유틸리티 컴포넌트
/// 작동 방식 설명:
/// 1. 공중(비Ground)일 때는 표면 검사를 하지 않음(지면에 닿은 상태에서만 처리)
/// 2. 평면 이동 중 경사면 위를 걸을 때(또는 오르내릴 때)
/// 3.
/// 4. 계단 처리 시 상단표면+하단표면을 샘플링하여 부드럽게 이동
/// 5. 지면 보정은 착지시 자동으로 하강 처리(스냅 다운)
/// </summary>
public class GroundingStablillizer : MonoBehaviour
{
    [Header("Gound Probe")]
    public float ProbeDistance = 0.6f; //표면 감지를 위한 레이 길이(기본적으로 발밑 충돌 확인 용도)
    public LayerMask GroundMask;

    [Header("Slope Projection")]
    public float SlopeStickAngleBias = 2.0f;//cc.slopeLimit보다 약간 작은 각도(밀착 유지용)
    public float MinMoveSpeedForProjection = 0.4f;//이동속도가 이 값 이상일 때만 경사면 보정(너무 느릴 때 생략)
    public float MaxProjectionLossRatio = 0.5f;//투영 손실률이 50% 이상일 경우 보정 생략(비정상 각도 방지)

    [Header("Step Smoothing")]
    public float StepUpSmoothTime = 0.06f;//스텝 오를 때 부드럽게 전환 (SmoothDamp 시간)
    public float MaxStepHeight = 0.4f;//계단의 최대 높이 (이 이상은 오르지 않음)
    public float StepProbeForward = 0.25f;//앞쪽 감지 거리(플레이어 전방으로 짧게 캐스팅)

    [Header("Snap Down")]
    public float SnapDownDistance = 0.35f;//지면 스냅용 최대 거리
    public float SnapDownSpeed = 20.0f;//계단 하강시 1초당 이동 속도 (부드럽게 붙음)

    //보정 변수(계단 이동 관련)
    private float _stepUpVelocity;//SmoothDamp용 보정 속도 저장
    private float _currentStepOffset;//현재 계단 높이 보정량(y 방향)

    /// <summary>
    /// 이동벡터를 현재 지면의 법선에 ‘투영’ => 경사면에 붙도록 변환
    /// </summary>
    public Vector3 ProjectOnGround(Vector3 move, CharacterController cc, PlayerController pc)
    {
        if (cc == null)
        {
            return move;//CC가 없으면 이동 벡터를 그대로 반환
        }

        //현재 프레임 이동 속도(지면에 있을 때만)
        float dt = Time.deltaTime;
        float speedPlanar = 0f;//현재 이동 속도(m/s)
        if (dt > 0)
        {
            //XZ 평면 이동 거리 계산
            speedPlanar = new Vector2(move.x, move.z).magnitude;//속도 크기 계산
        }

        //지면 체크(지면에 있을 때만 경사면 보정)
        bool grounded = false;
        if (pc != null)
        {
            //pc 쪽에서 별도 Ground 체크를 제공하면 사용
            grounded = pc.IsGround;
        }
        else
        {
            grounded = cc.isGrounded;
        }

        if (!grounded)
        {//공중일 땐 그대로
            return move;
        }

        //너무 느릴 때는 경사면 보정 생략
        if (speedPlanar < MinMoveSpeedForProjection)
        {
            return move;
        }

        //지면 법선 검사
        Vector3 normal;
        bool got = TryGetGroundNormal(cc, out normal);
        if (!got)//표면 감지 실패 시 그대로
        {
            return move;
        }

        //경사 각도 계산: CC가 허용하는 경사보다 크면 보정 생략
        float slope = Vector3.Angle(normal, Vector3.up);//법선과 Up의 각도(0=수평, 90=수직)
        float limit = cc.slopeLimit - SlopeStickAngleBias;
        if (slope > limit)
        {//허용각 초과
            return move;
        }

        //수평면 투영: move를 지면 법선에 투영
        Vector3 onPlane = Vector3.ProjectOnPlane(move, normal);

        //보정 후 속도 손실이 너무 크면 무시
        float originalLen = new Vector2(move.x, move.z).magnitude;
        float projectedLen = new Vector2(onPlane.x, onPlane.z).magnitude;

        if (originalLen > 0.0001f)
        {
            float ratio = projectedLen / originalLen;//투영 후 길이 비율
            if (ratio < MaxProjectionLossRatio)
            {//손실이 크면 그대로 반환(즉, ‘벽’에 부딪힌 경우)
                return move;
            }
        }

        //수직 속도(y)는 그대로 유지
        onPlane.y = move.y;
        return onPlane;
    }

    /// <summary>
    /// 계단 이동 시 SmoothDamp를 사용해 부드럽게 이동
    /// ‘상단 표면 + 하단 표면’을 샘플링해서 높이 결정
    /// </summary>
    public Vector3 ApplyStepSmoothing(Vector3 move, CharacterController cc, PlayerController pc)
    {
        if (cc == null)
        {//cc가 없으면 그냥 반환
            return move;
        }

        //이동 벡터가 거의 0이면 보정 해제
        Vector2 planner = new Vector2(move.x, move.z);
        if (planner.sqrMagnitude <= 0.000001f)
        {
            return ReleaseStepGradually(move);
        }

        //앞 방향 벡터 계산(수평 기준)
        Vector3 dir = GetHorizontalDir(move);//이동 방향 계산

        //표면 감지용 Ray 설정 (상단/하단)
        Vector3 originLow = cc.transform.position + Vector3.up * (cc.stepOffset * 0.5f);//하단 시작점
        Vector3 originHigh = cc.transform.position + Vector3.up * (cc.stepOffset * 0.08f);//상단 시작점(바닥 근처)

        //앞쪽 감지 거리(플레이어 반지름 + 여유값)
        float castDist = Mathf.Max(cc.radius + StepProbeForward, 0.15f);

        //표면 탐지 (충돌 체크)
        RaycastHit hitLow;
        bool lowHit = Physics.Raycast(originLow, dir, out hitLow, castDist, GroundMask, QueryTriggerInteraction.Ignore);
        RaycastHit hitHigh;
        bool highHit = Physics.Raycast(originHigh, dir, out hitHigh, castDist, GroundMask, QueryTriggerInteraction.Ignore);

        //하단은 맞고 상단은 비었을 경우 계단으로 판단
        if (lowHit && !highHit)
        {
            //올라갈 높이 계산
            float desireUp = hitLow.point.y - cc.transform.position.y;
            if (desireUp < 0)
            {//내리막일 때는 계단 보정 무시
                desireUp = 0f;
            }
            if (desireUp > MaxStepHeight)
            {//최대 계단 높이 제한
                desireUp = MaxStepHeight;
            }

            //SmoothDamp로 현재 보정값을 목표값으로 점진 조정
            float dt = Time.deltaTime;
            float smoothed = Mathf.SmoothDamp(
                _currentStepOffset, //현재값
                desireUp,           //목표값
                ref _stepUpVelocity,//보정 속도
                StepUpSmoothTime,   //시간
                Mathf.Infinity,     //최대속도 제한 없음
                dt
                );

            //계단 오를 때 추가 y 이동량 계산
            float delta = smoothed - _currentStepOffset;
            _currentStepOffset = smoothed;

            //y 값 보정 적용
            float newY = move.y + delta;
            if (newY > MaxStepHeight)
            {
                newY = MaxStepHeight;
            }
            move.y = newY;
        }
        else
        {//계단이 아닐 경우
            //보정값을 서서히 0으로 복귀
            move = ReleaseStepGradually(move);
        }

        return move;
    }

    /// <summary>
    /// 지면 스냅다운: ‘Smooth한 하강’을 위해 일정 거리 아래의 지면으로 자동 붙음
    /// 착지 시 사용 (계단/지면 경계)
    /// </summary>
    public void TrySnapDown(CharacterController cc, PlayerController pc)
    {
        if (cc == null || cc.velocity.y > 0f)
        {//cc가 없거나 위로 이동 중이면 생략(점프/상승 시)
            return;
        }

        //표면 탐지 시작 위치: 약간 위에서 아래로 레이 쏨
        Vector3 start = cc.transform.position + Vector3.up * 0.1f;

        //아래로 레이캐스트 (지면 찾기)
        RaycastHit hit;
        bool got = Physics.Raycast(
            start,
            Vector3.down,
            out hit,
            SnapDownDistance + 0.1f,
            GroundMask,
            QueryTriggerInteraction.Ignore
        );

        if (got)
        {
            //현재 위치와 지면 사이의 거리 계산
            float gap = (start.y - hit.point.y) - 0.1f;

            //너무 가까우면 생략
            if (gap < 0.015f)
            {
                return;
            }

            //Time.deltaTime 기준 하강 거리 계산
            float step = Mathf.Min(gap, SnapDownSpeed * Time.deltaTime);

            //CharacterController.Move()를 사용해 실제 이동
            Vector3 down = Vector3.down * step;
            cc.Move(down);
        }
    }

    #region 내부 유틸 함수

    /// <summary>
    /// 지면 법선 계산
    /// 캐릭터 아래로 레이캐스트하여 표면 법선 반환
    /// </summary>
    private bool TryGetGroundNormal(CharacterController cc, out Vector3 normal)
    {
        normal = Vector3.up;//기본값(수평 방향)

        //캐릭터 중심에서 약간 위에서 레이캐스트
        Vector3 origin = cc.transform.position + Vector3.up * 0.2f;
        RaycastHit hit;
        bool got = Physics.Raycast(
            origin,
            Vector3.down,
            out hit,
            ProbeDistance + 0.25f,
            GroundMask,
            QueryTriggerInteraction.Ignore
        );

        if (got)
        {
            //충돌 표면의 법선 반환
            normal = hit.normal;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 이동 벡터의 수평 방향만 추출(y=0)
    /// 수평 방향 반환
    /// </summary>
    private Vector3 GetHorizontalDir(Vector3 move)
    {
        Vector3 d = move;//입력 복사
        d.y = 0f;//수직 제거

        if (d.sqrMagnitude > 0f)
        {
            d.Normalize();//0이 아닐 경우 정규화
        }

        return d;
    }

    /// <summary>
    /// 계단 보정값을 서서히 0으로 되돌림 (천천히 복귀)
    /// </summary>
    private Vector3 ReleaseStepGradually(Vector3 move)
    {
        float dt = Time.deltaTime;

        //_currentStepOffset을 0으로 부드럽게 감소
        float smoothed = Mathf.SmoothDamp(
            _currentStepOffset,
            0f,
            ref _stepUpVelocity,
            StepUpSmoothTime,
            Mathf.Infinity,
            dt
        );

        //감소분만큼 y 보정
        float delta = smoothed - _currentStepOffset;
        _currentStepOffset = smoothed;
        move.y += delta;
        return move;
    }

    #endregion
}
