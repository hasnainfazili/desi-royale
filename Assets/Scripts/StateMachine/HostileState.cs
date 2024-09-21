using UnityEngine;

public class HostileState : BaseState
{
    public HostileState(Actor _actor, Animator animator) : base(_actor, animator) { }

    public override void OnEnter()
    {
        
    }

    public override void FixedUpdate()
    {
        Debug.Log("Hostile");
    }

    public override void Update()
    {
        

    }

    public override void OnExit()
    {
        
    }
    
    
}