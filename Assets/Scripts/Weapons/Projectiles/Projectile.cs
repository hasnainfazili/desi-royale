using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float x, y, z;
    [SerializeField] private GameObject hitImpact;
    private void Update()
    {
        transform.Rotate(x, y ,z, Space.Self);
    }
    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(hitImpact, collision.collider.ClosestPoint(transform.position), Quaternion.identity);
    }
}
