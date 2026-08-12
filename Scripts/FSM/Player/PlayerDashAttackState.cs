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
            owner.ExecuteDashAttack();
        }

        public override void OnUpdate()
        {
            CheckTransitions();
        }
        
        public override void OnFixedUpdate()
        {
            owner.PlayerAttackMove();
            owner.PlayerAttackHitCheck();
        }

        public override void OnExit()
        {
            owner.ExecuteAttackEnd();
        }
    }
}