using UnityEngine;

public class Magazine : MonoBehaviour
{
    [SerializeField] int _ammos;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<TestWeaponControl>(out var player))
        {
            player.GetMagazine(_ammos);
            player.UpdateHud();
            Destroy(gameObject);
        }
    }
}
