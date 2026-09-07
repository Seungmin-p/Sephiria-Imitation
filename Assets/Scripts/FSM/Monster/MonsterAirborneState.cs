namespace FSM.MonsterStates
{
    public class MonsterAirborneState : MonsterState
    {
        public MonsterAirborneState(Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine)
        {
        }

        public override void OnEnter()
        {
            owner.StartAirborne();
            owner.PlayAnimation("Airborne");
        }

        public override void OnFixedUpdate()
        {
            owner.ExecuteAirborne();
        }
    }
}