namespace FSM.MonsterStates
{
    public abstract class MonsterState : State<Monster>
    {
        protected MonsterState(Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine)
        {
        }
    }
}