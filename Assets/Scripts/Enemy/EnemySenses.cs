using System;
using UnityEditor.Rendering;
using UnityEngine;

/// <summary>
/// wjr tldi rPtks ahebf
/// rjfl: ViewDistnace alaks
/// tldirkr: ViewAngle/2 qksrkr dlso
/// rkfla: Eye dnlcldptj xkrpt qkdgiddmfh fpdlzotmxm -> cjt glxmrk xkrptdls ruddn qhdlsek
/// </summary>
public class EnemySenses : MonoBehaviour
{
    [Header("Vision")]
    public Transform Eye;//tldi tlwkrwja(ajflsk snsdml dnlcl)
    public float ViewDistance = 18.0f;//tldi rjfl(m)
    public float ViewAngle = 110.0f;//wjscp tldirkr. qksrkr = ViewAngle * 0.5f
    public LayerMask VisionMask;//fpdlzotmxm cndehf aktmzm(qur, qkekr, vmffpdldj emd)
    public Transform Target;//rkawl eotkd(vmffpdldj fnxm Transform)

    [Header("Debug")]
    public bool DrawDebug = false;//elqjrm tjs rmflrl xhrmf

    void Update()
    {

    }

    /// <summary>
    /// xkrptdl guswo qhdlsmswl rPtks
    /// qhdlsms ruddn true qksghks gn lastKnownPosdp guswo xkrpt dnlcl qksghks
    /// </summary>
    /// <param name="last"></param>
    /// <returns></returns>
    public bool CanSeeTarget(out Vector3 lastKnownPos)
    {
        //qksghksrkqt chrlghk
        lastKnownPos = Vector3.zero;

        //vlftn ckawh ghkrdls
        if (Eye == null || Target == null)
        {
            return false;
        }

        //rjfl vkswjd
        Vector3 toTarget = Target.position - Eye.position;//xkrptRkwl qprxj(dnjfem rlwns)
        float dist = toTarget.magnitude;//rjfl(m)
        if (dist > ViewDistance)
        {//tldi rjfl qkRdls ruddn
            return false;
        }

        //tldirkr vkswjd(ehxm vmfhejrxm = cos(rkr))
        Vector3 forward = Eye.forward;//sns wjsqkd
        Vector3 dir = toTarget.normalized;//xkrpt qkdgiddml eksdnl qprxj
        float dot = Vector3.Dot(forward, dir);//cos(theta): qprxjdml sowjr(en qprxj tkdldml rkreh)
        float halfRad = (ViewAngle * 0.5f) * Mathf.Deg2Rad;//qksrkr fkeldks
        float cosHalf = Mathf.Cos(halfRad);//dlarPcl: cos(qksrkr)

        //dot < cos(qksrkr)dlaus tldi qkR
        if (dot < cosHalf)
        {
            return false;
        }

        //rkfuwuTsmswl cpzm(fpdlzotmxmfh cjt cndehfcp ghkrdls)
        Ray ray = new Ray(Eye.position, dir);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, ViewAngle, VisionMask, QueryTriggerInteraction.Ignore))
        {
            //cjt glxmdml fnxmrk xkrptdml fnxmdls ruddn rkfuwlwl dksgdma
            Transform h = hit.collider.transform;
            if (IsSameRoot(h, Target))
            {
                lastKnownPos = Target.position;

                if (DrawDebug)
                {
                    Debug.DrawLine(Eye.position, hit.point, new Color(0.5f, 1.0f, 0.5f), 0.1f);
                }

                return true;
            }
            else
            {
                if (DrawDebug)
                {
                    Debug.DrawLine(Eye.position, hit.point, new Color(1.0f, 0.5f, 0.5f), 0.1f);
                }

                return false;
            }
        }

        //fpdldp dkanrjteh djqtdmaus qhdlwl dksgdma
        return false;
    }

    /// <summary>
    /// en Transformdml chltkddnl fnxmrk ehddlfgkswl vkswjd
    /// *zoflrxj qhrgkq zhffkdlej rnwhdptj rkxdms eotkddlswl vkseksgkrl dnlgka
    /// </summary>
    private bool IsSameRoot(Transform a, Transform b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        return a.root == b.root;
    }
}
