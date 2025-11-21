using UnityEngine;

/// <summary>
/// 뷰모델 총구에서 짧은 빛/파티클 재생
///     라이트 토글
///     파티클 시스템 플레이
/// </summary>
public class MuzzleFlash : MonoBehaviour
{
    public Light FlashLight;//총구 플래시로 쓸 라이트(선택), pointLight 등
    public ParticleSystem Particles;//파티클(선택)
    public float LightOnDuration = 0.03f;//라이트 켜둘 시간(초
    
    private float _lightOffTime;//라이트 끌 시간(초

    void Update()
    {
        if (FlashLight != null)
        {
            if (FlashLight.enabled && Time.time > _lightOffTime)
            {
                FlashLight.enabled = false;
            }
        }
    }

    public void PlayOnce()
    {
        if (FlashLight != null)
        {
            FlashLight.enabled = true;
            _lightOffTime = Time.time + LightOnDuration;
        }

        if(Particles != null)
        {
            Particles.Play();
        }
    }
}
