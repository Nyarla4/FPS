using UnityEngine;

/// <summary>
/// 맞은 지점에 파티클 재생
///     ParticleSystem 프리팹을 생성 후 위치/회전 세팅하고 Play
///     Destroy로 수명 관리
/// </summary>
public class HitSparkFX : MonoBehaviour
{
    public ParticleSystem SparkPrefab; //스파크/먼지 프리팹
    
    public void PlayAt(Vector3 position, Vector3 normal, Transform parent)
    {
        if(SparkPrefab == null)
        {
            return;
        }

        //법선 방향을 바라보도록 회전
        Quaternion rot = Quaternion.LookRotation(normal);

        ParticleSystem particle = Instantiate(SparkPrefab, position, rot, parent);
        particle.Play();
        
        Destroy(particle.gameObject, 1.0f);//1.0f 대신 particle.main.duration + particle.main.startLifetime.constantMax 도 선택 가능
    }
}
