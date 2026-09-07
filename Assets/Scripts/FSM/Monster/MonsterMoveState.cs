namespace FSM.MonsterStates
{
    public class MonsterMoveState : MonsterState
    {
        public MonsterMoveState(Monster owner, StateMachine<Monster> stateMachine) : base(owner, stateMachine)
        {
        }
        
        public override void OnEnter()
        {
            owner.PlayAnimation("Move");
        }

        public override void OnFixedUpdate()
        {
            owner.UpdateSpriteDirection();
            
            if (owner.CanAttack())
            {
                stateMachine.ChangeState(owner.AttackPrepareState);
                return;
            }
            
            owner.ExecuteMove();
        }
    }
}