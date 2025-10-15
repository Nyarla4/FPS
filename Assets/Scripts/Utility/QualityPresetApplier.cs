using UnityEngine;

public class QualityPresetApplier : MonoBehaviour
{
    [SerializeField] private bool _showToast = true; //show console

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ApplyQuality(0);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ApplyQuality(1);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ApplyQuality(2);
        }
    }

    private void ApplyQuality(int index)
    {
        index = Mathf.Clamp(index, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(index, true);
        if (_showToast)
        {
            Debug.Log($"[Quality] Set to {QualitySettings.names[index]}");
        }
    }
}
