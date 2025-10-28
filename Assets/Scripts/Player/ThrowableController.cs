using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// vmffpdldj xncjr zjsxmfhffj
///     qjxms xoq: rlqhstprl wmrtl ejswlrl
///     qjxms ghfem: vkdnj cnrcjr(dhqtus)dmf rjcu ejswlrl
/// </summary>
public class ThrowableController : MonoBehaviour
{
    [Header("Refs")]
    public Camera PlayerCamera; // ejswlf qkdgid rlwns
    public Transform Hand;// ths/ ejswlsms dnjswja
    public Grenade GrenadePrefab;//tnfbxks vmflvoq

    [Header("Throw")]
    public float BaseThrowSpeed = 14.0f;//rlqhs ejswlrl thrfur
    public float MaxThrowSpeed = 22.0f;//ghfemgksms ruddn chleo thrfur
    public float ChargeTime = 1.0f;//chleo tprlRkwl rjfflsms tlrks(ch)
    public float AngularSpin = 20.0f;//ghlwjs(tlrkr)

    private bool _charging;//vkdnj cndwjswnd duqn
    private float _charge;//0~1 snwjrehls vkdnj
    private bool _fireRequested;//dlqfur flfflwm rkawl

    void Update()
    {
        if (_fireRequested)
        {
            _fireRequested = false;
            ThrowOne();
        }

        if (_charging)
        {//vkdnj cndwjs cjfl
            float inc = 0.0f;
            if (ChargeTime > 0.0001f)
            {
                inc = Time.deltaTime / ChargeTime;
            }
            _charge += inc;
            if(_charge > 1.0f)
            {
                _charge = 1.0f;
            }
        }
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _charging = true;
            _charge = 0.0f;
        }
        else if (context.canceled)
        {
            _charging = false;
            _fireRequested = true;
        }
    }

    private void ThrowOne()
    {
        if(GrenadePrefab == null || PlayerCamera == null)
        {
            return;
        }

        //ejswlf dnjswja/qkdgid
        Vector3 origin = Hand!=null?Hand.position : PlayerCamera.transform.position;
        Vector3 dir = PlayerCamera.transform.forward;

        //thrfur rufwjd
        float spd = Mathf.Lerp(BaseThrowSpeed, MaxThrowSpeed, _charge);
        Vector3 vel = dir*spd;

        //rkrthreh(ghlwjs duscnfdyd)
        Vector3 ang = Random.onUnitSphere * AngularSpin;

        //dlstmxjstm todtjd gn Throw gkatn ghcnf
        Grenade g = Instantiate(GrenadePrefab, origin, Quaternion.identity);
        g.Throw(vel, ang);

        //cndwjsrkqt chrlghk
        _charge = 0.0f;
    }
}
