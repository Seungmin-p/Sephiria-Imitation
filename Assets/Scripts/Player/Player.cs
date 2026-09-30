using UnityEngine;
using UnityEngine.InputSystem;
using FSM;
using FSMGraph;
using FSM.PlayerStates;

public class Player : MonoBehaviour
{
    [Header("플레이어 컴포넌트")]
    [SerializeField] Animator animator;
    [SerializeField] PlayerDirection playerDirection; //플레이어 방향처리용 스크립트
    [SerializeField] PlayerMovement playerMovement; //플레이어 이동처리용 스크립트
    [SerializeField] PlayerAttack playerAttack; //플레이어 공격처리용 스크립트
    [SerializeField] PlayerGuard playerGuard; //플레이어 방어처리용 스크립트
    [SerializeField] PlayerDeath playerDeath; //플레이어 사망처리용 스크립트
    [SerializeField] PlayerStats playerStats; //플레이어 스탯
    [SerializeField] PlayerInvincibility playerInvincibility; //플레이어 무적 처리용
    [SerializeField] StatusEffectController statusEffect; //버프, 디버프 관리용
    
    [Header("상태머신 그래프")]
    [SerializeField] FSMRuntimeGraph movementGraph;
    [SerializeField] FSMRuntimeGraph actionGraph;
    
    //플레이어 스탯, 버프디버프, 방향
    public PlayerStats Stats => playerStats;
    public StatusEffectController StatusEffect => statusEffect;
    public Vector2 LookDirection => playerDirection.LookDirection;
    public bool IsInvincible => playerInvincibility.IsInvincible; //무적 여부
    
    //===================== 상태 머신용 변수 =====================
    //상태머신들
    private StateMachine<Player> movementStateMachine;
    private StateMachine<Player> actionStateMachine;
    
    private PlayerDeathState deathState; //별도로 관리할 사망상태
    
    //상태머신에서 이용할 변수
    public bool HasMoveInput => playerMovement.HasMoveInput;
    public float DashDuration => playerMovement.DashDuration;
    public bool IsGuardHeld => playerGuard.IsGuardHeld;
    public bool AttackActionTrigger => playerAttack.AttackActionTrigger;
    public bool AttackMotionEnd => playerAttack.AttackMotionEnd;
    public bool AttackWaitMotionEnd => playerAttack.AttackWaitMotionEnd;
    public bool StrikeAttackMotionEnd => playerAttack.StrikeAttackMotionEnd;
    public bool DashAttackMotionEnd => playerAttack.DashAttackMotionEnd;
    public bool IsThirdCombo => playerAttack.IsThirdCombo;
    public bool DashFinishedCondition => playerMovement.DashFinishedCondition;
    public bool DashActionTrigger => playerMovement.DashActionTrigger;
    public bool CanStrikeAttack => playerAttack.CanStrikeAttack();
    
    //현재 상태 확인용
    public bool IsDashing => movementStateMachine.CurrentState is PlayerDashState;
    public bool IsGuarding => actionStateMachine.CurrentState is PlayerGuardState;
    public bool IsAttacking => actionStateMachine.CurrentState is PlayerAttackState;
    public bool IsStrikeAttacking => actionStateMachine.CurrentState is PlayerStrikeAttackState;
    public bool IsDashAttacking => actionStateMachine.CurrentState is PlayerDashAttackState;
    private bool IsComboWait => actionStateMachine.CurrentState is PlayerComboWaitState;
    public bool IsDead => actionStateMachine.CurrentState is PlayerDeathState;
    
    private void Awake()
    {
        //각각 상태머신 생성
        movementStateMachine = movementGraph.CreateStateMachine(this);
        actionStateMachine = actionGraph.CreateStateMachine(this);
        
        //사망 상태 별도 생성
        deathState = new PlayerDeathState(this, actionStateMachine, playerDeath);

        //사망 처리에 이용할 충돌 대상 지정(몬스터 제외)
        LayerMask deathCollisionMask = playerMovement.CollisionMask & ~LayerMask.GetMask("Enemy");
        playerDeath.SetCollisionMask(deathCollisionMask);
    }
    
    private void Update()
    {   
        if (!IsDead)
        {
            playerDirection.UpdateDirection();

            if (!(IsAttacking || IsStrikeAttacking || IsDashAttacking))
                playerDirection.ApplyDirection(playerDirection.LookDirection);

            movementStateMachine.Update();

            //애니메이션 업데이트, 대시, 마나 리젠
            UpdateAnimation();
            playerMovement.UpdateDashRecharge();
            playerStats.UpdateManaRegen();
        }

        actionStateMachine.Update();
    }
    
    private void FixedUpdate()
    {
        if (!IsDead)
            movementStateMachine.FixedUpdate();

        actionStateMachine.FixedUpdate();
    }
    
    //플레이어 방향 적용 호출
    public void ApplyDirection(Vector2 direction)
    {
        playerDirection.ApplyDirection(direction);
    }
    
    //플레이어 사망 상태 전환
    public void ChangeDeathState(Vector2 hitDirection)
    {
        playerDeath.PrepareDeath(hitDirection);
        actionStateMachine.ChangeState(deathState);
    }

    //=================================== 플레이어 이동 및 애니메이션 ===================================
    //플레이어 이동 입력 받기
    public void OnMove(InputAction.CallbackContext context)
    {
        playerMovement.OnMove(context);
    }
    
    //플레이어 대시 입력 받기
    public void OnDash(InputAction.CallbackContext context)
    {
        playerMovement.OnDash(context);
    }
    
    //플레이어 움직임 처리 - 상태머신
    public void ExecuteMove()
    {
        if (IsAttacking || IsStrikeAttacking || IsDashAttacking) return;
            
        playerMovement.ExecuteMove();
    }
    
    //플레이어 대시 처리 - 상태머신
    public void ExecuteDash()
    {
        playerMovement.ExecuteDash();
    }
    
    //대시 트리거 비활성화 - 상태머신
    public void DashActionTriggerDisable()
    {
        playerMovement.DashActionTriggerDisable();
    }

    //플레이어 대시 종료 확인용 - 상태머신
    public void SetDashFinishedCondition(bool value)
    {
        playerMovement.SetDashFinishedCondition(value);
    }

    //플레이어 공격 이동처리
    public void PlayerAttackMove()
    {
        playerAttack.PlayerAttackMove();
    }
    
    //애니메이션 업데이트
    private void UpdateAnimation()
    {
        //위 아래 보는 방향 구분
        bool isAttack = IsAttacking || IsStrikeAttacking || IsDashAttacking;
        Vector2 animationDirection = isAttack ? playerAttack.AttackDirection : playerDirection.LookDirection;
        float lookY = animationDirection.y > 0f ? 1f : -1f;
        animator.SetFloat("LookY", lookY);
        
        //움직임 구분
        animator.SetBool("IsMoving", isAttack ? false : playerMovement.HasMoveInput);
        animator.SetBool("IsAttacking",IsAttacking);
        animator.SetBool("IsStrikeAttacking",IsStrikeAttacking);
    }
    
    //=================================== 공격 ===================================
    //플레이어 공격 입력 받기
    public void OnAttack(InputAction.CallbackContext context)
    {
        playerAttack.HandleAttackInput(context);
    }

    //일반 공격 실행
    public void ExecuteAttack()
    {
        playerAttack.ExecuteAttack();
    }
    
    //특수 공격 실행
    public void ExecuteStrikeAttack()
    {
        playerAttack.ExecuteStrikeAttack();
    }

    //대시 공격 실행
    public void ExecuteDashAttack()
    {
        playerAttack.ExecuteDashAttack();
    }

    //공격 판정 및 이펙트 실행
    public void OnAttackActive()
    {
        playerAttack.AttackActive();
    }
    
    //콤보 체크
    public void ComboCheck()
    {
        playerAttack.ComboCheck();
    }
    
    //공격 모션 종료
    public void OnAttackAnimationEnd()
    {
        playerAttack.AttackAnimationEnd();
    }
    
    //특수, 대시 공격 모션 종료
    public void OnStrikeAttackEnd()
    {
        playerAttack.StrikeAttackEnd();
    }
    
    //공격 후 대기 모션 종료
    public void OnAttackEnd()
    {
        playerAttack.AttackEnd();
    }

    //공격 대기 상태에 들어갔기 때문에 공격 모션 종료 트리거는 다시 비활성화 - 상태머신
    public void WaitForNextAttack()
    {
        playerAttack.WaitForNextAttack();
    }

    //공격 상태 및 애니메이션 초기화
    public void ExecuteAttackEnd()
    {
        playerAttack.ExecuteAttackEnd();
    }

    //공격 상태 초기화
    public void ClearAttackData()
    {
        playerAttack.ClearAttackData();
    }
    
    //공격 판정(지속)
    public void PlayerAttackHitCheck()
    {
        playerAttack.PlayerAttackHitCheck();
    }
    
    //=================================== 방어 ===================================
    //플레이어 방어 입력 받기
    public void OnGuard(InputAction.CallbackContext context)
    {
        playerGuard.OnGuard(context);
    }
    
    //방어 계산 진행
    public DamageResult.HitResultType TryGuard(AttackData attackData)
    {
        return playerGuard.TryGuard(attackData);
    }
    
    public float GetStrikeAttackManaCost(float baseCost)
    {
        return playerGuard.GetStrikeAttackManaCost(baseCost);
    }

    //상태머신용
    public void StartGuard()
    {
        playerGuard.StartGuard();
    }
    
    //상태머신용
    public void UpdateGuard()
    {
        playerGuard.UpdateGuard();
    }

    //상태머신용
    public void StopGuard()
    {
        playerGuard.StopGuard();
    }

    public void OnGuardingEnd()
    {
        playerGuard.OnGuardingEnd();
    }
    
    //플레이어 무적 적용 호출
    public void SetInvincibility(float duration)
    {
        playerInvincibility.SetInvincibility(duration);
    }
}