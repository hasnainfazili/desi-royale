using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    [Header("Actor Information")]
    // Current State of Actor
   [SerializeField] private ActorState activeState;
    // Type of Actor Attached to.
   [SerializeField] private ActorType currentActor;
    // Player Transform for testing!
   [SerializeField] private Transform Player;

    [Header("Actor Attack State References")]
    // Attack Projectile prefab
   [SerializeField] private GameObject SlipperGameObject;
    // Attack Projectile Transform position
   [SerializeField] private Transform projectileSpawnPoint;

    // Actor Rotation Speed
   [SerializeField] private float rotationSpeed;
    // Actor's Time till next attack
   [SerializeField] private float timeToAttack;
    // Can Actor Attack?
   [SerializeField] private bool CanAttack;
    //The Force of the projectile being launched.
   [SerializeField] private float ProjectileForce;
    
    private void Start()
    {
        activeState = ActorState.Hostile;
        CanAttack = true;
    }
    private void Update()
    {
        Vector3 LookAtPlayerTransform = new Vector3(Player.position.x, transform.position.y, Player.position.z);    

        transform.LookAt(Vector3.Lerp(transform.position, LookAtPlayerTransform, Time.deltaTime * rotationSpeed)) ;

        if(CanAttack)
        {
            ThrowSlipper();
            CanAttack = false;
            StartCoroutine(ResetTimeToAttack());
        }
    }
    private void ThrowSlipper()
    {


        GameObject InstantiatedSlipper = Instantiate(SlipperGameObject, projectileSpawnPoint.position, Quaternion.identity);
        InstantiatedSlipper.TryGetComponent(out Rigidbody slipperRigidbody);
        slipperRigidbody.AddForce(transform.forward * ProjectileForce, ForceMode.Impulse);
    }
    private IEnumerator ResetTimeToAttack()
    {
        yield return new WaitForSeconds(timeToAttack);
        CanAttack = true;
    }
}
