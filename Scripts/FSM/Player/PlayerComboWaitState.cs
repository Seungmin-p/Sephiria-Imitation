using UnityEngine;

namespace FSM.PlayerStates
{
    public class PlayerComboWaitState : PlayerActionStateBase
    {   
        public PlayerComboWaitState(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine)
        {
        }
    
        public override void OnEnter()
        {
            Debug.Log("Entered PlayerComboWaitState");
            owner.WaitForNextAttack();
        }

        public override void OnUpdate()
        {
            owner.ComboCheck();

            CheckTransitions();

            //추가 공격이 없어서 공격이 종료됐다면 공격 데이터 초기화
            if (owner.AttackWaitMotionEnd)
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