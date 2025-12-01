using UnityEngine;

public class Magazine : MonoBehaviour
{
    [SerializeField] int _ammos;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<TestPlayerControl>(out var player))
        {
            player.GetMagazine(_ammos);
            Destroy(gameObject);
        }
    }
}
