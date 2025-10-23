using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform _playerBody; //yaw
    [SerializeField] private Transform _cameraPivot;//pitch

    [Header("Look")]
    [SerializeField] private float _mouseSensitivity = 2.5f; // 기본 감도
    [SerializeField] private float _pitchMin = -89f;
    [SerializeField] private float _pitchMax = 89f;

    private Vector2 _lookInput;
    private float _yaw;
    private float _pitch;

    public float SensitivityMultiplier = 1.0f;

    private void Awake()
    {
        if (_playerBody == null)
        {
            Debug.LogError("[MouseLook] playerBody 할당이 필요합니다.");
        }
        if (_cameraPivot == null)
        {
            Debug.LogError("[MouseLook] cameraPivot 할당이 필요합니다.");
        }

        // 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Look 입력 반영(프레임 독립 보정)
        float dx = _lookInput.x * _mouseSensitivity * 10f * dt * SensitivityMultiplier;//좌우
        float dy = _lookInput.y * _mouseSensitivity * 10f * dt * SensitivityMultiplier;//상하

        _yaw += dx;
        _pitch -= dy;

        // Pitch 클램프
        _pitch = Mathf.Clamp(_pitch, _pitchMin, _pitchMax);

        //적용
        if (_playerBody != null)
        {
            Quaternion yRot = Quaternion.Euler(0f, _yaw, 0f);
            _playerBody.rotation = yRot;
        }
        if (_cameraPivot != null)
        {
            Quaternion xRot = Quaternion.Euler(_pitch, 0f, 0f);
            _cameraPivot.localRotation = xRot;
        }
    }

    // PlayerInput → Events에서 연결
    public void OnLook(InputAction.CallbackContext ctx)
    {
        if (ctx.performed == true || ctx.canceled == true)
        {
            _lookInput = ctx.ReadValue<Vector2>();
        }
    }

    public void SetSensitivityMultiplier(float adsMouseScale)
    {
        //너무 작으면 입력이 멈춘것으로 느껴짐
        //너무 크면 급회전 유발
        float requested = Mathf.Clamp(adsMouseScale, 0.01f, 5.0f);

        SensitivityMultiplier = requested;
    }
}
