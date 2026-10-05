using UnityEngine;

namespace FSM.PlayerStates
{
    public class PlayerDashState : PlayerMovementStateBase
    {
        private float dashTimer;
        public PlayerDashState(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine)
        {
        }
    
        public override void OnEnter()
        {
            owner.DashActionTriggerDisable();
            dashTimer = owner.DashDuration;
            owner.SetDashFinishedCondition(false);
            owner.SetInvincibility(owner.DashDuration); //대시무적 적용
        }

        public override void OnUpdate()
        {
        }

        public override void OnFixedUpdate()
        {
            dashTimer -= Time.fixedDeltaTime;
        
            if (dashTimer <= 0f)
                owner.SetDashFinishedCondition(true);

            if (CheckTransitions())
                return;
        
            owner.ExecuteDash();
        }

        public override void OnExit()
        {
        }
    }
}