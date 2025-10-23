using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 걷기 속도에 따라 발자국 이벤트를 발생시켜,
/// 왼발/오른발 이벤트를 분리 호출
/// PC + CC 환경 모두 지원
/// </summary>
[DisallowMultipleComponent]
public class StepEventSource : MonoBehaviour
{
    [Header("Refs")]
    public CharacterController Controller;//현재 속도 확인용
    public PlayerController Player;//걷기/달리기 상태 확인(캐릭터 속도와 연동)

    [Header("Step Timing")]
    public float WalkStepsPerSecond = 1.8f;//걷기 주파수(Hz)
    public float SprintStepsPerSecond = 2.6f;//달리기 주파수(Hz)
    public float MinSpeedForSteps = 1.0f;//이 속도 이상일 때만 발자국 이벤트 발생(멈춤 시 무효)
    public float GroundedGraceTime = 0.08f;//지면 접촉 후 짧은 시간 동안 발자국 허용(점프/착지 보정)

    [Header("Events")]
    public UnityEvent OnStepLeft;//왼발
    public UnityEvent OnStepRight;//오른발

    private bool _leftNext = true;//다음발: true-왼발, false-오른발
    private float _nextStepTime;//다음 발자국 이벤트 타이밍(예측시간)
    private float _lastGroundedTime;//최근 지면 접촉 시간(보정용 타이머)

    private void Awake()
    {
        if (Controller == null)
        {
            Controller = GetComponent<CharacterController>();
        }

        if (Player == null)
        {
            Player = GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        float now = Time.time;

        //지면 접촉여부를 확인하고 발자국 이벤트 관리
        bool grounded = false;
        if (Controller != null)
        {
            grounded = Controller.isGrounded;
        }

        if (grounded)
        {
            _lastGroundedTime = now;
        }

        //현재 이동 속도(발자국 주파수 조절용)
        float planarSpeed = 0.0f;
        if (Controller != null)
        {
            Vector3 v = Controller.velocity;
            v.y = 0.0f;
            planarSpeed = v.magnitude;
        }

        //최소 속도보다 느리면 발자국 이벤트 중단(발자국 간격 초기화)
        if (planarSpeed < MinSpeedForSteps)
        {
            if (now > _nextStepTime)
            {
                _nextStepTime = now;
            }
            return;
        }

        //걷기/달리기 주파수 설정
        float stepsHz = WalkStepsPerSecond;
        bool isSprint = planarSpeed > (MinSpeedForSteps + 2.0f);//기본 속도보다 빠름
        if (Player != null)
        {
            //입력에 따라 실제 달리기 여부를 판단
            isSprint = Player.SprintHeld;
        }

        if (isSprint)
        {
            stepsHz = SprintStepsPerSecond;
        }

        //다음 발자국 타이밍 초기화(최초 1회 설정)
        if (_nextStepTime <= 0.0f)
        {
            _nextStepTime = now + (1.0f / stepsHz);
        }

        //지면 또는 짧은 접지시간 내에서는 발자국 발생
        bool canStep = grounded || ((now - _lastGroundedTime) <= GroundedGraceTime);

        if (canStep)
        {
            if (now >= _nextStepTime)
            {
                if (_leftNext)
                {
                    OnStepLeft?.Invoke();
                    _leftNext = false;
                }
                else
                {
                    OnStepRight?.Invoke();
                    _leftNext = true;
                }

                _nextStepTime = now + (1.0f / stepsHz);
            }
        }
    }
}
