using System;
using UnityEngine;
/// <summary>
/// 레이캐스트 정보를 받아 표면별 임팩트 이펙트/데칼/사운드 발생
/// SurfaceMaterial 재사용
/// </summary>
public class ImpactEffectRouter : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource AudioSource;//재생기(없으면 생략)
    public AudioClip ConcreteClip;
    public AudioClip DirtClip;
    public AudioClip WoodClip;
    public AudioClip MetalClip;
    public AudioClip WaterClip;

    [Header("VFX")]
    public ParticleSystem ConcreteVfx;
    public ParticleSystem DirtVfx;
    public ParticleSystem WoodVfx;
    public ParticleSystem MetalVfx;
    public ParticleSystem WaterVfx;

    [Header("Decals")]
    public DecalPool DecalPool;//데칼 풀(없으면 생략)
    public GameObject ConcreteDecal;
    public GameObject DirtDecal;
    public GameObject WoodDecal;
    public GameObject MetalDecal;
    public GameObject WaterDecal;

    [Header("Options")]
    public float DecalNormalOffset = 0.002f;//표면에서 살짝 띄워서
    public float VfxNormalOffset = 0.01f;//VFX도 표면에서 살짝 띄움
    public bool ParentDecalToHit = true;//움직이는 표면인 경우 부모로 붙이도록

    public void SpawnImpact(RaycastHit hit)
    {
        //표면 타입 확인
        SurfaceType type = SurfaceType.Concrete;
        SurfaceMaterial sm = hit.collider.GetComponent<SurfaceMaterial>();
        if (sm != null)
        {
            type = sm.SurfaceType_;
        }

        if(hit.collider.TryGetComponent<Hitbox>(out var hitbox))
        {

        }
        else
        {
            //오디오 처리
            AudioClip clip = GetClip(type);
            if (AudioSource != null)
            {
                if (clip != null)
                {
                    AudioSource.PlayOneShot(clip, 1.0f);
                }
            }

            //VFX
            ParticleSystem vfx = GetVfx(type);
            if (vfx != null)
            {
                Quaternion rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(hit.normal, Vector3.up), hit.normal);
                Vector3 pos = hit.point + hit.normal * VfxNormalOffset;
                var instVfx = Instantiate(vfx, pos, rot);
                Destroy(instVfx.gameObject, 1.0f);
            }

            //Decal
            GameObject decalPrefab = GetDecal(type);
            if (decalPrefab != null)
            {
                if (DecalPool != null)
                {
                    Quaternion rot = Quaternion.LookRotation(-hit.normal, Vector3.up);
                    Vector3 pos = hit.point + hit.normal * DecalNormalOffset;
                    GameObject decal = DecalPool.Spawn(decalPrefab, pos, rot);
                    if (ParentDecalToHit)
                    {
                        decal.transform.SetParent(hit.collider.transform, true);
                    }
                }
            }
        }
    }

    private AudioClip GetClip(SurfaceType type)
    {
        switch (type)
        {
            case SurfaceType.Concrete:
            default:
                return ConcreteClip;
            case SurfaceType.Dirt:
                return DirtClip;
            case SurfaceType.Wood:
                return WoodClip;
            case SurfaceType.Metal:
                return MetalClip;
            case SurfaceType.Water:
                return WaterClip;
        }
    }

    private ParticleSystem GetVfx(SurfaceType type)
    {
        switch (type)
        {
            case SurfaceType.Concrete:
            default:
                return ConcreteVfx;
            case SurfaceType.Dirt:
                return DirtVfx;
            case SurfaceType.Wood:
                return WoodVfx;
            case SurfaceType.Metal:
                return MetalVfx;
            case SurfaceType.Water:
                return WaterVfx;
        }
    }

    private GameObject GetDecal(SurfaceType type)
    {
        switch (type)
        {
            case SurfaceType.Concrete:
            default:
                return ConcreteDecal;
            case SurfaceType.Dirt:
                return DirtDecal;
            case SurfaceType.Wood:
                return WoodDecal;
            case SurfaceType.Metal:
                return MetalDecal;
            case SurfaceType.Water:
                return WaterDecal;
        }
    }
}
