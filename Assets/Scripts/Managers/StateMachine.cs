using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StateMachine
{
    public IState currentState { get; private set;}

    //References to The States present
    public HostileState hostileState;
    public PatrolState patrolState;
    public AttackState attackState;
    public DeathState deathState;


    public event Action<IState> onActorStateChanged;
    
    public StateMachine(Actor _actor)
    {
        //Set the instances for each of the actors state
        this.hostileState = new HostileState(_actor);
        this.patrolState = new PatrolState(_actor);
        this.attackState = new AttackState(_actor);
        this.deathState = new DeathState(_actor);
    }

    public void Initialize(IState state)
    {
        currentState = state;
        state.EnterState();
        
        //Sends out the event that the actors state has changed
        onActorStateChanged?.Invoke(state);
    }

    public void ChangeState(IState nextState)
    {
        currentState.ExitState();
        currentState = nextState;
        nextState.EnterState();


        onActorStateChanged?.Invoke(nextState);
    }

    private void Update()
    {
        if (currentState == null)
        {
            return;
        }
        currentState.UpdateState();
    }
}
