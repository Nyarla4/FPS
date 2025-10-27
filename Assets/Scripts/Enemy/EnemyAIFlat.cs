using System.Collections.Generic;
using UnityEngine;

public enum State
{
    Idle = 0,
    Chase = 1,
    Attack = 2,
    Patrol = 3,
    Dead = 4,
}

/// <summary>
/// spqlaptl djqtdl vudwl wlrtjs dlehdaksdmfh cnrur/rhdrur/tntor tngod ekstns FSM
///     Idle: eorl, qkfrustl Chasefh wjsghk
///     Chase: akwlakrdmfh qhs dnlclfmf gidgo wjswls(tnvud ghlwjs qhrks)
///     Attack: tkrjfl dlsodlf Eo znfekdns wlzlau vmffpdldjdp vlgo qndu
///     Patrol: tldi tkdtlf gn akwlakr dnlcl rmsqkddptj xkator gn Idlefh qhrrnl
///     Dead: Healthrk 0dlgkdls ruddn wjdwl(doslapdltusdlsk vkrhlsms Health.OnDeathdptj cjfl)
/// </summary>
public class EnemyAIFlat : MonoBehaviour
{
    [Header("Modules")]
    [SerializeField] private EnemySenses _enemySenses;//tldi ahebf(qkfrus/tkdtlf vkswjd)
    [SerializeField] private Health _health;//cpfur(tkakd tkdxo wjsdl xmflrj)
    [SerializeField] private Transform _player;//cnrur/rhdrur eotkd xmfostmvha
    [SerializeField] private PlayerHealth _playerDamageable;//eotkd IDamageable(vlgo wjsekfdyd)
    [SerializeField] private CharacterController _controller;//ekstns dlehddmf dnlgks cc(cndehf/ rudtk dkswjdghk)

    [Header("Movement (flat)")]
    [SerializeField] private float _chaseSpeed = 3.0f;//cnrur threh(m/s). vudwl wjswls threh
    [SerializeField] private float _rotateSpeed = 12.0f;//ghlwjs qhrks threh(shvdmaus ej Qkfmek)
    [SerializeField] private float _stoppingDistance = 1.2f;//wjswls wjdwl rlwns(emfWnrrjfla qkdwldyd)
    [SerializeField] private float _gravity = -20.0f;//altp wjqwl dkswjdghkfmf dnlgks wndfur rkthreh(dmatn)

    [Header("Attack")]
    [SerializeField] private float _attackRange = 1.8f;//rhdrur tkrjfl(m)
    [SerializeField] private float _attackDamage = 10.0f;//vlgofid
    [SerializeField] private float _attackCooldown = 1.2f;//rhdrur rksrur(ch). 0dlsruddn dusxk

    [Header("Patrol")]
    [SerializeField] private float _patrolDuration = 3.0f;//tldi tkdtlf gn akwlakr dnlcl rmsqkd xkator tlrks(ch)

    [Header("Debug")]
    [SerializeField] private bool _drawForward = false;//wjsqkd elqjrm fpdl vyrl duqn

    [SerializeField]//rktlghk
    private State _state;//guswo FSM tkdxo
    private float _attackTimer;//rhdrur znfekdns zkdnsxj(0dlfEo rhdrur tlfgod)
    private float _patrolTimer;//patrol wksdu tlrks
    private Vector3 _lastKnownPos;//akwlakrdmfh qhs vmffpdldjdml whkvy(tldi tkdtlf eoql)

    [SerializeField] private List<Transform> _patrolPoints;

    private void Awake()
    {
        if (_controller == null)
        {
            _controller = GetComponent<CharacterController>();
        }

        //chrlghk
        _state = State.Idle;
        _attackTimer = 0.0f;
        _patrolTimer = 0.0f;
        _lastKnownPos = transform.position;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        //rhdrur znf rkath
        if (_attackTimer > 0.0f)
        {
            _attackTimer -= dt;
            if (_attackTimer < 0.0f)
            {
                _attackTimer = 0.0f;
            }
        }

        //tkakd tkdxo wjsdl
        if (_health != null)
        {
            if (_health.CurrentHealth <= 0.0f)
            {
                if (_state != State.Dead)
                {
                    EnterDead();
                }
                return;//Dead tkdxodls ruddn ejdltkd cjflgkwl dksgdma
                //eocpfhsms vkrhltlzlamfh zmrp dudgiddmf wnwl dksgdma
            }
        }

        //tldi cpzm, rksmdgkekaus _lastKnownPos rodtls
        bool seen = false;
        Vector3 seenPos = Vector3.zero;
        if (_enemySenses != null)
        {
            if (_enemySenses.CanSeeTarget(out seenPos))
            {
                seen = true;
                _lastKnownPos = seenPos;
            }
        }

        //tkdxo djqepdlxm
        switch (_state)
        {
            case State.Idle:
                UpdateIdle(seen);
                break;
            case State.Chase:
                UpdateChase(seen);
                break;
            case State.Attack:
                UpdateAttack(seen);
                break;
            case State.Patrol:
                UpdatePatrol(seen);
                break;
        }

        //wjsqkd elqjrm fpdl(tlrkr ghkrdlsdyd)
        if (_drawForward)
        {
            Debug.DrawRay(transform.position + Vector3.up * 1.0f, transform.forward * 1.5f, Color.yellow, 0.02f);
        }
    }

    #region tkdxo wlsdlq/djqepdlxm

    private void EnterIdle()
    {
        _state = State.Idle;
        //epahfktj Idledptj gksms dlfdms djqtek, qkfrusgkaus qkfh Chasefh sjadjrka
    }

    private void UpdateIdle(bool seen)
    {
        if (seen)
        {//qkfrustl wmrtl cnrur tlwkr
            EnterChase();
            return;
        }
        //alqkfrustl Idle dbwl
    }

    private void EnterChase()
    {
        _state = State.Chase;
        //qufeh chrlghk djqtdma
        //UpdateChasedptj ghlwjs alc wjswls cjfl
    }

    private void UpdateChase(bool seen)
    {
        //ghlwjs
        Vector3 targetPos = _lastKnownPos;
        Vector3 flatTarget = targetPos;
        flatTarget.y = transform.position.y;//vudwlfh rkwjd => tnvud yrkqt ehddlfghk cjfl

        Vector3 to = flatTarget - transform.position;//dlehd/ghlwjs rlwns qprxj
        to.y = 0.0f;//tnvudaks rhfu

        if (to.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * _rotateSpeed);
        }

        //wjdwlrjfl/wjswls
        float dist = Vector3.Distance(transform.position, flatTarget);

        if (dist > _stoppingDistance)
        {
            //wjswls qprxj(dnjfem wjsqkd rlwns)
            Vector3 move = transform.forward * _chaseSpeed;

            //cc dkswjdghkfmf dnlgo wndfur qhwjd(vudwldptjeh rudrPdptjdml Ejffla wnfdlrl dnlgka)
            move.y = _gravity;

            _controller.Move(move * Time.deltaTime);
        }

        if (_player != null)
        {//rhdrur tkrjfl wlsdlqtl Attackdmfh
            float d2 = Vector3.Distance(transform.position, _player.position);
            if (d2 <= _attackRange)
            {
                EnterAttack();
                return;
            }
        }

        //tldi tkdtlf tkdxodptj akwlakr whkvydp ehekfgksms ruddn Patrolfh
        if (!seen)
        {
            if (dist <= _stoppingDistance)
            {
                EnterPatrol();
                return;
            }
        }
    }

    private void EnterAttack()
    {
        _state = State.Attack;
        //rhdrur tkdxodptjsms wjdwl/ghlwjs/znfekdns rlqksdmfh vlgo wjrdyd
    }

    private void UpdateAttack(bool seen)
    {
        //tkrjfl dbwlrk Rowlsms ruddn Chasefh qhrrnl
        if (_player != null)
        {
            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist > _attackRange)
            {
                EnterChase();
                return;
            }
        }

        //tldirk Rmsgrlaus Patrolfh
        if (!seen)
        {
            EnterPatrol();
            return;
        }

        //rhdrur tngod cjfl
        if (_attackTimer <= 0.0f)
        {
            DoAttack();
            _attackTimer = _attackCooldown;
        }

        //rhdrur tkdxodptjeh vmffpdldjfmf gidgo ghlwjs dbwl
        if (_player != null)
        {
            Vector3 dir = _player.position - transform.position;
            dir.y = 0.0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * _rotateSpeed);
            }
        }
    }

    /// <summary>
    /// tlfwp eoalwlfmf vmffpdldj IDamageabledp wjsekf
    /// </summary>
    private void DoAttack()
    {
        if (_playerDamageable == null)
        {
            return;
        }

        //duscnfdyd dlswk : hitPointsms vmffpdldj dnlcl, qjqtjsdms Vector3.up tkdyd
        _playerDamageable.ApplyDamage(_attackDamage, _player.position, Vector3.up, transform);
    }

    private void EnterPatrol()
    {
        _state = State.Patrol;
        _patrolTimer = _patrolDuration;
    }

    private void UpdatePatrol(bool seen)
    {
        //akwlakr dnlclfmf gidgo wjqrms(ghlwjs + skwdms threhfh wjswls)
        Vector3 flatTarget = _lastKnownPos;
        flatTarget.y = transform.position.y;

        Vector3 to = flatTarget - transform.position;
        to.y = 0.0f;

        float dist = to.magnitude;

        if (dist > _stoppingDistance)
        {
            Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * _rotateSpeed);

            Vector3 move = transform.forward * _chaseSpeed * 0.6f;//patroldms chaseqhek smflehfhr
            move.y = _gravity;
            _controller.Move(move * Time.deltaTime);
        }

        //qkfrusgkaus Chasefh
        if (seen)
        {
            EnterChase();
            return;
        }

        //xkator tlrksdl Rmxsks ruddn Idlefh qhrrnlcjfl
        _patrolTimer -= Time.deltaTime;
        if (_patrolTimer <= 0.0f)
        {
            EnterIdle();
        }
    }

    private void EnterDead()
    {
        _state = State.Dead;
    }
    #endregion
}
