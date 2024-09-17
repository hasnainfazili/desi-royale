using UnityEngine;
using UnityEngine.AI;

public class HostileState : BaseState
{
    public HostileState(Actor _actor, Animator animator) : base(_actor, animator) { }

    public override void OnEnter()
    {
        _animator.CrossFadeInFixedTime("Hostile", 0.1f);
    }
    
    public override void FixedUpdate() { }
    public override void Update() { }
    
    public override void OnExit() { }
}