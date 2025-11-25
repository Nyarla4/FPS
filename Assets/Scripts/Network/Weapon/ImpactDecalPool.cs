using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데칼 풀
///     *탄흔으로 사용
///     Ray hit 지점에 Quad 배치, 표면 법선 방향으로 회전
///     너무 많은 생성 방지를 위해 풀에서 재사용 처리
/// </summary>
public class ImpactDecalPool : MonoBehaviour
{
    public GameObject DecalPrefab;//Quad+머티리얼 프리팹
    public int PoolSize = 32;//풀 크기(최대갯수)
    public float DecalLife = 10.0f;//수명(초)
    public float ZOffset = 0.01f;//깊이 겹침 방지용으로 띄울 거리

    private Queue<GameObject> _pool;//비활성 큐
    private List<GameObject> _actives;//활성 큐

    private void Awake()
    {
        _pool = new(PoolSize);
        _actives = new(PoolSize);

        for (int i = 0; i < PoolSize; i++)
        {
            if (DecalPrefab == null)
            {
                break;
            }

            GameObject go = Instantiate(DecalPrefab);
            go.SetActive(false);
            _pool.Enqueue(go);
        }
    }

    public void SpawnDecal(Vector3 position, Vector3 normal, Transform parent)
    {
        GameObject go = null;
        if (_pool.Count > 0)
        {
            go = _pool.Dequeue();
            if (go != null)
            {
                //명중된 곳의 자식으로 지정(캐릭터 따라 움직이는 현상 방지)
                go.transform.SetParent(parent);
            }
        }
        else
        {
            //풀을 다 썼으면 신규 작성
            if (DecalPrefab != null)
            {
                go = Instantiate(DecalPrefab);
                if (go != null)
                {
                    go.transform.SetParent(parent);
                }
            }
        }

        if (go == null)
        {
            return;
        }

        //표면 살짝 띄우기(깊이 겹침 방지)
        Vector3 pos = position + normal * ZOffset;//표면에서 ZOffset만큼 뜨도록

        //법선에 정렬(법선이 forward가 되도록 회전)
        Quaternion rot = Quaternion.LookRotation(normal);

        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);

        //수명 타이머 코루틴
        StartCoroutine(DespawnAfter(go, DecalLife));
        _actives.Add(go);
    }

    private IEnumerator DespawnAfter(GameObject go, float seconds)
    {
        var dmg = go.GetComponentInParent<Damageable>();

        float end = Time.time + seconds;//Despawns 시각

        while (Time.time < end)
        {
            if (dmg.CurHp <= 0)
            {//죽으면 즉시 제거
                break;
            }
            yield return null;
        }
        if (go != null)
        {
            go.transform.SetParent(null);
            go.SetActive(false);
            _pool.Enqueue(go);
            _actives.Remove(go);
        }
    }
}
