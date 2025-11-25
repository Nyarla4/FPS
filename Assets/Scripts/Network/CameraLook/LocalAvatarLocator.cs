using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� ����: ���� Ŭ���̾�Ʈ(�Ǵ� ȣ��Ʈ)�� '�� �ƹ�Ÿ' Transform�� ã�Ƽ� �����ش�.
/// ���� �������� �ƹ�Ÿ �̸��� "Avatar_{id}" ��� ����.
/// �����δ� NetworkRunner�� '�� id'�� ��ȯ�ϴ� getter�� ������ �װ� ���� �� ���� ����.
/// </summary>
public class LocalAvatarLocator : MonoBehaviour
{
    public int myId = -1;                      // ȣ��Ʈ=0, ù Ŭ��=1 ...
    public Transform avatarsRoot;             // �ƹ�Ÿ���� ���ִ� ��Ʈ

    public Transform FindMyAvatar()
    {
        if (avatarsRoot == null)
        {
            return null;
        }

        if (myId < 0)
        {
            myId = NetworkRunner.instance.IsServerRunning() ? 0 : 1;
        }

        string targetName = $"Avatar_{myId}";
        for (int i = 0; i < avatarsRoot.childCount; i = i + 1)
        {
            Transform c = avatarsRoot.GetChild(i);
            if (c == null)
            {
                continue;
            }
            if (c.name == targetName)
            {
                return c;
            }
        }
        return null;
    }
}
