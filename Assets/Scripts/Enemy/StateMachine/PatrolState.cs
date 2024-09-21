using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : BaseState
{
    public PatrolState(Actor _actor, Animator _animator) : base(_actor, _animator) { }

    public override void OnEnter()
    { 
        
    }

    public override void Update()
    {
        _actor.Patrol();
    }

    public override void FixedUpdate()
    {
        
    }

    public override void OnExit()
    {
    }
}