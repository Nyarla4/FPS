using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float _walkSpeed = 4.5f;//도보 속도
    [SerializeField] private float _sprintSpeed = 7.5f;//질주 속도
    [SerializeField] private float _acceleration = 15.0f;//가속도
    [SerializeField] private float _deceleration = 20.0f;//제동속도

    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 1.2f;//최고 높이
    [SerializeField] private float _gravity = -9.81f * 2.0f;//중력
    [SerializeField] private float _coyoteTime = 0.12f;//체공 시간
    [SerializeField] private float _jumpBufferTime = 0.12f;//점프키 선입력 허용시간
    [SerializeField] private LayerMask _groundMask;//지면 레이어
    //[SerializeField] private float _groundCheckRadius = 0.3f;//지면 감지 범위
    //[SerializeField] private Transform _groundCheck;

    [Header("ref")]//참조
    [SerializeField] private Transform _cameraPivot;
    private CharacterController _controller;

    public GroundingStablillizer Stablillizer;

    private Vector2 _moveInput;
    private bool _sprintHeld;//질주 버튼 눌렸는지 여부
    public bool SprintHeld => _sprintHeld;
    private float _currentSpeed;
    private Vector3 _velocity;

    public bool IsGround => _feed.IsGrounded;
    private float _lastGroundedTime;//마지막 착지시각
    private float _lastJumpPressTime;//마지막 점프입력시각

    private LocomotionFeed _feed;

    [SerializeField] private Animator _animator;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null)
        {
            Debug.LogError("[PlayerController] 캐릭터 컨트롤러 누락");
        }

        if (_feed == null)
        {
            _feed = GetComponent<LocomotionFeed>();
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        UpdateGround();
        UpdateHorizontalSpeed(dt);
        ApplyGravityAndJump(dt);

        Vector3 move = CalculateWorldMove();

        //경사나 계단에서 보정 처리
        if (Stablillizer != null)
        {
            move = Stablillizer.ProjectOnGround(move, _controller, this);
            move = Stablillizer.ApplyStepSmoothing(move, _controller, this);
        }

        if (_animator != null)
        {
            _animator.SetFloat("Speed", _currentSpeed);
        }

        _controller.Move(move * dt);

        if (Stablillizer != null)
        {
            if (_controller.velocity.y <= 0.0f)
            {//떨어지고 있는 상황인 경우
                Stablillizer.TrySnapDown(_controller, this);
            }
        }
    }

    #region InputAction
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed || context.canceled)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _sprintHeld = true;
        }

        if (context.canceled)
        {
            _sprintHeld = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _lastJumpPressTime = Time.time;
        }
    }
    #endregion

    public void UpdateGround()
    {
        //bool hit = false;

        //if (_groundCheck != null)
        //{
        //    Collider[] hits = Physics.OverlapSphere(_groundCheck.position, _groundCheckRadius, _groundMask, QueryTriggerInteraction.Ignore);
        //    if (hits != null && hits.Length > 0)
        //    {
        //        hit = true;
        //    }
        //}

        //if (hit)
        //{
        //    if (!IsGround)
        //    {
        //        if (_velocity.y < 0.0f)
        //        {//떨어지는 중
        //            _velocity.y = -2.0f;
        //        }
        //    }
        //
        //    IsGround = true;
        //    _lastGroundedTime = Time.time;
        //}
        //else
        //{
        //    IsGround = false;
        //}

        if (!IsGround)
        {
            if (_velocity.y < 0.0f)
            {//떨어지는 중
                _velocity.y = -2.0f;
            }
        }

        _lastGroundedTime = Time.time;
    }

    private void UpdateHorizontalSpeed(float dt)
    {
        //변경점 1: 입력 크기에 따라 '목표 속도'를 0~최대까지 설정
        //  -아날로그 입력일 때 '이동이 너무 작다'는 체감 개선
        float inputMag = _moveInput.magnitude;//0~1

        float baseTarget = _walkSpeed;
        if (_sprintHeld)
        {
            baseTarget = _sprintSpeed;
        }

        float targetSpeed = baseTarget * inputMag;//입력 크기 반영

        if (_currentSpeed < targetSpeed)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _acceleration * dt);
        }
        else if (_currentSpeed > targetSpeed)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _deceleration * dt);
        }
    }

    private void ApplyGravityAndJump(float dt)
    {
        //점프 가능 여부
        bool coyote = (Time.time - _lastGroundedTime) <= _coyoteTime;

        //점프 키 눌림 여부
        bool buffered = (Time.time - _lastJumpPressTime) <= _jumpBufferTime;

        if (buffered && (IsGround || coyote))
        {
            _velocity.y = Mathf.Sqrt(Mathf.Abs(2.0f * _gravity * _jumpHeight));
            _lastJumpPressTime = -999.0f;
        }

        _velocity.y += _gravity * dt;
    }

    private Vector3 CalculateWorldMove()
    {
        //변경점 2: 카메라 평면 투영이 '너무 작으면' 플레이어 전방/우측으로 폴백
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (_cameraPivot != null)
        {
            Vector3 camForward = _cameraPivot.forward;
            Vector3 camRight = _cameraPivot.right;

            camForward.y = 0.0f;
            camRight.y = 0.0f;

            float lenF = camForward.magnitude;
            float lenR = camRight.magnitude;

            if (lenF > 0.0001f)
            {
                camForward /= lenF;
            }
            if (lenR > 0.0001f)
            {
                camRight /= lenR;
            }

            //임계치 이하(거의 수직 시야 등)면 안전 폴백
            if (camForward.sqrMagnitude < 0.0001f || camRight.sqrMagnitude < 0.0001f)
            {
                Vector3 bodyF = transform.forward;
                Vector3 bodyR = transform.right;

                bodyF.y = 0.0f;
                bodyR.y = 0.0f;

                if (bodyF.sqrMagnitude > 0.0f)
                {
                    bodyF.Normalize();
                }
                if (bodyR.sqrMagnitude > 0.0f)
                {
                    bodyR.Normalize();
                }

                forward = bodyF;
                right = bodyR;
            }
            else
            {
                forward = camForward;
                right = camRight;
            }
        }

        //희망 방향
        Vector3 wish = forward * _moveInput.y + right * _moveInput.x;

        //정규화
        if (wish.sqrMagnitude > 1.0f)
        {
            wish.Normalize();
        }

        //수평 속도 벡터 = 방향 × 현재 속도
        Vector3 horizontal = wish * _currentSpeed;

        //최종 이동 속도(m/s)
        Vector3 move = new Vector3(horizontal.x, _velocity.y, horizontal.z);
        return move;
    }
}
