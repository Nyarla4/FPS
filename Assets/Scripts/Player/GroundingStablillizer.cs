using UnityEditor.Connect;
using UnityEngine;

/// <summary>
/// rudtkxndud, tmxpqqhwjd, tmsoqekdns wprhddml dkswjdghk ahebf
/// CharacterController dlehdwnd rPeks/rudtk/dptwl tkdghkddml ajacna/xnr/qndEma wnfdutj cprka vnawlf gidtkd
/// gortla dkswjswkdcl
/// 1. rhdwnd(wjqwlX) ghrdms fpdl alglxmaus qhwjdX(dhqhwjddmfh dlsgks dlehd wkarla qkdwl)
/// 2. wjthrdls ruddn rudtkxndud gkwl dksgdma(dmlal djqtsms Ejffladuddur)
/// 3.
/// 4. tmsoq qhwjddms skwdmsfpdlglxm+shvdmsfpdlalglxm dlfEoaks tngod
/// 5. tmsoqekdnsdms tkdtmdwnddpsms altngod(wjavm djrdkq qkdwl)
/// </summary>
public class GroundingStablillizer : MonoBehaviour
{
    [Header("Gound Probe")]
    public float ProbeDistance = 0.6f; //qkekr xkatkdyd fpdl rlfdl(vmffpdldj qkfqhek dirrks rlfrp)
    public LayerMask GroundMask;

    [Header("Slope Projection")]
    public float SlopeStickAngleBias = 2.0f;//cc.slopelimitqhek dirrks wkrdms gjdyd dudb rkr(alRMffla qkdwldyd dhkscnd)
    public float MinMoveSpeedForProjection = 0.4f;//dlehdthrehrk dl rkqt alaksdls ruddn alxndud(wjdwl rmscj EJffla qkdwl)
    public float MaxProjectionLossRatio = 0.5f;//xndud gn tnvudrlfdlrk dnjsfl 50%alaksdls ruddn dnjsqhs dbwl(ajacna qkdwl)

    [Header("Step Smoothing")]
    public float StepUpSmoothTime = 0.06f;//rPeks tkdtmd qhrks tlrks(Wkfqrh qnemfjqrp)
    public float MaxStepHeight = 0.4f;//tmxpqdmfh tkdtmdtlzlf tn dlTsms chleo shvdl(rhktkdtmd qkdwldyd)
    public float StepProbeForward = 0.25f;//wjsqkd xkatk rjfl(sjan rlfaus dkvqur dhvks rksmdtjd dlTdma)

    [Header("Snap Down")]
    public float SnapDownDistance = 0.35f;//qkfalx rksrmr apdnf chleo rjfl
    public float SnapDownSpeed = 20.0f;//tmsoq ekdnstl 1chekd chleo gkrkdfid(qnemfjqrp qnxdlrl dnlgka)

    //soqn tkdxo(tmxpq qhwjddyd qustn)
    private float _stepUpVelocity;//SmoothDampdyd soqn threh zotl
    private float _currentStepOffset;//guswo snwjr tmxpq tkdtmdcl(y dhvmtpt)

    /// <summary>
    /// dlehdqprxjfmf guswo qkekr qjqtjs rlwnsdmfh 'vudausxndud' => rudtkfmf Ekfkrkrp aksema
    /// </summary>
    public Vector3 ProjectOnGround(Vector3 move, CharacterController cc, PlayerController pc)
    {
        if (cc == null)
        {
            return move;//CCrk djqtsms ruddn xndud qnfrksmd, rhfh dnjsqhs dbwl
        }

        //guswo tnvud dlehd threh cnwjd(qhwjd duqn vkseks)
        float dt = Time.deltaTime;
        float speedPlanar = 0f;//guswo tnvud threh(m/s)
        if (dt > 0)
        {
            //dlqjs vmfpdla dlehdfid X, chekd threh
            speedPlanar = new Vector2(move.x, move.z).magnitude;//tnvud tjdqns zmrl
        }

        //wjdwl tkdxo vkseks(wjdwlrk dksls ruddn rudtkxndud gkwl dksgdma)
        bool grounded = false;
        if (pc != null)
        {
            //pc soqn wjqwl duqn rhdrork djqtekaus dkswjsgkrp cc rbclrdp dnldlagkrjsk qhtnwjrdmfh true/false wjdcor tjsxor
            grounded = pc.IsGround;
        }
        else
        {
            grounded = cc.isGrounded;
        }

        if (!grounded)
        {//rhdwnddlaus xndud rmawl
            return move;
        }

        //sjan smfls ruddn xndudgkwl dskgdma(wjdwl rmscjdml altp Ejffla qkdwldyd)
        if (speedPlanar < MinMoveSpeedForProjection)
        {
            return move;
        }

        //qkekr qjqtjs rngkrl
        Vector3 normal;
        bool got = TryGetGroundNormal(cc, out normal);
        if (!got)//fpdl alglxmtl xndudgkwl dksgdma
        {
            return move;
        }

        //rudtk rkreh rjatk: CCrk dhfmf tn dqjtsms rmq rudtkaus xndud tmzlq
        float slope = Vector3.Angle(normal, Vector3.up);//qjqtjsrhk Updml rkreh(0: vudwl, 90: tnwlr)
        float limit = cc.slopeLimit - SlopeStickAngleBias;
        if (slope > limit)
        {//rmqrudtkdptjsms dpswls rbclr(CC)ehdwkrdp akxrla
            return move;
        }

        //vudaus xndud: movefmf qkekr qjqtjsdp snqgu 'ausdmf Ekfk' dlehd
        Vector3 onPlane = Vector3.ProjectOnPlane(move, normal);

        //xnduddmfh rlfdlrk rhkehgkrp wnfdjTdmaus dnjsqhs dbwl(ajacna qkdwldyd)
        float originalLen = new Vector2(move.x, move.z).magnitude;
        float projectedLen = new Vector2(onPlane.x, onPlane.z).magnitude;

        if (originalLen > 0.0001f)
        {
            float ratio = projectedLen / originalLen;//xndud gn rlfdl qldbf
            if (ratio < MaxProjectionLossRatio)
            {//wjfqks dlgkfh wnfaus dnjsqhs dbwl(cmraus qurdp 'gmqckr'ehlwl dksgehfhr)
                return move;
            }
        }

        //tnwlr threh(y)sms dnjsqhs dbwl(tnvudaks snqglehfhr)
        onPlane.y = move.y;
        return onPlane;
    }

    /// <summary>
    /// wkrdms xjrdmf qnemfjqrp sjaehfhr tkdtmd qhrks wjrdyd
    /// 'skwdms fpdl glxm + shvdms fpdl alglxm'dptjaks tmxpqdmfh vkswjdcjfl
    /// </summary>
    public Vector3 ApplyStepSmoothing(Vector3 move, CharacterController cc, PlayerController pc)
    {
        if (cc == null)
        {//ccrk djqtsm sruddn qhwjd qnfrksmd
            return move;
        }

        //tnvud dlehddl rjdml djqtdmaus tmxpq qhwjd eotls 'tjtjgl qhrrnl'aks wjrdyd
        Vector2 planner = new Vector2(move.x, move.z);
        if (planner.sqrMagnitude <= 0.000001f)
        {
            return ReleaseStepGradually(move);
        }

        //wjsqkd qkdgid rPtks(tnvud qkdgidaks)
        Vector3 dir = GetHorizontalDir(move);//tnvud eksdnl qprxj

        //fpdl dnjswja enro(skwdms/shvdms) - skwdms wjadms xjrdp rjfflrh shvdms wjadms rjfflwl dksgdmaus 'rPeks'dmfh vkswjd
        Vector3 originLow = cc.transform.position + Vector3.up * (cc.stepOffset * 0.5f);//skwdms dnjswja
        Vector3 originHigh = cc.transform.position + Vector3.up * (cc.stepOffset * 0.08f);//shvdms dnjswja(whrma ej dnl)

        //wjsqkd fpdl rlfdl(sjan rlfaus dkvqurdmf xjrdmfh dhvksgkf tn dlTdma)
        float castDist = Mathf.Max(cc.radius + StepProbeForward, 0.15f);

        //fpdlzotmxm tngod(xmflrj wpdhl)
        RaycastHit hitLow;
        bool lowHit = Physics.Raycast(originLow, dir, out hitLow, castDist, GroundMask, QueryTriggerInteraction.Ignore);
        RaycastHit hitHigh;
        bool highHit = Physics.Raycast(originHigh, dir, out hitHigh, castDist, GroundMask, QueryTriggerInteraction.Ignore);

        //skwdms fpdlsms akwrh shvdms fpdlsms qlaus 'xjr'dmfh rkswn, tkdtmd qhrks cjfl
        if (lowHit && !highHit)
        {
            //ahrvy tkdtmdfid: skwdms fpdlrk Wlrgls wlwjadml shvdl ckdl
            float desireUp = hitLow.point.y - cc.transform.position.y;
            if (desireUp < 0)
            {//gkrkd qkdgiddms tmxpq tkdtmddmfh qlcjfl
                desireUp = 0f;
            }
            if (desireUp > MaxStepHeight)
            {//rhktkdtmd qkdwl
                desireUp = MaxStepHeight;
            }

            //SmoothDampfh _currentStepOffsetdmf desireUpdmfh aoRMsgkrp tnfuacjfl
            float dt = Time.deltaTime;
            float smoothed = Mathf.SmoothDamp(
                _currentStepOffset, //guswo rkqt
                desireUp,           //ahrvy rkqt
                ref _stepUpVelocity,//soqn threh zotl(ckawhdyd)
                StepUpSmoothTime,   //qhrks tlrks
                Mathf.Infinity,     //chleo threh(anwpgks)
                dt                  //vmfpdla tlrks
                );

            //dlqjs vmfpdladptj wmdrkgk stkdtmdfid(epfxk) rPtks
            float delta = smoothed - _currentStepOffset;
            _currentStepOffset = smoothed;

            //chlwhd y = rlwhs y + epfxk(rhktkdtmd wpgksehla)
            float newY = move.y + delta;
            if (newY > MaxStepHeight)
            {
                newY = MaxStepHeight;
            }
            move.y = newY;
        }
        else
        {//xjr tkdghkddl dksls ruddn
            //qhwjdrkqtdmf wjawlswjrdmfh 0dmfh ghlqhr
            move = ReleaseStepGradually(move);
        }

        return move;
    }

    /// <summary>
    /// qkfalx rksrmrdl wkrrp todruTdmfEo 'Wkfqrp dkfofh' dlehdtlzu qkekrdp qnxdlsms tmsoqekdns
    /// tkdtmdwnddpsms tngodX(wjavm/tkdtmd djrdkq qkdwl)
    /// </summary>
    public void TrySnapDown(CharacterController cc, PlayerController pc)
    {
        if (cc == null || cc.velocity.y > 0f)
        {//ccrk djqtrjsk tkdtmdwnddlaus tmsoqekdns rmawl(wjavm/dhfmakr wlsgod qkdgo qkdwl)
            return;
        }

        //fpdl tlwkrwja: vmffpdldj qkf dkfo tkfWkr dnlwlwja
        Vector3 start = cc.transform.position + Vector3.up * 0.1f;

        //dkfofh fpdlfmf Thktj rkRkdns qkekr rksrur cmrwjd
        RaycastHit hit;
        bool got = Physics.Raycast(
            start,              //tkwkrwja
            Vector3.down,       //dkfo qkdgid
            out hit,            //glxm rufrhk
            SnapDownDistance + 0.1f,//xkatk rjfl(dudbqns vhgka)
            GroundMask,         //qkekr fpdldj
            QueryTriggerInteraction.Ignore//xmflrj antl
        );

        if (got)
        {
            //guswo dnlcldhk qkekr tkdl tlfwp rksrur
            float gap = (start.y - hit.point.y) - 0.1f;//dudbqns rPtks

            //rksrmrdl sjan wkrdms ruddn antl(altp Ejffla qkdwl)
            if (gap < 0.015f)
            {
                return;
            }

            //gks vmfpdladp dlehdgkf gkrkdfid(sjan zmrp EJfdjwlwl dksgehfhr wpgks cjfl)
            float step = Mathf.Min(gap, SnapDownSpeed * Time.deltaTime);

            //dkfofh Move - CC rbclr(cndehf/tmffhvm)dmf Ekfma
            Vector3 down = Vector3.down * step;
            cc.Move(down);
        }
    }

    #region soqn dbxlf gkatn

    /// <summary>
    /// qkekr qjqtjs qprxj ghlremr
    /// vmffpdldj qkf dkfofh fpdlzotmxm, qkekr qjqtjs cnwjd
    /// </summary>
    private bool TryGetGroundNormal(CharacterController cc, out Vector3 normal)
    {
        normal = Vector3.up;//rlqhs rqkt(vudwl rkwjd)

        //fpdl tlwkrwjadmf qkfqhek dirrks dnlfh wkqdk wkqdma rkath
        Vector3 origin = cc.transform.position + Vector3.up * 0.2f;
        RaycastHit hit;
        bool got = Physics.Raycast(
            origin,                 //tlwkrwja
            Vector3.down,           //dkfoqkdgid
            out hit,                //glxm rufrhk
            ProbeDistance + 0.25f,  //xkatk rjfl(dudbcjfl)
            GroundMask,             //fpdldj
            QueryTriggerInteraction.Ignore//xmflrj antl
        );

        if (got)
        {
            //tlfwp qkekr qjqtjs
            normal = hit.normal;
            return true;
        }

        return false;
    }

    /// <summary>
    /// tnvud qkdgid eksdnl qprxj rngksek(y=0, wjdrbghk cjfl)
    /// tnvud eksdnl qprxj rPtks
    /// </summary>
    private Vector3 GetHorizontalDir(Vector3 move)
    {
        Vector3 d = move;//wldur qhrtk(dnjsqhs qhwhsdyd)
        d.y = 0f;//tnvud tjdqnsaks tkdyd

        if (d.sqrMagnitude > 0f)
        {
            d.Normalize();//0dl dksls ruddn wjdrbghk
        }

        return d;
    }
    /// <summary>
    /// tmxpq tkdtmd qhwjdclfmf tjtjgl 0dmfh ehlehfflau, qusghkqnsdmf vmfpdla dlehddp qksdud
    /// tjtjgl qhrrnl
    /// </summary>
    private Vector3 ReleaseStepGradually(Vector3 move)
    {
        float dt = Time.deltaTime;

        //_currentStepOffsetdmf 0dmfh qnemfjqrp tnfua
        float smoothed = Mathf.SmoothDamp(
            _currentStepOffset, //guswo rkqt
            0f,                 //ahrvy rkqt
            ref _stepUpVelocity,//soqn threh zotl(ckawhdyd)
            StepUpSmoothTime,   //qhrks tlrks
            Mathf.Infinity,     //chleo threh(anwpgks)
            dt                  //vmfpdla tlrks
        );

        //dlqjs vmfpdladptj qusghkgks akszma ydp ejgowna
        float delta = smoothed - _currentStepOffset;
        _currentStepOffset = smoothed;
        move.y += delta;
        return move;
    }

    #endregion
}
