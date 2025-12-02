using System.Collections;
using TMPro;
using UnityEngine;

public class TestLobbyUI : MonoBehaviour
{
    [SerializeField] TMP_Text _celebrateText;
    void Start()
    {
        if (SaveSystem.CheckStageAble(4))
        {
            if (_celebrateText != null)
            {
                StartCoroutine(Celebrate());
            }
        }
        else
        {
            _celebrateText.text = "";
            _celebrateText.color = Color.clear;
        }
    }

    IEnumerator Celebrate()
    {
        _celebrateText.color = Color.white;
        var timer = 0.0f;
        while (timer < 3f)
        {
            timer += Time.deltaTime;
            
            Color color = Color.Lerp(Color.white, Color.clear, timer / 3f);

            _celebrateText.color = color;
            yield return null;
        }
        _celebrateText.color = Color.clear;
    }
}
