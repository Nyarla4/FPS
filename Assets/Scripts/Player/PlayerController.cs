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
    [SerializeField] private float _groundCheckRadius = 0.3f;//지면 감지 범위
    [SerializeField] private Transform _groundCheck;

    [Header("ref")]//참조
    [SerializeField] private Transform _cameraPivot;
    private CharacterController _controller;

    private Vector2 _moveInput;
    private bool _sprintHeld;//질주 버튼 눌렸는지 여부
    private float _currentSpeed;
    private Vector3 _velocity;

    public bool IsGround;
    private float _lastGroundedTime;//마지막 착지시각
    private float _lastJumpPressTime;//마지막 점프입력시각

    private void Awake()
    {
        if (_groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.up * 0.1f;
            _groundCheck = go.transform;
        }
        _controller = GetComponent<CharacterController>();
    }

    void Start()
    {

    }

    void Update()
    {
        float dt = Time.deltaTime;
        UpdateGround();
        UpdateHorizontalSpeed(dt);
        ApplyGravityAndJump(dt);

        Vector3 move = CalculateWorldMove();
        _controller.Move(move * dt);
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
        bool hit = false;

        if (_groundCheck != null)
        {
            Collider[] hits = Physics.OverlapSphere(_groundCheck.position, _groundCheckRadius, _groundMask, QueryTriggerInteraction.Ignore);
            if (hits != null && hits.Length > 0)
            {
                hit = true;
            }
        }

        if (hit)
        {
            if (!IsGround)
            {
                if (_velocity.y < 0.0f)
                {//떨어지는 중
                    _velocity.y = -2.0f;
                }
            }

            IsGround = true;
            _lastGroundedTime = Time.time;
        }
        else
        {
            IsGround = false;
        }
    }

    private void UpdateHorizontalSpeed(float dt)
    {
        float targetSpeed = _sprintHeld ? _sprintSpeed : _walkSpeed;

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

        if(buffered && (IsGround || coyote))
        {
            _velocity.y = Mathf.Sqrt(Mathf.Abs(2.0f * _gravity * _jumpHeight));
            _lastJumpPressTime = -999.0f;
        }

        _velocity.y += _gravity * dt;
    }

    private Vector3 CalculateWorldMove()
    {
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (_cameraPivot != null)
        {
            Vector3 camForward = _cameraPivot.forward;
            Vector3 camRight = _cameraPivot.right;

            camForward.y = 0.0f;
            camRight.y = 0.0f;

            if (camForward.sqrMagnitude > 0.0f)
            {
                camForward.Normalize();
            }

            if (camRight.sqrMagnitude > 0.0f)
            {
                camRight.Normalize();
            }

            forward = camForward;
            right = camRight;
        }

        Vector3 wish = forward * _moveInput.y + right * _moveInput.x;
        if (wish.sqrMagnitude > 1.0f)
        {
            wish.Normalize();
        }

        Vector3 horizontal = wish * _currentSpeed;
        Vector3 move = new Vector3(horizontal.x, _velocity.y, horizontal.z);
        return move;
    }
}
