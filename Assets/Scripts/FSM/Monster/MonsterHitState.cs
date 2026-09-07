using UnityEngine;

namespace FSM.MonsterStates
{
    public class MonsterHitState : MonsterState
    {
        private float knockbackTimer;
    
        public MonsterHitState(Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine)
        {
        }

        public override void OnEnter()
        {
            knockbackTimer = owner.KnockbackDuration;
        }

        public override void OnFixedUpdate()
        {
            knockbackTimer -= Time.fixedDeltaTime;

            if (knockbackTimer <= 0f)
            {
                owner.StartHitStun();
                stateMachine.ChangeState(owner.IdleState);
                return;
            }
        
            owner.ExecuteKnockback();
        }
    }
}