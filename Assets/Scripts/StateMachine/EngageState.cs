
using UnityEngine;

public class EngageState : BaseState
{
    protected readonly Actor _actor;
    protected readonly Animator _animator;
    public EngageState(Actor _actor, Animator _animator) : base(_actor, _animator)
    {
    }

    public override void OnEnter()
    {
        Debug.Log("Throwing Initialized");
        _actor.InitializeThrow();
    }

    public override void FixedUpdate()
    {
        _actor.Engaged();
    }

    public override void Update()
    {
        base.Update();
    }


    public override void OnExit()
    {
        base.OnExit();
    }
    
    
}