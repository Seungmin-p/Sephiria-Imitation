using UnityEngine;

namespace FSM.PlayerStates
{
    public class PlayerNoneState : PlayerActionStateBase
    {   
        public PlayerNoneState(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine)
        {
        }
    
        public override void OnEnter()
        {
            Debug.Log("Entered PlayerNoneState");
        }

        public override void OnUpdate()
        {
            CheckTransitions();
        }
    }
}