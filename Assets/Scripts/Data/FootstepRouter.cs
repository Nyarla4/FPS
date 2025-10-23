using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StepEventSource로부터 특정 이벤트를 받아
/// 발소리 > 오디오 재생/VFX > 카메라 진동 > 임펄스 실행
/// 역할 수행
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
            // 표면을 찾지 못했을 경우 기본값으로 사운드를 재생
            type = SurfaceType.Concrete;
            point = transform.position;
            normal = Vector3.up;
        }

        // 사운드 재생, 파티클, 카메라 진동
        PlayFootstepAudio(type);
        SpawnFootstepVfx(type, point, normal);
        PulseCameraImpulse();
    }

    private void PlayFootstepAudio(SurfaceType type)
    {
        Debug.Log($"{type} ground audio");

        if (AudioSource == null)
        {
            return;
        }

        List<AudioClip> bank = GetClipBank(type);
        if (bank == null)
        {
            return;
        }
        if (bank.Count == 0)
        {
            return;
        }

        // 무작위 1곡, 랜덤 볼륨과 피치 설정
        int idx = Random.Range(0, bank.Count);
        AudioClip clip = bank[idx];

        float vol = Random.Range(VolumeMin, VolumeMax);
        float pit = Random.Range(PitchMin, PitchMax);

        AudioSource.pitch = pit;
        AudioSource.PlayOneShot(clip, vol);
    }

    // 시각효과: 발소리 위치에 따라 VFX 생성
    private void SpawnFootstepVfx(SurfaceType type, Vector3 point, Vector3 normal)
    {
        //Debug.Log($"{type} ground vfx");

        ParticleSystem prefab = GetVfxPrefab(type);
        if (prefab == null)
        {
            return;
        }

        Vector3 spawnPos = point + normal * VfxUpOffset;
        Quaternion spawnRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(normal, Vector3.up), normal);

        if (ParentToWorld)
        {
            ParticleSystem vfx = Instantiate(prefab, spawnPos, spawnRot);
            if (vfx != null)
            {
                // 일정 시간 후 자동 파괴로 메모리 관리
                Destroy(vfx.gameObject, 0.5f);
            }
        }
        else
        {
            ParticleSystem vfx = Instantiate(prefab, spawnPos, spawnRot, null);
            if (vfx != null)
            {
                Destroy(vfx.gameObject, 0.5f);
            }
        }
    }

    private void PulseCameraImpulse()
    {
        if (CameraImpulse != null)
        {
            CameraImpulse.Pulse();
        }
    }

    private List<AudioClip> GetClipBank(SurfaceType type)
    {
        switch (type)
        {
            case SurfaceType.Concrete:
                return ConcreteClips;
            case SurfaceType.Dirt:
                return DirtClips;
            case SurfaceType.Wood:
                return WoodClips;
            case SurfaceType.Metal:
                return MetalClips;
            case SurfaceType.Water:
                return WaterClips;
            default:
                return ConcreteClips;
        }
    }

    private ParticleSystem GetVfxPrefab(SurfaceType type)
    {
        switch (type)
        {
            case SurfaceType.Concrete:
                return ConcreteVfxPrefab;
            case SurfaceType.Dirt:
                return DirtVfxPrefab;
            case SurfaceType.Wood:
                return WoodVfxPrefab;
            case SurfaceType.Metal:
                return MetalVfxPrefab;
            case SurfaceType.Water:
                return WaterVfxPrefab;
            default:
                return ConcreteVfxPrefab;
        }
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
