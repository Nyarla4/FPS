using UnityEngine;

public class GridGizmo : MonoBehaviour
{
    [SerializeField] private int _smallStep = 1;//1m
    [SerializeField] private int _LargeStep = 5;//5m
    [SerializeField] private int _halfExtend = 25;//Radius(m)
    [SerializeField] private Camera _camera;
    private void OnDrawGizmos()
    {
        Vector3 center = Vector3.zero;
        if (_camera != null)
        {
            center = _camera.transform.position;
        }

        int minX = Mathf.FloorToInt(center.x) - _halfExtend;
        int maxX = Mathf.FloorToInt(center.x) + _halfExtend;
        int minZ = Mathf.FloorToInt(center.z) - _halfExtend;
        int maxZ = Mathf.FloorToInt(center.z) + _halfExtend;

        for (int x = minX; x <= maxX; x += _smallStep)
        {
            if (x % _LargeStep == 0)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                Gizmos.DrawLine(new Vector3(x - 0.1f, 0f, minZ), new Vector3(x - 0.1f, 0f, maxZ));
                Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
                Gizmos.DrawLine(new Vector3(x + 0.1f, 0f, minZ), new Vector3(x + 0.1f, 0f, maxZ));
                Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
                Gizmos.DrawLine(new Vector3(x - 0.1f, 0f, minZ), new Vector3(x - 0.1f, 0f, maxZ));
                Gizmos.color = new Color(0f, 0f, 1f, 0.15f);
                Gizmos.DrawLine(new Vector3(x + 0.1f, 0f, minZ), new Vector3(x + 0.1f, 0f, maxZ));
                Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
            }
            Gizmos.DrawLine(new Vector3(x, 0f, minZ), new Vector3(x, 0f, maxZ));
        }
        for (int z = minZ; z <= maxZ; z += _smallStep)
        {
            if (z % _LargeStep == 0)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                Gizmos.DrawLine(new Vector3(minX, 0f, z - 0.1f), new Vector3(maxX, 0f, z - 0.1f));
                Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
                Gizmos.DrawLine(new Vector3(minX, 0f, z + 0.1f), new Vector3(maxX, 0f, z + 0.1f));
                Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
                Gizmos.DrawLine(new Vector3(minX, 0f, z - 0.1f), new Vector3(maxX, 0f, z - 0.1f));
                Gizmos.color = new Color(0f, 0f, 1f, 0.15f);
                Gizmos.DrawLine(new Vector3(minX, 0f, z + 0.1f), new Vector3(maxX, 0f, z + 0.1f));
                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.15f);
            }
            Gizmos.DrawLine(new Vector3(minX, 0f, z), new Vector3(maxX, 0f, z));
        }
    }
}
