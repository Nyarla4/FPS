using UnityEngine;
using UnityEngine.InputSystem;

public class TestWeaponInput : MonoBehaviour
{
    public bool Triggered { get; private set; }
    public bool Reload { get; private set; }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Triggered = true;
        }
        if (context.canceled)
        {
            Triggered = false;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Reload = true;
        }
    }

    public void ReleaseReload()
    {
        Reload = false;
    }
}
