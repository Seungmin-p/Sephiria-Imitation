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
            Debug.Log("Entered PlayerDashState");
            owner.DashActionTriggerDisable();
            dashTimer = owner.DashDuration;
            owner.SetDashFinishedCondition(false);
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