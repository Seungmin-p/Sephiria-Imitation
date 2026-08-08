using UnityEngine;

namespace FSM.PlayerStates
{
    public class PlayerMoveState : PlayerMovementStateBase
    {   
        public PlayerMoveState(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine)
        {
        }
    
        public override void OnEnter()
        {
            Debug.Log("Entered PlayerMoveState");
        }

        public override void OnUpdate()
        {
            CheckTransitions();
        }

        public override void OnFixedUpdate()
        {
            owner.ExecuteMove();
        }
    }
}