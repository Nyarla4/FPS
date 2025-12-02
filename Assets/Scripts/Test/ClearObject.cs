using UnityEngine;

public class ClearObject : MonoBehaviour
{
    [SerializeField] private GameObject _arrow;
    [SerializeField] private int _stage;
    private void Update()
    {
        if(gameObject.activeInHierarchy && _arrow != null)
        {
            var rot = _arrow.transform.rotation.eulerAngles;
            rot.y += Time.deltaTime * 50f;
            _arrow.transform.rotation = Quaternion.Euler(rot);
        }
    }

    public void Clear()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SaveSystem.SaveLastClearStage(_stage);
        SceneChanger.ToLobby();
    }
}
