using UnityEngine;

public class ThrowState : BaseState
{
    public ThrowState(Actor _actor, Animator _animator) : base(_actor, _animator)
    {
        
    }

    public override void OnEnter()
    {
        _actor.InitializeThrow();
    }

    public override void Update()
    {
        //if condition is met 
        
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}