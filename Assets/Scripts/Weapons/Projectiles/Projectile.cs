
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private GameObject hitImpact;
  
    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(hitImpact, collision.GetContact(0).point, Quaternion.identity);
    }
}
