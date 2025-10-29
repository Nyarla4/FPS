using System.Collections.Generic;
using UnityEngine;

public class StatusEffectHUD : MonoBehaviour
{
    [Header("Refs")]
    public StatusEffectHost Host;           // 대상 호스트.
    public RectTransform Content;           // 슬롯들이 배치될 부모(수평 레이아웃 권장)
    public GameObject SlotPrefab;           // 슬롯 프리팹(이미지+텍스트)

    private readonly List<GameObject> _slots = new List<GameObject>(); // 생성된 슬롯.

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
