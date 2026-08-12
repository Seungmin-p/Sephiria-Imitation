using System.Collections.Generic;
using UnityEngine;
using FSM;
using FSM.PlayerStates;

public class PlayerDownState : PlayerActionStateBase
{   
    public PlayerDownState(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine)
    {
    }
    
    public override void OnEnter()
    {
    }

    public override void OnUpdate()
    {
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnExit()
    {
    }
}