using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Ŭ���̾�Ʈ���� ���� STATE �޽����� �޾Ƽ� ĳ���Ϳ� ����
///     ����/���� �����Ƿ� ���� �״�� ó��
/// ĳ���� ���� Ȥ�� ��ġ/ȸ�� ���� ���� ó��
/// </summary>
public class ClientGame : MonoBehaviour
{
    [Header("Avatar")]
    public Transform AvatarsRoot;          // ĳ���� �θ�(������ �ڵ� ����)
    public GameObject AvatarPrefab;        // �ܼ� ĸ�� ������(��Ƽ���� ���� �޶� OK)

    // id �� Transform
    private Dictionary<int, Transform> _avatars;

    private void Awake()
    {
        _avatars = new Dictionary<int, Transform>();
    }

    public void InitAvatarRoot()
    {
        if (AvatarsRoot == null)
        {
            GameObject go = new GameObject("AvatarsRoot");
            AvatarsRoot = go.transform;
        }
    }

    private void OnEnable()
    {
        if (NetworkRunner.instance != null)
        {
            NetworkRunner.instance.OnClientCommand += OnClientCommand;
        }
    }

    private void OnDisable()
    {
        if (NetworkRunner.instance != null)
        {
            NetworkRunner.instance.OnClientCommand -= OnClientCommand;
        }
    }

    private void OnClientCommand(string cmd, string payload)
    {
        if (cmd == "STATE")
        {
            ApplyStateJson(payload);
        }
    }

    private void ApplyStateJson(string json)
    {
        // ���� �ܼ� �Ľ�: ���Խ����� players �迭���� id, x,y,z, yaw�� �̴´�.
        // (���������� JSON �ļ� ��� ����. �ʺ� ���Ƕ� ������ ���̱� ���� ���� �Ľ�)
        // ���� ��: {"id":1,"x":0.0,"y":1.8,"z":2.0,"yaw":90,"hp":100}
        Regex item = new Regex("\\{\"id\": (\\d+), \"x\": ([-0-9\\.]+), \"y\": ([-0-9\\.]+), \"z\": ([-0-9\\.]+), \"yaw\": ([-0-9\\.]+), \"hp\": (\\d+)\\}",
                               RegexOptions.Compiled);

        MatchCollection matches = item.Matches(json);
        if (matches == null)
        {
            return;
        }

        for (int i = 0; i < matches.Count; i = i + 1)
        {
            GroupCollection g = matches[i].Groups;

            int id = 0;
            float x = 0.0f;
            float y = 0.0f;
            float z = 0.0f;
            float yaw = 0.0f;

            int.TryParse(g[1].Value, out id);
            float.TryParse(g[2].Value, out x);
            float.TryParse(g[3].Value, out y);
            float.TryParse(g[4].Value, out z);
            float.TryParse(g[5].Value, out yaw);

            Transform t = GetOrCreateAvatar(id);
            if (t == null)
            {
                continue;
            }

            // ����(���� ����)
            t.position = new Vector3(x, y, z);
            t.rotation = Quaternion.Euler(0.0f, yaw, 0.0f);
        }
    }

    private Transform GetOrCreateAvatar(int id)
    {
        if (_avatars.ContainsKey(id) == true)
        {
            return _avatars[id];
        }

        //�������� ����Ǿ����� ���� ��� �˾Ƽ� ���� ó��
        if (AvatarPrefab == null)
        {
            // ĸ�� �⺻ ����
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = $"Avatar_{id}";
            capsule.transform.SetParent(AvatarsRoot, false);
            _avatars.Add(id, capsule.transform);
            return capsule.transform;
        }
        else
        {
            GameObject go = GameObject.Instantiate(AvatarPrefab, AvatarsRoot);
            go.name = $"Avatar_{id}";
            _avatars.Add(id, go.transform);
            return go.transform;
        }
    }
}
