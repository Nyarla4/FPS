using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TestPlayerControl : MonoBehaviour
{
    [SerializeField] private CharacterController _cc;
    
    private Vector2 _moveInput;
    private bool _sprintHeld;//���� ��ư ���ȴ��� ����
    public bool SprintHeld => _sprintHeld;
    private float _currentSpeed;
    private Vector3 _velocity;

    private float _lastGroundedTime;//������ �����ð�
    private float _lastJumpPressTime;//������ �����Է½ð�

    [SerializeField] private float _moveSpeed;

    private void Awake()
    {
        if (_cc == null)
        {
            _cc = GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        //��� ����
        Vector3 wish = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        //����ȭ
        if (wish.sqrMagnitude > 1.0f)
        {
            wish.Normalize();
        }

        _cc.Move(wish * dt * _moveSpeed);
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
}
