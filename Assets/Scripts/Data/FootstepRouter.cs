using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StepEventSource로부터 특정 이벤트를 받아
/// 발소리 > 오디오 재생/VFX > 카메라 진동 > 임펄스 실행
/// 까지 처리
/// </summary>
[DisallowMultipleComponent]
public class FootstepRouter : MonoBehaviour
{
    [Header("Refs")]
    public FootstepSurfaceDetector SurfaceDetector; // 발밑 표면 감지기
    public AudioSource AudioSource; // 오디오(발소리용 3D 사운드)
    public SimpleCameraImpulse CameraImpulse; // 카메라 임펄스(진동)

    [Header("Audio Library")]
    public List<AudioClip> ConcreteClips = new();
    public List<AudioClip> DirtClips = new();
    public List<AudioClip> WoodClips = new();
    public List<AudioClip> MetalClips = new();
    public List<AudioClip> WaterClips = new();
    public float VolumeMin = 0.8f;
    public float VolumeMax = 1.0f;
    public float PitchMin = 0.95f;
    public float PitchMax = 1.05f;

    [Header("VFX")]
    public ParticleSystem ConcreteVfxPrefab;
    public ParticleSystem DirtVfxPrefab;
    public ParticleSystem WoodVfxPrefab;
    public ParticleSystem MetalVfxPrefab;
    public ParticleSystem WaterVfxPrefab;

    [Header("VFX Options")]
    public float VfxUpOffset = 0.05f; // 발소리 시 약간의 위치 오프셋
    public bool ParentToWorld = true; // 부모 오브젝트(월드) 기준

    // 외부 호출: 특정 이벤트 함수
    public void OnStepLeft()
    {
        HandleStep();
    }
    public void OnStepRight()
    {
        HandleStep();
    }

    // 내부 처리
    private void HandleStep()
    {
        if (SurfaceDetector == null)
        {
            return;
        }

        SurfaceType type;
        Vector3 point;
        Vector3 normal;

        bool ok = SurfaceDetector.TryGetSurface(out type, out point, out normal);
        if (!ok)
        {
