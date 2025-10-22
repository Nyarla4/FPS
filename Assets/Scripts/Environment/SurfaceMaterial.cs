using UnityEngine;

public enum SurfaceType
{
    Concrete = 0,
    Dirt = 1,
    Wood = 2,
    Metal = 3,
    Water = 4
}
/// <summary>
/// 표면/재질 및 이펙트와 관련된 처리를 담당하는 컴포넌트
/// </summary>
[DisallowMultipleComponent]
public class SurfaceMaterial : MonoBehaviour
{
    public SurfaceType surfaceType = SurfaceType.Concrete;//표면재질의 기본 값
}
