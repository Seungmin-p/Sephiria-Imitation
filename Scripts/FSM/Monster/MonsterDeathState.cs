namespace FSM.MonsterStates
{
    public class MonsterDeathState  : MonsterState
    {
        public MonsterDeathState (Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine)
        {
        }
    
        public override void OnEnter()
        {
            owner.ExecuteDeath();
        }
    }
}