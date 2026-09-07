namespace FSM.MonsterStates
{
    public class MonsterAttackPrepareState : MonsterState
    {
        public MonsterAttackPrepareState(Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine)
        {
        }

        public override void OnEnter()
        {
            //공격 시작 전 사전 작업
            owner.PlayAnimation("AttackPrepare");
            owner.StartAttackPrepare();
        }

        public override void OnFixedUpdate()
        {
            //공격 준비가 다 됐으면 공격 상태로 전환
            if (owner.ExecuteAttackPrepare())
            {
                stateMachine.ChangeState(owner.AttackState);
                return;
            }
        }
    }
}