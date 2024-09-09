using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostileState : IState
{
     [SerializeField] private Actor _actor; 
    public HostileState(Actor _actor)
    {
        this._actor = _actor;
    }

     public void EnterState()
    {
        //Code to run when entering state
        Debug.Log( " has seen or heard the player, actor is now Hostile");

    }

    public void UpdateState()
    {
        //Code to run while in current state
    }

    public void ExitState()
    {
        //Code to run when exiting state
        Debug.Log(" is no longer Hostile");

    }
}
