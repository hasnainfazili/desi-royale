using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health 
{
    [SerializeField] private float maxHealth;
    public float currentHealth {get; private set;}
}