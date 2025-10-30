using System.Collections;
using UnityEngine;

/// <summary>
/// wjr FSM zjsxprtmxmdlwk wjsghksrl
///     rhddyd epdlxj(zjavhsjsxm, vkfkalxj, fjsxkdla zotl) qhrhks
///     guswo tkdxodml Enter/Update/Exit ghcnf
///     tkdxo wjsghk sdycjddmf dkswjsgl tngod
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class StateManager : MonoBehaviour
{
    [Header("Modules")]
    public EnemySenses Senses;//tldi ahebf(rjfl/rkr/rkfla)
    [SerializeField] private Health _health;//cpfur(tkakd xmflrj)
    public Transform Player;//cnrur/rhdrur eotkd Transform
    [SerializeField] private CharacterController _controller;//vudwl dlehddyd CC

    [Header("Movement (flat)")]
    public float ChaseSpeed = 3.0f;//cnwjr threh(m/s)
    public float SearchSpeed = 1.8f;//tntor threh(m/s)
    [SerializeField] private float _rotateSpeed = 12.0f;//ghlwjs qhrks threh
    public float StoppingDistance = 1.2f;//ajacna rjfl
    [SerializeField] private float _gravity = -20.0f;//wndfur rkthreh(dmatn)

    [Header("Attack")]
    public float AttackRange = 1.8f;//rhdrur tkrjfl
    public float AttackDamage = 10.0f;//rhdrur eoalwl
    public float AttackCooldown = 1.2f;//rhdrur rksrur(ch)
    public statusEffects AttackEffect;

    [Header("Search")]
    public float SearchDuration = 3.0f;//tntor tlrks(ch)

    [Header("Debug")]
    [SerializeField] private bool _drawForward = false;//wjsqkd elqjrm fpdl

    [HideInInspector] public Vector3 LastKnownPos;//akwlakr vmffpdldj whkvy
    [HideInInspector] public float AttackTimer;//rhdrur znfekdns skadms tlrks
    [HideInInspector] public float SearchTimer;//tntor skadms tlrks

    [HideInInspector] public BaseState _currentState;//guswotkdxo
    [HideInInspector] public IdleState Idle;//Idle tkdxo dlstmxjstm
    [HideInInspector] public ChaseState Chase;//Chase tkdxo dlstmxjstm
    [HideInInspector] public AttackState Attack;//Attack tkdxo dlstmxjstm
    [HideInInspector] public SearchState Search;//Search tkdxo dlstmxjstm
    [HideInInspector] public DeadState Dead;//Dead tkdxo dlstmxjstm

    public StatusEffectHost StatusHost;

    private void Awake()
    {
        if (_controller == null)
        {
            _controller = GetComponent<CharacterController>();
        }

        if (_health == null)
        {
            _health = GetComponent<Health>();
        }

        if (Player == null)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            float closestDist = float.PositiveInfinity;
            GameObject closestPl = null;
            for (int plIdx = 0; plIdx < players.Length; plIdx++)
            {
                var curPl =players[plIdx];
                var dist = Vector3.Distance(transform.position, curPl.transform.position);
                if (closestDist > dist)
                {
                    closestDist = dist;
                    closestPl = curPl;
                }
            }
            GameObject go = closestPl;

            if (go != null)
            {
                Player = go.transform;
            }
        }

        if (Senses.Target == null)
        {
            Senses.Target = Player;
        }

        if (StatusHost == null)
        {
            StatusHost = GetComponent<StatusEffectHost>();
        }

        //tkdxo dlstmxjstm todtjd alc zjsxprtmxm wndlq
        //durltj wkrtjdgks State rocpemfdms monobehaviorrk djqtdmamfh wlrwjq todtjdgodigka
        Idle = new(this);
        Chase = new(this);
        Attack = new(this);
        Search = new(this);
        Dead = new(this);

        //chrl fjsxkdla zotlrkqt tpxld
        LastKnownPos = transform.position;
        AttackTimer = 0.0f;
        SearchTimer = 0.0f;

        //chlch tkdxo wlsdlq: Idle
        RequestStateChange(Idle);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        //tkakd cpzmsms dlfrhkf rkatl(tkdxo antl)
        if (_health != null)
        {
            if (_health.CurrentHealth <= 0.0f)
            {
                if (_currentState != Dead)
                {
                    RequestStateChange(Dead);
                }
                return;
            }
        }

        //wjsqkd elqjrm tjfwjdtl
        if (_drawForward)
        {
            Debug.DrawRay(transform.position + Vector3.up * 1.0f, transform.forward * 1.5f, Color.yellow, 0.02f);
        }

        //guswo tkdxo djqepdlxm
        if (_currentState != null)
        {
            _currentState.OnUpdate(dt);
        }
    }

    /// <summary>
    /// dkswjstks tkdxo wjsghks
    /// Exit => tkdxo rycp => Enter ghcnf
    /// null wjsekfdms antl
    /// </summary>
    public void RequestStateChange(BaseState next)
    {
        if (next == null)
        {
            return;
        }

        if (_currentState != null)
        {
            _currentState.OnExit();
        }

        _currentState = next;
        _currentState.OnEnter();
    }

    #region rhdxhd dbxlf(rkr tkdxodptj ghcnf)
    /// <summary>
    /// vudwl wjscpdptj ahrvywjadmf gidgo ghlwjs(tnvud qhwjd vhgka)
    /// </summary>
    public void FacePosition(Vector3 target, float dt)
    {
        Vector3 flatTarget = target;
        flatTarget.y = transform.position.y;

        Vector3 to = flatTarget - transform.position;//tnvud qkdgid
        to.y = 0.0f;

        if (to.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, dt * _rotateSpeed);
        }
    }

    /// <summary>
    /// vudwl wjscpdptj wjswls dlehd(wndfur qhwjd vhgka)
    /// </summary>
    public void MoveForward(float speed, float dt)
    {
        Vector3 move = transform.forward * speed;//wjswls threh
        move.y = _gravity;//rudrP dkswjdghkdyd wndfur

        _controller.Move(move * dt);
    }

    /// <summary>
    /// vmffpdldjdhkdml guswo rjfl qksghks
    /// </summary>
    public float DistanceToPlayer()
    {
        if (Player == null)
        {
            return float.PositiveInfinity;
        }

        float d = Vector3.Distance(transform.position, Player.position);
        return d;
    }

    /// <summary>
    /// guswo tkdxoaud answkduf(elqjrm/HUD dyd)
    /// </summary>
    /// <returns></returns>
    public string CurrentStateName()
    {
        if(_currentState == null)
        {
            return "None";
        }

        return _currentState.Name();
    }
    #endregion
}
