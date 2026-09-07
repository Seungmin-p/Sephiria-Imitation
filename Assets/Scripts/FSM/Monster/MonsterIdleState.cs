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
            //플레이어 탐지가 아직 안됐다면
            if (!owner.IsPlayerDetected)
            {
                if (owner.DetectTarget())
                {
                    stateMachine.ChangeState(owner.MoveState);
                    return;
                }
            }
            else
            {
                //탐지가 됐다면 공격 후딜레이 등의 상태를 의미
                
                //방향 업데이트
                owner.UpdateSpriteDirection();
            
                //만약 Idle 대기 시간이 남아있으면 대기
                if (owner.IsIdleLocked)
                    return;

                stateMachine.ChangeState(owner.MoveState);
            }
        }
    }
}