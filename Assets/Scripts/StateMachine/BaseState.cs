using UnityEngine;
using UnityEngine.AI;
public abstract class BaseState : IState 
{  
    protected readonly Actor _actor;
    protected readonly Animator _animator;
    protected BaseState(Actor _actor, Animator _animator)
    {
        this._actor = _actor;
        this._animator = _animator;
    }
    public virtual void OnEnter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void OnExit() { }
    
}

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