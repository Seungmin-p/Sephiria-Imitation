namespace FSM.MonsterStates
{
    public class MonsterDownState : MonsterState
    {
        public MonsterDownState(Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine)
        {
        }
        
        public override void OnEnter()
        {
            owner.StartDown();
        }

        public override void OnUpdate()
        {
            if (!owner.ExecuteDown())
                return;

            stateMachine.ChangeState(owner.IdleState);
        }
    }
}