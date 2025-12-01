using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TestPlayerControl : MonoBehaviour
{
    [SerializeField] private CharacterController _cc;
    [SerializeField] private GunKind_SO _gun;

    private Vector2 _moveInput;
    private bool _sprintHeld;//질주 버튼 눌렸는지 여부
    public bool SprintHeld => _sprintHeld;
    private float _currentSpeed;
    private Vector3 _velocity;

    private float _lastGroundedTime;//마지막 착지시각
    private float _lastJumpPressTime;//마지막 점프입력시각

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

        //희망 방향
        Vector3 wish = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        //정규화
        if (wish.sqrMagnitude > 1.0f)
        {
            wish.Normalize();
        }

        _cc.Move(wish * dt * _moveSpeed);
    }

    public void GetMagazine(int ammo)
    {
        _gun.ReserveAmmo += ammo;
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
