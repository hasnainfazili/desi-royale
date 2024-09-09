using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IState
{
    [SerializeField] private Actor _actor; 
//    [SerializeField] private Transform Player;

//      // Attack Projectile prefab
//    [SerializeField] private GameObject SlipperGameObject;
//     // Attack Projectile Transform position
//    [SerializeField] private Transform projectileSpawnPoint;

//     // Actor Rotation Speed
//    [SerializeField] private float rotationSpeed;
//     // Actor's Time till next attack
//    [SerializeField] private float timeToAttack;
//     // Can Actor Attack?
//    [SerializeField] private bool CanAttack;
//     //The Force of the projectile being launched.
//    [SerializeField] private float ProjectileForce;
    public AttackState(Actor _actor)
    {
        this._actor = _actor;
    }

    public void EnterState()
    {
        //Code to run when entering state
        Debug.Log(" is now Attacking");
        // CanAttack = true;


    }

    public void UpdateState()
    {
        //Code to run while in current state
        // Vector3 LookAtPlayerTransform = new Vector3(Player.position.x, transform.position.y, Player.position.z);    

        // transform.LookAt(Vector3.Lerp(transform.position, LookAtPlayerTransform, Time.deltaTime * rotationSpeed)) ;

        // if(CanAttack)
        // {
        //     ThrowSlipper();
        //     CanAttack = false;
        //     // StartCoroutine(ResetTimeToAttack());
        // }
    }
//    private void ThrowSlipper()
//     {
//         // GameObject InstantiatedSlipper = Instantiate(SlipperGameObject, projectileSpawnPoint.position, Quaternion.identity);
//         // InstantiatedSlipper.TryGetComponent(out Rigidbody slipperRigidbody);
//         // slipperRigidbody.AddForce(transform.forward * ProjectileForce, ForceMode.Impulse);
//     }
//     private IEnumerator ResetTimeToAttack()
//     {
//         yield return new WaitForSeconds(timeToAttack);
//         CanAttack = true;
//     }
    
    public void ExitState()
    {
        //Code to run when exiting state
        Debug.Log("No longer Attacking");

    }
}
