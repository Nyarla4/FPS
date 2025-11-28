using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SceneChanger;

[RequireComponent(typeof(Button))]
public class StageSelectButton : MonoBehaviour
{
    [SerializeField] private int _currentStage;
    private Button _button;
    [SerializeField] private Scenes _scene;
    [SerializeField] private TMP_Text _stageText;

    void Start()
    {
        _button = GetComponent<Button>();
        _stageText.color = Color.red;
        bool able = SaveSystem.CheckStageCleared(_currentStage);
        _button.interactable = able;
        if (able)
        {
            _button.onClick.AddListener(() => ToScene(_scene));
            _stageText.color = Color.green;
        }
    }

    [ContextMenu("test")]
    void test()
    {
        SaveSystem.SaveLastClearStage(0);
    }
}
