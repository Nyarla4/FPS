using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneChanger
{
    public enum Scenes
    {
        LobbyScene,
        Stage1Scene,
        Stage2Scene,
        Stage3Scene,
    }

    public static void ToScene(Scenes scenes)
    {
        string sceneName = scenes.ToString();
        var sceneIdx = SceneUtility.GetBuildIndexByScenePath(sceneName);

        if (sceneIdx < 0)
        {
            Debug.Log("해당 Scene 없음");
            return;
        }

        SceneManager.LoadScene(sceneIdx);
    }

    public static void ToThisScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void ToLobby()
    {
        ToScene(Scenes.LobbyScene);
    }

    public static void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
