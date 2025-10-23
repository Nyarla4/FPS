using TMPro;
using UnityEngine;

/// <summary>
/// 탄창 / 예비 로 탄약 표시
/// </summary>
public class AmmoHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;

    private void Awake()
    {
        if(_label == null)
        {
            _label = GetComponent<TextMeshProUGUI>();
        }
    }

    public void SetAmmo(int mag, int reserve)
    {
        if(_label == null)
        {
            return;
        }

        _label.text = $"{mag} / {reserve}";
    }
}
