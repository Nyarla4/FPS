using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간단한 오브젝트 풀(데칼/스파크 등)
/// </summary>
public class DecalPool : MonoBehaviour
{
    public int PrewarmCount = 50;//시작시 미리 만들어 둘 개수
    public float AutoReturnAfter = 10.0f;//자동 변환 시간(초)

    private List<GameObject> _pooled = new();//풀(대기중)
    private List<GameObject> _inUse = new();//사용중

    private void Awake()
    {
        //미리 풀링X, 다양한 프리팹 => 런타임 최초 요청 때 생성
    }
    
    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        GameObject go = null;

        //동일 프리팹의 미리 만든 오브젝트가 있는 경우 재사용
        // *단순화로 인한 타입 미검증, 추후 사용할 경우 타입 검증 과정 필요
        if(_pooled.Count > 0)
        {
            go = _pooled[_pooled.Count - 1];
            _pooled.RemoveAt(_pooled.Count - 1);
            go.SetActive(true);
        }
        else
        {
            //없는 경우 새로 생성
            go = Instantiate(prefab, pos, rot);
        }
        
        _inUse.Add(go);
        StartCoroutine(AutoReturn(go, AutoReturnAfter));
        return go;
    }

    public void Despawn(GameObject go)
    {
        if(go == null)
        {
            return;
        }

        bool removed = _inUse.Remove(go);
        if (removed)
        {
            go.SetActive(false);
            go.transform.SetParent(transform, true);
            _pooled.Add(go);
        }
    }

    IEnumerator AutoReturn(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        Despawn(go);
    }
}
