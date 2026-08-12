using System;
using System.Collections;
using UnityEngine;
using FSM;
using FSM.MonsterStates;

public class Monster : MonoBehaviour, IDamageable
{
    [Header("컴포넌트")] 
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Collider2D col;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Shadow shadow;

    [Header("각종 속성")] 
    [SerializeField] protected Transform target; //플레이어 위치
    [SerializeField] protected float detectionRange = 5f; //플레이어 탐지 범위
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected int contactDamage;
    [SerializeField] protected float knockbackSpeed = 3f; //넉백 속도
    [SerializeField] protected float knockbackDuration = 0.1f; //넉백 유지 시작
    [SerializeField] protected float turnSpeed = 2f; //관성처리
    [SerializeField] protected int defense; //방어력

    [Header("피격 연출")] 
    [SerializeField] protected Transform visual;
    [SerializeField] protected float hitStretchScale = 2f;
    [SerializeField] protected float hitStretchDuration = 0.1f;
    [SerializeField] protected float hitRecoverDuration = 0.1f;

    [Header("에어본 연출")] 
    [SerializeField] protected float airborneDuration = 0.6f;
    [SerializeField] protected float airborneDistance = 1.5f;
    [SerializeField] protected float airborneHeight = 2f;

    //기타 변수
    [SerializeField] protected float currentHealth; //(체력 확인용)
    
    protected Vector2 hitDirection; //피격 방향 저장용
    protected Vector3 originalVisualScale;
    protected Color originalColor;
    protected Coroutine hitVisualCoroutine;
    protected Vector2 currentMoveDirection; //관성에 사용될 기존 이동 방향
    protected bool isDying;

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

    public MonsterIdleState IdleState => idleState;
    public MonsterMoveState MoveState => moveState;
    public MonsterHitState HitState => hitState;
    public MonsterDeathState DeathState => deathState;
    public MonsterAirborneState AirborneState => airborneState;

    public float KnockbackDuration => knockbackDuration;

    protected virtual void Awake()
    {
        //체력, 초기 크기, 색상 초기화
        currentHealth = maxHealth;
        originalVisualScale = visual.localScale;
        originalColor = spriteRenderer.color;

        stateMachine = new StateMachine<Monster>();

        idleState = new MonsterIdleState(this, stateMachine);
        moveState = new MonsterMoveState(this, stateMachine);
        hitState = new MonsterHitState(this, stateMachine);
        deathState = new MonsterDeathState(this, stateMachine);
        airborneState = new MonsterAirborneState(this, stateMachine);

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
        stateMachine.FixedUpdate();
    }
    
    //특정 애니메이션 플레이
    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    //플레이어가 탐지 범위 내에 존재하는지 확인
    public bool DetectTarget()
    {
        if (target == null) return false;

        float distance = Vector2.Distance(rb.position, target.position);

        return distance <= detectionRange;
    }

    //움직임 처리
    public virtual void ExecuteMove()
    {
        if (target == null) return;

        Vector2 targetDirection = ((Vector2)target.position - rb.position).normalized;
        currentMoveDirection = Vector2.Lerp(currentMoveDirection, targetDirection, turnSpeed * Time.fixedDeltaTime)
            .normalized;

        rb.MovePosition(rb.position + currentMoveDirection * (moveSpeed * Time.fixedDeltaTime));

        UpdateSpriteDirection();
    }

    //방향 전환
    private void UpdateSpriteDirection()
    {
        if (currentMoveDirection.x == 0f) return;

        spriteRenderer.flipX = target.position.x < transform.position.x;
    }

    //피격 데미지 판정
    public virtual DamageResult TakeDamage(AttackData attackData)
    {
        //최종 데미지 계산
        float finalDamage = CalculateFinalDamage(attackData.damage);

        currentHealth = Mathf.Max(currentHealth - finalDamage, 0f);

        //피격 시각 연출
        PlayHitVisual();

        //데미지 결과 생성
        DamageResult result = new(finalDamage, attackData.isCritical, false, false, attackData.element,
            attackData.source);

        Debug.Log($"{name} 피해 : {finalDamage}, 현재 HP : {currentHealth}");
        
        //피격 방향 저장
        hitDirection = attackData.direction;
        
        //체력이 0이면 사망 에어본 처리
        if (currentHealth <= 0f)
        {
            StartDeath();
            stateMachine.ChangeState(airborneState);
            return result;
        }
        
        //에어본 중이라면 Hit 상태로 전환하지 않음
        if (stateMachine.CurrentState == airborneState)
            return result;
        
        //hit 상태 전환
        //TODO : 보호막이 있다면 무시
        stateMachine.ChangeState(hitState);

        return result;
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

        visual.localScale = originalVisualScale;
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
            visual.localScale = Vector3.Lerp(originalVisualScale, stretchedScale, t);
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
            visual.localScale = Vector3.Lerp(stretchedScale, originalVisualScale, t);
            yield return null;
        }

        visual.localScale = originalVisualScale;
        hitVisualCoroutine = null;
    }

    //피격 넉백
    public void ExecuteKnockback()
    {
        Vector2 nextPosition = rb.position + hitDirection * (knockbackSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
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
            stateMachine.ChangeState(deathState);
        else
            stateMachine.ChangeState(moveState);
    }

    //플레이어 충돌 판정
    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            IDamageable damageable = other.collider.GetComponentInParent<IDamageable>();

            if (damageable == null) return;

            AttackData attackData = new(
                contactDamage,
                ((Vector2)other.transform.position - rb.position).normalized,
                false,
                ElementType.Physical,
                DamageSource.Enemy
            );

            damageable.TakeDamage(attackData);
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

        visual.localScale = originalVisualScale;
        spriteRenderer.color = originalColor;
        ApplyDeathVisual();
    }
    
    protected virtual void ApplyDeathVisual()
    {
        spriteRenderer.color = Color.gray;
    }
    
    //사망 상태 진입 시 처리
    public virtual void ExecuteDeath()
    {
        Destroy(gameObject);
    }
}