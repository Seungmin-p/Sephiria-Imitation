using UnityEngine;

public class BoomMole : Monster
{
    [Header("실제 공격 관련")]
    [SerializeField] GameObject attackEffect;
    [SerializeField] BoomMoleProjectile projectilePrefab;
    [SerializeField] float attackDistance = 6f;
    [SerializeField] float approachDistance = 4f; //공격 범위 밖에서 공격하러 들어올 때 최대한 들어올 거리?
    [SerializeField] float retreatDistance = 3f;
    [SerializeField] float attackPrepareDuration = 1.25f;
    [SerializeField] float attackEndDelay = 2.5f;
    
    [SerializeField] int projectileCount = 4;
    [SerializeField] float projectileDamage = 6f;
    [SerializeField] float projectileSpeed = 5f;
    [SerializeField] float projectileLifetime = 3f;

    [SerializeField] Vector2 projectilePositionRandomRange = new Vector2(0.1f, 0.05f);
    [SerializeField] float projectileAngleRandomRange = 10f;
    [SerializeField] float projectileSpeedRandomRange = 1.5f;

    [Header("공격 범위 표시 선")]
    [SerializeField] Transform attackWarning;
    [SerializeField] AttackWarningLine leftOuter;
    [SerializeField] AttackWarningLine rightOuter;

    [Header("공격 범위 표시 설정")]
    [SerializeField] float warningWidth = 1f;
    [SerializeField] float warningStartLength = 0.5f;
    [SerializeField] float warningEndLength = 1.5f;

    private Animator attackEffectAnimator;
    private Vector3 attackEffectLocalPosition;
    private bool needsApproach = true; //가능한 최대한 접근해야하는지 확인
    private float attackPrepareTimer;
    private float attackCueStartTime;
    private Vector2 attackDirection;
    private bool isAttackCueStarted;
    private bool isRetreating;

    protected override void Awake()
    {
        base.Awake();

        if (attackEffect != null)
        {
            attackEffectAnimator = attackEffect.GetComponent<Animator>();
            attackEffectLocalPosition = attackEffect.transform.localPosition;
        }
    }

    //공격 가능한지 체크
    public override bool CanAttack()
    {
        if (target == null) return false;

        float distance = Vector2.Distance(rb.position, target.position);

        //최대 공격 사거리 밖으로 벗어났으면 다시 접근 필요
        if (distance > attackDistance)
        {
            needsApproach = true;
            return false;
        }

        //접근이 필요한 상태라면 지정 거리까지 접근
        if (needsApproach)
            return distance <= approachDistance;

        //공격권 내에 계속 유지되는 상태면 그대로 공격
        return true;
    }
    
    //공격 이펙트 x축 보정
    private void UpdateAttackEffectPosition()
    {
        if (attackEffect == null) return;

        Vector3 position = attackEffectLocalPosition;
        position.x = Mathf.Abs(position.x) * (spriteRenderer.flipX ? -1f : 1f);

        attackEffect.transform.localPosition = position;
    }

    //공격 준비 시작
    public override void StartAttackPrepare()
    {
        //공격 준비 상태 초기화
        attackPrepareTimer = 0f;
        attackCueStartTime = 0f;
        isAttackCueStarted = false;
        isRetreating = false;

        //스프라이트, 공격 이펙트 x축 업데이트
        UpdateSpriteDirection();
        UpdateAttackEffectPosition();
        UpdateAttackDirection();

        //Animation Event가 발생하기 전까지 이펙트와 경고선 비활성화
        leftOuter.Hide();
        rightOuter.Hide();

        if (attackEffect != null)
            attackEffect.SetActive(false);
    }

    //본체 준비 애니메이션의 지정 프레임에서 이펙트와 경고선 시작
    public override void OnAttackPrepareCue()
    {
        if (stateMachine.CurrentState != attackPrepareState || isAttackCueStarted)
            return;

        isAttackCueStarted = true;
        attackCueStartTime = attackPrepareTimer;

        UpdateSpriteDirection();
        UpdateAttackEffectPosition();
        UpdateAttackDirection();

        SetupAttackWarning();

        if (attackEffect == null) return;

        attackEffect.SetActive(true);

        if (attackEffectAnimator != null)
            attackEffectAnimator.Play("BoomPrepare_1");
    }
    
    //원거리용 공격 선 배치
    private void SetupAttackWarning()
    {
        //선 범위만큼 좌우로 늘려서 배치
        float halfWidth = warningWidth * 0.5f;

        Vector3 leftStart = new(-halfWidth, 0f, 0f);
        Vector3 rightStart = new(halfWidth, 0f, 0f);
        Vector3 endPosition = Vector3.zero;

        leftOuter.Setup(leftStart, endPosition, warningStartLength, warningEndLength);
        rightOuter.Setup(rightStart, endPosition, warningStartLength, warningEndLength);
    }
    
    //공격 방향 조정
    private void UpdateAttackDirection()
    {
        if (target == null) return;

        attackDirection = ((Vector2)target.position - rb.position).normalized;

        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg - 90f;
        attackWarning.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    //공격 준비단계 진행
    public override bool ExecuteAttackPrepare()
    {
        attackPrepareTimer += Time.fixedDeltaTime;

        //공격 준비 중에도 계속 플레이어 추적
        UpdateSpriteDirection();
        UpdateAttackEffectPosition();
        UpdateAttackDirection();

        //Animation Event 이후부터 실제 발사 시점까지 경고선 진행
        if (isAttackCueStarted)
        {
            float duration = Mathf.Max(attackPrepareDuration - attackCueStartTime, Mathf.Epsilon);
            float progress = Mathf.Clamp01((attackPrepareTimer - attackCueStartTime) / duration);

            leftOuter.SetProgress(progress);
            rightOuter.SetProgress(progress);
        }

        return attackPrepareTimer >= attackPrepareDuration;
    }

    public override void PlayAttackAnimation()
    {
        PlayAnimation("Idle");
    }

    public override void StartAttack()
    {
        //실제 공격이 시작되면, 접근 상태 삭제
        needsApproach = false;

        leftOuter.Hide();
        rightOuter.Hide();

        if (attackEffectAnimator != null)
            attackEffectAnimator.Play("BoomFire");

        SpawnProjectiles();
    }
    
    //투사체 생성
    private void SpawnProjectiles()
    {
        if (projectilePrefab == null) return;

        //기본 위치
        Vector2 basePosition = attackEffect != null
            ? attackEffect.transform.position
            : transform.position;

        //공격 방향에 수직인 방향, 좌우로 총알을 흩뿌리기 위해 사용
        Vector2 sideDirection = new Vector2(-attackDirection.y, attackDirection.x);

        for (int i = 0; i < projectileCount; i++)
        {
            //발사 위치 랜덤 오차
            float sideOffset = Random.Range(-projectilePositionRandomRange.x, projectilePositionRandomRange.x);
            float forwardOffset = Random.Range(-projectilePositionRandomRange.y, projectilePositionRandomRange.y);

            //랜덤값 적용해서 스폰 진행
            Vector2 spawnPosition =
                basePosition +
                sideDirection * sideOffset +
                attackDirection * forwardOffset;

            //발사 각도 랜덤 오차, 최종 각도 도출
            float angleOffset = Random.Range(-projectileAngleRandomRange, projectileAngleRandomRange);
            Vector2 projectileDirection = Quaternion.Euler(0f, 0f, angleOffset) * (Vector3)attackDirection;

            //속도 랜덤 오차, 최종 속도 도출
            float speed = projectileSpeed + Random.Range(-projectileSpeedRandomRange, projectileSpeedRandomRange);

            //총알 생성
            BoomMoleProjectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            //총알에 각도, 속도, 데미지, 유지시간 부여
            projectile.Initialize(projectileDirection, speed, projectileDamage, projectileLifetime);
        }
    }

    //원거리 공격은 총알을 생성하는 순간 종료
    //따라서 공격 실행 시 바로 다음 상태로 넘어갈 수 있도록 true 전달
    public override bool ExecuteAttack()
    {
        return true;
    }

    public override void StartAttackEndDelay()
    {
        isRetreating = false;
        StartIdleLock(attackEndDelay);
    }

    public override void ExecuteIdleLockMovement()
    {
        if (target == null)
        {
            if (isRetreating)
            {
                isRetreating = false;
                PlayAnimation("Idle");
            }

            return;
        }

        float distance = Vector2.Distance(rb.position, target.position);

        //거리가 확보되면 후퇴를 끝내고 Idle 애니메이션으로 복귀
        if (distance >= retreatDistance)
        {
            if (isRetreating)
            {
                isRetreating = false;
                PlayAnimation("Idle");
            }

            return;
        }

        //후퇴를 시작할 때 Move 애니메이션을 한 번만 재생
        if (!isRetreating)
        {
            isRetreating = true;
            PlayAnimation("Move");
        }

        //너무 가까우면 도망가기 시작
        MoveAwayFromTarget();
    }

    //공격 캔슬 당할 시
    protected override void CancelCurrentAction()
    {
        leftOuter.Hide();
        rightOuter.Hide();

        isAttackCueStarted = false;
        attackCueStartTime = 0f;
        isRetreating = false;

        if (attackEffect != null)
            attackEffect.SetActive(false);
    }
}
