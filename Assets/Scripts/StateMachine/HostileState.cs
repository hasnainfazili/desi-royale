using UnityEngine;
using UnityEngine.AI;

public class HostileState : BaseState
{
    public HostileState(Actor _actor, Animator animator) : base(_actor, animator) { }

    public override void OnEnter()
    {
        Debug.Log(_actor.name + " is hostile");
    }

    public override void FixedUpdate()
    {
        _actor.Hostile();
    }
    public override void Update() { }
    
    public override void OnExit() { }
}