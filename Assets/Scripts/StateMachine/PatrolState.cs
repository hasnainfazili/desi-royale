using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : BaseState
{
    public PatrolState(Actor _actor, Animator _animator) : base(_actor, _animator) { }

    public override void OnEnter()
    {
        Debug.Log(_actor.name + " is patrolling");
    }

    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        _actor.Patrol();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}