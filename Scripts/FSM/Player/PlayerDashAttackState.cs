using UnityEngine;

namespace FSM.PlayerStates
{
    public class PlayerDashAttackState : PlayerActionStateBase
    {   
        public PlayerDashAttackState(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine)
        {
        }
    
        public override void OnEnter()
        {
            Debug.Log("Entered PlayerDashAttackState");
            owner.ExecuteDashAttack();
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