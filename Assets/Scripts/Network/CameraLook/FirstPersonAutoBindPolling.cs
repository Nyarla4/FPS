using System.Collections;
using UnityEngine;

/// <summary>
/// ���� �ð� ���� �ֱ������� �ƹ�Ÿ�� ã�ٰ� �߰� ��� ���ε�.
/// </summary>
[DisallowMultipleComponent]
public class FirstPersonAutoBindPolling : MonoBehaviour
{
    public FirstPersonCameraRig rig;
    public LocalAvatarLocator locator;
    public float timeoutSeconds = 5.0f;    // �ִ� ��� �ð�
    public float intervalSeconds = 0.2f;   // ��õ� ����

    private void Start()
    {
        StartCoroutine(CoBind());
    }

    private IEnumerator CoBind()
    {
        float elapsed = 0.0f;

        while (elapsed < timeoutSeconds)
        {
            if (rig == null)
            {
                rig = GetComponent<FirstPersonCameraRig>();
            }
            if (locator == null)
            {
                locator = GetComponent<LocalAvatarLocator>();
            }
            if (rig != null && locator != null)
            {
                Transform me = locator.FindMyAvatar();
                if (me != null)
                {
                    rig.target = me;
                    yield break;
                }
            }

            yield return new WaitForSeconds(intervalSeconds);
            elapsed = elapsed + intervalSeconds;
        }

        Debug.LogWarning("FirstPersonAutoBindPolling: Ÿ�Ӿƿ����� ���ε� ����.");
    }
}
