using UnityEngine;

namespace FSM.PlayerStates
{
    public class PlayerStrikeAttackState : PlayerActionStateBase
    {   
        public PlayerStrikeAttackState(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine)
        {
        }
    
        public override void OnEnter()
        {
            Debug.Log("Entered PlayerStrikeAttackState");
            owner.ExecuteStrikeAttack();
        }

        public override void OnUpdate()
        {
            CheckTransitions();
        }

        public override void OnExit()
        {
            owner.ExecuteAttackEnd();
        }
    }
}