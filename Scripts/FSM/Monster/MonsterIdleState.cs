namespace FSM.MonsterStates
{
    public class MonsterIdleState : MonsterState
    {
        public MonsterIdleState(Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine)
        {
        }
        
        public override void OnEnter()
        {
            owner.PlayAnimation("Idle");
        }
    
        public override void OnUpdate()
        {
            if (owner.DetectTarget())
            {
                stateMachine.ChangeState(owner.MoveState);
            }
        }
    }
}