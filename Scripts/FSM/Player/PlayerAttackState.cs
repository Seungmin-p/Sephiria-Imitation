using UnityEngine;

namespace FSM.PlayerStates
{
    public class PlayerAttackState : PlayerActionStateBase
    {   
        public PlayerAttackState(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine)
        {
        }
    
        public override void OnEnter()
        {
            Debug.Log("Entered PlayerAttackState");
            owner.ExecuteAttack();
        }

        public override void OnUpdate()
        {
            CheckTransitions();

            // 3타 공격 모션이 종료됐다면 공격 데이터 초기화
            if (owner.AttackMotionEnd && owner.IsThirdCombo)
                owner.ExecuteAttackEnd();
        }

        public override void OnFixedUpdate()
        {
        }

        public override void OnExit()
        {
        }
    }
}