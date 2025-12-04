using UnityEngine;

public class TestDeadUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    
    void Start()
    {
        ClosePanel();
    }

    public void OpenPanel()
    {
        _panel.SetActive(true);
    }

    public void ClosePanel()
    {
        _panel.SetActive(false);
    }

    public void Retry()
    {
        Time.timeScale = 1;
        SceneChanger.ToThisScene();
    }

    public void Exit()
    {
        Time.timeScale = 1;
        SceneChanger.ToLobby();
    }
}
