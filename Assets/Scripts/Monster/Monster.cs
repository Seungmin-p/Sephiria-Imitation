using System;
using System.Collections;
using UnityEngine;
using FSM;
using FSM.MonsterStates;

public class Monster : MonoBehaviour, IDamageable
{
    [Header("컴포넌트")]
    [SerializeField] protected Transform body;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Collider2D col;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Shadow shadow;
    [SerializeField] protected LayerMask collisionMask;

    [Header("각종 속성")] 
    [SerializeField] protected Transform target; //플레이어 위치
    [SerializeField] protected float detectionRange = 5f; //플레이어 탐지 범위
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float knockbackSpeed = 3f; //넉백 속도
    [SerializeField] protected float knockbackDuration = 0.1f; //넉백 유지 시작
    [SerializeField] protected float turnSpeed = 2f; //관성처리
    [SerializeField] protected int defense; //방어력
    [SerializeField] protected float maxShield; //보호막
    [SerializeField, Range(0f, 1f)] protected float shieldAbsorbRate = 0.35f; //보호막 데미지 비율
    
    [Header("공격 관련")]
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackCooldown = 1f;

    [Header("피격 연출")] 
    [SerializeField] protected float hitStretchScale = 1.5f;
    [SerializeField] protected float hitStretchDuration = 0.1f;
    [SerializeField] protected float hitRecoverDuration = 0.1f;
    [SerializeField] protected float hitStunDuration = 0.2f; //피격 후 경직

    [Header("에어본 연출")] 
    [SerializeField] protected float airborneDuration = 0.6f;
    [SerializeField] protected float airborneDistance = 1.5f;
    [SerializeField] protected float airborneHeight = 2f;
    [SerializeField] protected float downDuration = 2f; //에어본 이후 단순 Down 상태 유지할 경우
    
    [Header("사망 연출")]
    [SerializeField] protected float deathWaitDuration = 2f;
    [SerializeField] protected float deathFadeDuration = 0.5f;
    
    [Header("보호막 연출")]
    [SerializeField] protected Material defaultMaterial;
    [SerializeField] protected Material shieldMaterial;

    //기타 변수
    [SerializeField] protected float currentHealth; //(체력 확인용)
    
    protected Vector2 hitDirection; //피격 방향 저장용
    protected Vector3 originalVisualScale;
    protected Color originalColor;
    protected Coroutine hitVisualCoroutine;
    protected Vector2 currentMoveDirection; //관성에 사용될 기존 이동 방향
    protected bool isDying;
    protected float currentShield;
    protected bool isPlayerDetected; //플레이어 탐지 여부
    protected float idleLockTimer; //Idle 대기 상태(후딜레이 등)
    protected float deathTimer;
    protected bool isDeathFading;
    protected float downTimer;
    protected bool isDown;
    protected Rigidbody2D.SlideMovement slideMovement;
    protected float attackCooldownTimer; //공격 쿨타임, 후딜레이랑은 별도로 같이 돌아감

    //에어본
    protected float airborneTimer;
    protected Vector2 airborneStartPosition;
    protected Vector2 airborneEndPosition;

    //상태머신 변수들
    protected StateMachine<Monster> stateMachine;

    protected MonsterIdleState idleState;
    protected MonsterMoveState moveState;
    protected MonsterHitState hitState;
    protected MonsterDeathState deathState;
    protected MonsterAirborneState airborneState;
    protected MonsterAttackState attackState;
    protected MonsterAttackPrepareState attackPrepareState;
    protected MonsterDownState downState;

    public MonsterIdleState IdleState => idleState;
    public MonsterMoveState MoveState => moveState;
    public MonsterHitState HitState => hitState;
    public MonsterDeathState DeathState => deathState;
    public MonsterAirborneState AirborneState => airborneState;
    public MonsterAttackState AttackState => attackState;
    public MonsterAttackPrepareState AttackPrepareState => attackPrepareState;
    public MonsterDownState DownState => downState;

    public float KnockbackDuration => knockbackDuration;
    public bool HasShield => currentShield > 0f;
    public bool IsPlayerDetected => isPlayerDetected;
    public bool IsIdleLocked => idleLockTimer > 0f;

    //===================== 기본 로직 =====================
    protected virtual void Awake()
    {
        //체력, 초기 크기, 색상 초기화
        currentHealth = maxHealth;
        currentShield = maxShield;
        originalVisualScale = body.localScale;
        originalColor = spriteRenderer.color;
        UpdateShieldMaterial();
        
        slideMovement = new Rigidbody2D.SlideMovement
        {
            selectedCollider = col,
            gravity = Vector2.zero,
            surfaceUp = Vector2.zero,
            surfaceAnchor = Vector2.zero,
            maxIterations = 2,
            useSimulationMove = true
        };
        slideMovement.SetLayerMask(collisionMask);
        
        stateMachine = new StateMachine<Monster>();

        idleState = new MonsterIdleState(this, stateMachine);
        moveState = new MonsterMoveState(this, stateMachine);
        hitState = new MonsterHitState(this, stateMachine);
        deathState = new MonsterDeathState(this, stateMachine);
        airborneState = new MonsterAirborneState(this, stateMachine);
        attackState = new MonsterAttackState(this, stateMachine);
        attackPrepareState = new MonsterAttackPrepareState(this, stateMachine);
        downState = new MonsterDownState(this, stateMachine);

        stateMachine.ChangeState(idleState);
    }

    protected virtual void Start()
    {
        //플레이어 객체 찾아서 타겟으로 저장해두기
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            target = playerObject.transform;
    }

    protected virtual void Update()
    {
        stateMachine.Update();
    }

    protected virtual void FixedUpdate()
    {
        //후딜레이, 공격 쿨타임 감소
        if (idleLockTimer > 0f) idleLockTimer -= Time.fixedDeltaTime;
        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.fixedDeltaTime;
        
        stateMachine.FixedUpdate();
    }
    
    //===================== 이동 및 방향, 기타 =====================
    //애니메이션 플레이
    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }
    
    //애니메이션 플레이 시도
    public void TryPlayAnimation(string stateName)
    {
        int hash = Animator.StringToHash(stateName);

        if (animator.HasState(0, hash))
            animator.Play(hash);
        else
            animator.Play("Idle");
    }

    //플레이어가 탐지 범위 내에 존재하는지 확인
    public bool DetectTarget()
    {
        if (target == null) return false;

        float distance = Vector2.Distance(rb.position, target.position);

        if (distance <= detectionRange)
            isPlayerDetected = true;
        
        return isPlayerDetected;
    }

    //움직임 처리
    public virtual void ExecuteMove()
    {
        if (target == null) return;

        Vector2 targetDirection = ((Vector2)target.position - rb.position).normalized;
        currentMoveDirection = Vector2.Lerp(currentMoveDirection, targetDirection, turnSpeed * Time.fixedDeltaTime)
            .normalized;

        Vector2 velocity = currentMoveDirection * moveSpeed;
        rb.Slide(velocity, Time.fixedDeltaTime, slideMovement);

        UpdateSpriteDirection();
    }
    
    //플레이어 반대 방향으로 이동
    protected virtual void MoveAwayFromTarget()
    {
        if (target == null) return;

        //플레이어의 정 반대방향
        Vector2 moveDirection = (rb.position - (Vector2)target.position).normalized;
        Vector2 velocity = moveDirection * moveSpeed;

        rb.Slide(velocity, Time.fixedDeltaTime, slideMovement);

        //이동 방향과 관계없이 플레이어 방향을 바라봄
        UpdateSpriteDirection();
    }

    //방향 전환
    public void UpdateSpriteDirection()
    {
        if (target == null) return;

        //방향 전환이 필요한지 체크
        bool shouldFlip = target.position.x < transform.position.x;
        if (spriteRenderer.flipX == shouldFlip) return;

        //방향 및 그림자 x축 반전 진행
        spriteRenderer.flipX = shouldFlip;

        if (shadow != null)
            shadow.FlipXPosition();
    }
    
    //딜레이용 Idle 유지 시간 지정
    public void StartIdleLock(float duration)
    {
        idleLockTimer = duration;
    }
    
    //도망용 메소드
    public virtual void ExecuteIdleLockMovement()
    {
    }
    
    //===================== 공격 로직 =====================
    //공격 가능 여부
    public virtual bool CanAttack()
    {
        return !(attackCooldownTimer > 0f);
    }

    //공격 준비 시작
    public virtual void StartAttackPrepare()
    {
    }

    //공격 준비 애니메이션 이벤트
    public virtual void ShowAttackWarning()
    {
    }

    //공격 준비 진행
    public virtual bool ExecuteAttackPrepare()
    {
        return false;
    }

    //공격 시작
    public virtual void StartAttack()
    {
        //공격 쿨타임 적용
        attackCooldownTimer = attackCooldown;
    }

    //공격 진행
    public virtual bool ExecuteAttack()
    {
        return false;
    }
    
    //공격 종료
    public virtual void AttackEnd()
    {
    }
    
    //공격 후딜레이 지정
    public virtual void ApplyAttackEndDelay()
    {
    }
    
    //행동 캔슬 메소드
    protected virtual void CancelCurrentAction()
    {
    }
    
    //보호막 상태에 따른 머티리얼 적용
    protected void UpdateShieldMaterial()
    {
        spriteRenderer.material = HasShield ? shieldMaterial : defaultMaterial;
    }
    
    //이동 관성 초기화
    public void ResetMoveDirection()
    {
        if (target == null)
        {
            currentMoveDirection = Vector2.zero;
            return;
        }

        currentMoveDirection = ((Vector2)target.position - rb.position).normalized;
    }
    
    //공격 애니메이션 호출 시도
    public virtual void PlayAttackAnimation()
    {
        PlayAnimation("Attack");
    }

    //===================== 피격 및 사망 로직 =====================
    //피격 데미지 판정
    public virtual DamageResult TakeDamage(AttackData attackData)
    {
        //사망 중이라면 피격 패스
        if (isDying)
        {
            return new DamageResult(
                0f,
                attackData.isCritical,
                DamageResult.HitResultType.Ignored,
                attackData.element,
                attackData.source
            );
        }
        
        //피격 전 보호막 존재 여부 저장
        bool hadShield = HasShield;

        //최종 데미지 계산
        float finalDamage = CalculateFinalDamage(attackData.damage);

        //보호막 적용
        float healthDamage = ApplyShieldDamage(finalDamage);

        currentHealth = Mathf.Max(currentHealth - healthDamage, 0f);

        //피격 시각 연출
        PlayHitVisual();

        //데미지 결과 생성
        DamageResult result = new(healthDamage, attackData.isCritical, DamageResult.HitResultType.Hit, attackData.element,
            attackData.source);

        Debug.Log($"{name} 피해 : {healthDamage}, 현재 HP : {currentHealth}, " +
                  $"보호막 피해 : {finalDamage - healthDamage}, 현재 보호막 : {currentShield}");

        //피격 방향 저장
        hitDirection = attackData.direction;

        //체력이 0이면 현재 행동 취소 후 사망 에어본 처리
        if (currentHealth <= 0f)
        {
            CancelCurrentAction();
            StartDeath();
            stateMachine.ChangeState(airborneState);
            return result;
        }

        //보호막이 있었던 경우
        if (hadShield)
        {
            //보호막이 남아있으면 현재 행동 유지
            if (HasShield)
                return result;
            
            //머티리얼 변경
            UpdateShieldMaterial();

            //보호막이 깨지면 현재 행동 취소 후 에어본
            CancelCurrentAction();
            
            isDown = true;
            
            stateMachine.ChangeState(airborneState);
            return result;
        }

        //이미 에어본 중이라면 Hit 상태로 전환하지 않음
        if (stateMachine.CurrentState == airborneState)
            return result;

        //보호막이 없던 상태에서 피격되면 현재 행동 취소 후 Hit
        CancelCurrentAction();
        stateMachine.ChangeState(hitState);

        return result;
    }
    
    //보호막 데미지 처리
    protected float ApplyShieldDamage(float damage)
    {
        if (!HasShield)
            return damage;

        float shieldDamage = Mathf.Min(currentShield, damage * shieldAbsorbRate);

        currentShield = Mathf.Max(currentShield - shieldDamage, 0f);

        return damage - shieldDamage;
    }

    //방어력 적용 피해랑 계산
    protected float CalculateFinalDamage(float damage)
    {
        float damageReduction = 44.5f * Mathf.Log(defense / 40f + 1f) / 100f;
        return damage * (1f - Mathf.Clamp01(damageReduction));
    }

    //몬스터 피격 연출 시작
    private void PlayHitVisual()
    {
        if (hitVisualCoroutine != null)
            StopCoroutine(hitVisualCoroutine);

        body.localScale = originalVisualScale;
        spriteRenderer.color = originalColor;

        hitVisualCoroutine = StartCoroutine(HitVisualCoroutine());
    }

    //피격 연출 코루틴
    private IEnumerator HitVisualCoroutine()
    {
        //색상 변경
        spriteRenderer.color = Color.red;

        //변경될 크기
        Vector3 stretchedScale = new(
            originalVisualScale.x,
            originalVisualScale.y * hitStretchScale,
            originalVisualScale.z
        );

        float timer = 0f;

        //세로로 늘어남
        while (timer < hitStretchDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / hitStretchDuration);
            body.localScale = Vector3.Lerp(originalVisualScale, stretchedScale, t);
            yield return null;
        }

        //색상 복귀
        spriteRenderer.color = originalColor;

        timer = 0f;

        //원래 크기로 복귀
        while (timer < hitRecoverDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / hitRecoverDuration);
            body.localScale = Vector3.Lerp(stretchedScale, originalVisualScale, t);
            yield return null;
        }

        body.localScale = originalVisualScale;
        hitVisualCoroutine = null;
    }

    //피격 넉백
    public void ExecuteKnockback()
    {
        Vector2 nextPosition = rb.position + hitDirection * (knockbackSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
    }
    
    //피격 경직 부여
    public void StartHitStun()
    {
        StartIdleLock(hitStunDuration);
    }

    //에어본 시작
    public virtual void StartAirborne()
    {
        //에어본 타이머, 시작 위치, 착지 위치
        airborneTimer = 0f;
        airborneStartPosition = rb.position;
        airborneEndPosition = airborneStartPosition + hitDirection.normalized * airborneDistance;
    }

    //에어본 진행
    public virtual void ExecuteAirborne()
    {
        airborneTimer += Time.fixedDeltaTime;

        float t = Mathf.Clamp01(airborneTimer / airborneDuration);

        //바닥상의 이동
        float moveT = 1f - Mathf.Pow(1f - t, 2f);
        Vector2 groundPosition = Vector2.Lerp(airborneStartPosition, airborneEndPosition, moveT);

        //공중 높이
        float height = airborneHeight * t * (1f - t);
        
        if (shadow != null)
            shadow.SetAirborneHeight(height);

        //몬스터 본체 이동
        rb.MovePosition(groundPosition + Vector2.up * height);

        //에어본 종료
        if (t >= 1f)
            OnAirborneEnd();
    }

    //에어본 종료
    protected virtual void OnAirborneEnd()
    {
        if (shadow != null)
            shadow.ResetPosition();
        
        if (isDying)
        {
            stateMachine.ChangeState(deathState);
            return;
        }

        if (isDown)
        {
            stateMachine.ChangeState(downState);
            return;
        }
    }

    //사망 확정 후, 사전 작업
    protected virtual void StartDeath()
    {
        isDying = true;
        col.enabled = false;

        if (hitVisualCoroutine != null)
        {
            StopCoroutine(hitVisualCoroutine);
            hitVisualCoroutine = null;
        }

        body.localScale = originalVisualScale;
        spriteRenderer.color = originalColor;
        ApplyDeathVisual();
    }
    
    protected virtual void ApplyDeathVisual()
    {
        spriteRenderer.color = Color.gray;
    }
    
    //사망상태 변경 시
    public virtual void StartDeathState()
    {
        deathTimer = 0f;
        isDeathFading = false;

        PlayAnimation("Die");
    }
    
    //사망 애니메이션 종료
    public virtual void OnDeathAnimationEnd()
    {
    }
    
    //사망 상태 처리
    public virtual void ExecuteDeath()
    {
        deathTimer += Time.deltaTime;

        if (!isDeathFading)
        {
            if (deathTimer < deathWaitDuration)
                return;

            isDeathFading = true;
            deathTimer = 0f;
        }

        float progress = Mathf.Clamp01(deathTimer / deathFadeDuration);

        Color color = spriteRenderer.color;
        color.a = 1f - progress;
        spriteRenderer.color = color;

        if (progress >= 1f)
            Destroy(gameObject);
    }
    
    //사망이 아닌 단순 Down 상태
    public void StartDown()
    {
        downTimer = 0f;
        StartIdleLock(downDuration + (downDuration/5));

        PlayAnimation("Die");
    }
    
    //다운상태 시간 체크
    public bool ExecuteDown()
    {
        downTimer += Time.deltaTime;

        if (downTimer < downDuration)
            return false;

        isDown = false;

        return true;
    }
}
