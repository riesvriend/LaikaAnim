using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synchrony;

public class StateMachine 
{
    public IState currentState;
    public IState previousState;

    public StateMachine()
    {
        "<<< StateMachine created".Log();
    }

    public void ChangeState(IState state)
    {
        currentState?.ExitState();

        previousState = currentState;
        currentState = state;
        currentState.EnterState();
    }

    public void ExecuteStateUpdate()
    {
        currentState?.ExecuteState();
    }

    public void ChangeAndExecute(IState state)
    {
        ChangeState(state);
        ExecuteStateUpdate();
    }

    public void SwitchToPreviousState()
    {
        if (previousState != null)
            ChangeState(previousState);
    }
}
