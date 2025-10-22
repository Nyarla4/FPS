using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// dlehd threhdp Ekfk tmxpq xpavh rPtks, dhlsqkf/dhfmsqkf dlqpsxm ryeo qkftod
/// PC + CC egkaRp ehdwkr
/// </summary>
[DisallowMultipleComponent]
public class StepEventSource : MonoBehaviour
{
    [Header("Refs")]
    public CharacterController Controller;//guswo threh whghldyd
    public PlayerController Player;//rjerl/ekfflrl tkdxo ckawh(djqtdmaus threhfh vkseks)

    [Header("Step Timing")]
    public float WalkStepsPerSecond = 1.8f;//rjerl xpavh(Hz)
    public float SprintStepsPerSecond = 2.6f;//ekfflrl xpavh(Hz)
    public float MinSpeedForSteps = 1.0f;//dl threh alaksdls ruddn tmxpq qkftodX(wjdwlsk dkwnsmfls dlehdtl djrwp)
    public float GroundedGraceTime = 0.08f;//wjqwl dlxkf gn Wkfqdms tlrks sodpsms tmxpq gjdyd(rPeks/xjr qhwjd)

    [Header("Events")]
    public UnityEvent OnStepLeft;//dhlsqkf
    public UnityEvent OnStepRight;//dhfmsqkf

    private bool _leftNext = true;//ekdmaqkf: true-dhlsqkf, false-dhfmsqkf
    private float _nextStepTime;//ekdma tmxpq qkftod dPwjd tlrkr(wjfeotlrks)
    private float _lastGroundedTime;//chlrms wjqwl tlrkr(rkseksgks zhdyxpxkdla dydeh)

    private void Awake()
    {
        if (Controller == null)
        {
            Controller = GetComponent<CharacterController>();
        }

        if (Player == null)
        {
            Player = GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        float now = Time.time;

        //tlrks rksrurdmfh cpzmgotj rkr qkf dlqpsxm tlfgod
        bool grounded = false;
        if (Controller != null)
        {
            grounded = Controller.isGrounded;
        }

        if (grounded)
        {
            _lastGroundedTime = now;
        }

        //guswo tnvud threh(tmxpq xpavh tkswjddydeh)
        float planarSpeed = 0.0f;
        if (Controller != null)
        {
            Vector3 v = Controller.velocity;
            v.y = 0.0f;
            planarSpeed = v.magnitude;
        }

        //chlth threhqhek smfls ruddn xkdlaj fltpt(tmxpq cjflgkwldksgdam)
        if (planarSpeed < MinSpeedForSteps)
        {
            if (now > _nextStepTime)
            {
                _nextStepTime = now;
            }
            return;
        }

        //rjerl/ekfflrl xpavh rufwjd
        float stepsHz = WalkStepsPerSecond;
        bool isSprint = planarSpeed > (MinSpeedForSteps + 2.0f);//rlqhs threh rlqks
        if (Player != null)
        {
            //vmffpdldjrk dlTsms ruddn vmffpdldjdml tmvmflsxm snfmsrjf tmvmflsxm duqnfh vkseks
            isSprint = Player.SprintHeld;
        }

        if (isSprint)
        {
            stepsHz = SprintStepsPerSecond;
        }

        //ekdma tmxpq tlrks chrlghk(chlch wlsdlqtldpaks)
        if (_nextStepTime <= 0.0f)
        {
            _nextStepTime = now + (1.0f / stepsHz);
        }

        //wjqwl Ehsms zhdyxp tlrks sodptjaks tmxpq qkftod
        bool canStep = grounded || ((now - _lastGroundedTime) <= GroundedGraceTime);

        if (canStep)
        {
            if (now >= _nextStepTime)
            {
                if (_leftNext)
                {
                    OnStepLeft?.Invoke();
                    _leftNext = false;
                }
                else
                {
                    OnStepRight?.Invoke();
                    _leftNext = true;
                }

                _nextStepTime = now + (1.0f / stepsHz);
            }
        }
    }
}
