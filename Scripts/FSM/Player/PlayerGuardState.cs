using UnityEngine;

namespace FSM.PlayerStates
{
    public class PlayerGuardState : PlayerActionStateBase
    {   
        public PlayerGuardState(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine)
        {
        }
    
        public override void OnEnter()
        {
            owner.ClearAttackData();
            owner.StartGuard();
        }

        public override void OnUpdate()
        {
            //TODO : 마나 체크 필요
            CheckTransitions();
        }

        public override void OnExit()
        {
            owner.StopGuard();
        }
    }
}