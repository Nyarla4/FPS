using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;

public interface ICameraEffect
{
    Vector3 CurrentPositionOffset { get; }
    Vector3 CurrentRotationOffestEuler { get; }
    float CurrentFovOffsets { get; }
}
public class CameraEffectsMixer : MonoBehaviour
{
    [Header("Target")]
    public Camera TargetCamera;//rufrhk wjrdyd zkapfk

    [Header("Effects")]
    public MonoBehaviour[] EffectBehaviours;//ICameraEffect rngus zjavhsjsxmemf

    [Header("Master Intensity")]
    [Range(0f, 3f)] public float PositionIntensity = 1.25f;//dnlcl dhvmtpt wjscp qodbf
    [Range(0f, 3f)] public float RotationIntensity = 1.25f;//ghlwjs dhvmtpt wjscp qodbf
    [Range(0f, 3f)] public float FovIntensity = 1.10f;//FOV dhvmtpt wjscp qodbf

    private Vector3 _baseLocalPosition;      //rlwns fhzjf dnlcl
    private Quaternion _baseLocalRotation;   //rlwns fhzjf ghlwjs
    private float _baseFov;                  //rlwns FOV

    private ICameraEffect[] _effects;//zotmxldehls gyrhk ahrfhr

    private void Awake()
    {
        if (TargetCamera == null)
        {
            TargetCamera = GetComponent<Camera>();
        }

        if (TargetCamera == null)
        {
            Debug.LogError("[CameraEffectsMixer] 카메라 누락");
        }

        _baseLocalPosition = transform.localPosition;
        _baseLocalRotation = transform.localRotation;

        if (TargetCamera != null)
        {
            _baseFov = TargetCamera.fieldOfView;
        }

        if (EffectBehaviours != null)
        {
            _effects = new ICameraEffect[EffectBehaviours.Length];

            for (int i = 0; i < EffectBehaviours.Length; i++)
            {
                //tkdthrdmf dldydgks gudqusghks
                ICameraEffect eff = EffectBehaviours[i] as ICameraEffect;//gyrhk zotmxld rufrhk
                if (eff != null)
                {
                    _effects[i] = eff;
                }
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 posOffest = Vector3.zero;       //dnlcl dhvmtpt snwjr
        Vector3 rotEulerOffest = Vector3.zero;  //ghlwjs dhvmtpt snwjr
        float fovOffset = 0f;                   //FOV dhvmtpt snwjr

        if (_effects!=null)
        {
            for (int i = 0; i < _effects.Length; i++)
            {
                var eff = _effects[i];
                if (eff != null)
                {
                    posOffest += eff.CurrentPositionOffset;
                    rotEulerOffest += eff.CurrentRotationOffestEuler;
                    fovOffset += eff.CurrentFovOffsets;
                }
            }
        }

        //wjsdur rkdeh wjrdyd
        posOffest *= PositionIntensity;
        rotEulerOffest *= RotationIntensity;
        fovOffset *= FovIntensity;

        transform.localPosition = _baseLocalPosition + posOffest;
        
        Quaternion rotOffQuat = Quaternion.Euler(rotEulerOffest);//dhdlffj => znjxjsldjs
        transform.localRotation = _baseLocalRotation * rotOffQuat;

        if(TargetCamera != null)
        {
            TargetCamera.fieldOfView = _baseFov + fovOffset;
        }
    }
}
