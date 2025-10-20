using UnityEngine;

public class LocomotionFeed : MonoBehaviour
{
    [Header("Ground Check")]
    public Transform GroundCheck;
    public float GroundCheckRadius;
    [SerializeField] private LayerMask _groundMask;

    private CharacterController _controller;

    public float HorizontalSpeed { get; private set; }
    public Vector3 HorizontalVelocity { get; private set; }
    public float VerticalVelocity { get; private set; }
    public bool IsGrounded { get; private set; }

    public Vector3 PlayerRight { get { return transform.right; } }
    public Vector3 PlayerForward { get { return transform.forward; } }
    // public Vector3 PlayerForward => transform.forward;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null)
        {
            Debug.LogError("[LocomotionFeed] 캐릭터 컨트롤러 누락");
        }

        if (GroundCheck == null)
        {
            // GroundCheck가 없을 경우 자동 생성
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.up * 0.1f;
            GroundCheck = go.transform;
        }
    }

    void Update()
    {
        Vector3 vel = _controller.velocity; // 캐릭터 컨트롤러의 현재 속도
        Vector3 horizontal = vel;           // 수평 속도만 계산용
        horizontal.y = 0f;

        HorizontalVelocity = horizontal;
        HorizontalSpeed = horizontal.magnitude;
        VerticalVelocity = vel.y;

        bool grounded = false;

        if (GroundCheck != null)
        {
            Collider[] hits = Physics.OverlapSphere(
                GroundCheck.position,
                GroundCheckRadius,
                _groundMask,
                QueryTriggerInteraction.Ignore);
            if (hits != null && hits.Length > 0)
            {
                grounded = true;
            }
        }

        IsGrounded = grounded;
    }

    private void OnDrawGizmosSelected()
    {
        if (GroundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(GroundCheck.position, GroundCheckRadius);
        }
    }
}
