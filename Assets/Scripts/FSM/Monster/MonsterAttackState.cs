namespace FSM.MonsterStates
{
    public class MonsterAttackState : MonsterState
    {
        public MonsterAttackState(Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine)
        {
        }

        public override void OnEnter()
        {
            owner.PlayAnimation("Attack");
            owner.StartAttack();
        }

        public override void OnFixedUpdate()
        {
            //공격이 전부 끝나면 상태 전환
            if (owner.ExecuteAttack())
            {
                //공격 후딜레이 적용
                owner.StartAttackEndDelay();
                stateMachine.ChangeState(owner.IdleState);
                return;
            }
        }
    }
}