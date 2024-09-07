using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float x, y, z;
    void Update()
    {
        transform.Rotate(x, y ,z, Space.Self);
    }
}
