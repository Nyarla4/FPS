using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControl : MonoBehaviour
{
    [SerializeField] private CharacterController _cc;

    private void Awake()
    {
        if (_cc == null)
        {
            _cc = GetComponent<CharacterController>();
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
