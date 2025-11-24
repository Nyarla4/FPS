using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 월드 공간 HP바
///     Damageable의 HP를 Slider로 표시
///     카메라방향 회전(빌보드)
/// </summary>
public class WorldHealthBar : MonoBehaviour
{
    public Damageable Target;//HP를 보여줄 대상의 Damageable
    public Slider Slider;//슬라이더UI
    public Transform Billboard;//빌보드 루트(HP바 루트의 Transform)
    public Camera ViewCamera;//바라볼 카메라(없으면 메인)

    void Start()
    {
        if(ViewCamera == null)
        {
            ViewCamera = Camera.main;
        }
    }

    void Update()
    {
        if (Target != null)
        {
            if (Target.MaxHp > 0)
            {
                float ratio = (float)Target.CurHp / (float)Target.MaxHp;//체력 비율 계산
                if (Slider != null)
                {
                    Slider.value = ratio;
                }
            }
        }

        //카메라 바라보도록(단순 빌보드)
        if(Billboard != null && ViewCamera != null)
        {
            Vector3 dir = Billboard.position - ViewCamera.transform.position;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(dir);
                Billboard.rotation = look;
            }
        }
    }
}
