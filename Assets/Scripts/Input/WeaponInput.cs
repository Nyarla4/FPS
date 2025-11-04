using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 사격/재장전/ADS 입력 수집, WeaponController로 전달
/// 입력에는 상태X, 판단은 컨트롤러에서
/// </summary>
[DisallowMultipleComponent]
public class WeaponInput : MonoBehaviour
{
    public WeaponController Controller;//이벤트 전달 대상
    public CameraEffectsMixer Mixer;

    private bool _fireHeld;//좌클릭 누름
    private bool _adsHeld;//우클릭 누름
    private bool _reloadPressed;//재장전 눌린 프레임

    private void Update()
    {
        if (Controller == null)
        {
            return;
        }

        Controller.SetFireHeld(_fireHeld);
        Controller.SetAdsHeld(_adsHeld);

        if (_reloadPressed)
        {
            Controller.RequestReload();
            _reloadPressed = false;
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _fireHeld = true;
        }
        if (context.canceled)
        {
            _fireHeld = false;
        }
    }

    public void OnADS(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _adsHeld = true;
            if (Mixer != null)
            {
                Mixer.SetFOV(false);
            }
        }
        if (context.canceled)
        {
            _adsHeld = false;
            if (Mixer != null)
            {
                Mixer.SetFOV(true);
            }
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _reloadPressed = true;
        }
    }
}
