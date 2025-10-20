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
    public Camera TargetCamera; // 적용할 카메라

    [Header("Effects")]
    public MonoBehaviour[] EffectBehaviours; // ICameraEffect를 구현한 이펙트 스크립트들

    [Header("Master Intensity")]
    [Range(0f, 3f)] public float PositionIntensity = 1.25f; // 위치 오프셋 전체 강도
    [Range(0f, 3f)] public float RotationIntensity = 1.25f; // 회전 오프셋 전체 강도
    [Range(0f, 3f)] public float FovIntensity = 1.10f;      // FOV 오프셋 전체 강도

    private Vector3 _baseLocalPosition;      // 기본 로컬 위치
    private Quaternion _baseLocalRotation;   // 기본 로컬 회전
    private float _baseFov;                  // 기본 FOV

    private ICameraEffect[] _effects; // 적용할 이펙트 목록

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
                // 스크립트를 ICameraEffect로 캐스팅
                ICameraEffect eff = EffectBehaviours[i] as ICameraEffect; // 이펙트 인터페이스 참조
                if (eff != null)
                {
                    _effects[i] = eff;
                }
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 posOffest = Vector3.zero;       // 위치 오프셋 합계
        Vector3 rotEulerOffest = Vector3.zero;  // 회전 오프셋 합계
        float fovOffset = 0f;                   // FOV 오프셋 합계

        if (_effects != null)
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

        // 전체 강도 적용
        posOffest *= PositionIntensity;
        rotEulerOffest *= RotationIntensity;
        fovOffset *= FovIntensity;

        transform.localPosition = _baseLocalPosition + posOffest;

        Quaternion rotOffQuat = Quaternion.Euler(rotEulerOffest); // 오일러 -> 쿼터니언 변환
        transform.localRotation = _baseLocalRotation * rotOffQuat;

        if (TargetCamera != null)
        {
            TargetCamera.fieldOfView = _baseFov + fovOffset;
        }
    }
}
