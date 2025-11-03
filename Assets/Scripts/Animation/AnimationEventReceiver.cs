using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    [SerializeField] private StateManager _stateManager;

    public void OnAttack()
    {
        if (_stateManager != null && _stateManager.Attack != null)
        {
            _stateManager.Attack.TryAttack();
        }
    }
}