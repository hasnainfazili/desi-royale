using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : IState
{
     [SerializeField] private Actor _actor; 
    public DeathState(Actor _actor)
    {
        this._actor = _actor;
    }

     public void EnterState()
    {
        //Code to run when entering state
        Debug.Log(" is now dead");

    }

    public void UpdateState()
    {
        //Code to run while in current state
    }

    public void ExitState()
    {
        //Code to run when exiting state
        Debug.Log(" GameObject has been disabled");

    }
}
