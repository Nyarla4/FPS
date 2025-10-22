using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

/// <summary>
/// StepEventSourcerk sosms tmxpq dlqpsxmfmf qkedktj
/// vyausxkawl > dheldh tjsxor/wotod > vkxlzmf tmvhs > zkapfk dlavjftm ghcnf
/// Rkwl tngod
/// </summary>
[DisallowMultipleComponent]
public class FootstepRouter : MonoBehaviour
{
    [Header("Refs")]
    public FootstepSurfaceDetector SurfaceDetector;//qkfalx vyaus vkseks
    public AudioSource AudioSource;//wotodrl(vmffpdldjdjp qnxdlrjsk 3D dheldh)
    public SimpleCameraImpulse CameraImpulse;//zkapfk gmsemfrl(tjsxor)

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
    public float VfxUpOffset = 0.05f;//wlausdptj tkfWkr dnlfh
    public bool ParentToWorld = true;//qnah djqtdl(dnjfemfh) tmvhs

    //dhlqn dusruf tmxpq dlqpsxm wlsdlqwja
    public void OnStepLeft()
    {
        HandleStep();
    }
    public void OnStepRight()
    {
        HandleStep();
    }

    //soqn gkatn
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
            //vyausdmf ckwwl ahtgks ruddn dheldhsms rlqhsrkqtdmfh cjfl
            type = SurfaceType.Concrete;
            point = transform.position;
            normal = Vector3.up;
        }

        //tnstjeofh tkdnsem, qkfwkrnr, zkapfk cjfl
        PlayFootstepAudio(type);
        SpawnFootstepVfx(type,point, normal);
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

        //foseja 1xor, qhffba alc vlcl foseja
        int idx = Random.Range(0, bank.Count);
        AudioClip clip = bank[idx];

        float vol = Random.Range(VolumeMin, VolumeMax);
        float pit = Random.Range(PitchMin, PitchMax);

        AudioSource.pitch = pit;
        AudioSource.PlayOneShot(clip, vol);
    }

    //dlvprxm: skqheksms tkdeoqkddl qhrp ehlf rjt
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
            if(vfx != null)
            {
                //ghrdms vnfflddlsk wkehdvkrhlfmf vmflvoqdmfh tjfwjdgotj cjflgoeh ehla
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
        if(CameraImpulse != null)
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
