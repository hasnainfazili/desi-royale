using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : BaseState
{
    public PatrolState(Actor _actor, Animator _animator) : base(_actor, _animator) { }

    public override void OnEnter()
    {
        _animator.CrossFadeInFixedTime("Patrol", 0.1f);
    }

    public override void FixedUpdate()
    {
        _actor.Patrol();
    }
    
    
}