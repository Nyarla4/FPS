using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �κ񿡼� Start ��ȣ ������ GameSceneName ������ ��ȯ
/// </summary>
public class SceneFlow : MonoBehaviour
{
    public string GameSceneName = "Main";

    private void OnEnable()
    {
        if(NetworkRunner.instance!= null)
        {
            NetworkRunner.instance.OnStartSignal += OnStartSignal;
        }
    }
    private void OnDisable()
    {
        if(NetworkRunner.instance!= null)
        {
            NetworkRunner.instance.OnStartSignal -= OnStartSignal;
        }
    }

    private void OnStartSignal()
    {
        //Start ��ȣ ���� �� �ش� ������ �ε�
        SceneManager.LoadScene(GameSceneName);
    }
}
