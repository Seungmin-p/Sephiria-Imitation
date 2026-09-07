using UnityEngine;

namespace FSM.PlayerStates
{
    public class PlayerIdleState : PlayerMovementStateBase
    {   
        public PlayerIdleState(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine)
        {
        }

        public override void OnUpdate()
        {
            CheckTransitions();
        }
    }
}