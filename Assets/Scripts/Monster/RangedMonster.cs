using UnityEngine;

public abstract class RangedMonster : Monster
{
    [Header("실제 공격 관련")]
    [SerializeField] float attackDistance = 6f;
    [SerializeField] float approachDistance = 4f; //공격 범위 밖에서 공격하러 들어올 때 최대한 들어올 거리?
    [SerializeField] float retreatDistance = 3f;
    [SerializeField] protected float attackPrepareDuration = 1.25f; //공격 준비 전체 시간
    [SerializeField] float attackEndDelay = 2.5f;

    [Header("공격 안내선")]
    [SerializeField] Transform attackWarning;
    [SerializeField] protected AttackWarningLine leftOuter;
    [SerializeField] protected AttackWarningLine rightOuter;

    [Header("공격 안내선 설정")]
    [SerializeField] float warningWidth = 1f;
    [SerializeField] float warningStartLength = 0.5f;
    [SerializeField] float warningEndLength = 1.5f;

    private bool needsApproach = true; //가능한 최대한 접근해야하는지 확인
    protected float attackPrepareTimer; //공격 준비 진행 타이머(진행 정도 확인)
    private float attackWarningStartTime; //안내선 시작 당시 공격 타이머 값
    protected Vector2 attackDirection;
    protected bool isAttackWarningStarted;
    private bool isRetreating;

    protected float AttackDistance => attackDistance;
    protected float ApproachDistance => approachDistance;

    protected virtual float GetWarningLengthScale() => 1f;

    //공격 가능한지 체크
    public override bool CanAttack()
    {
        //타겟이 없거나, 공격이 불가능한 상태면 패스
        if (target == null || !base.CanAttack()) return false;

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
    
    //공격 준비 시작
    public override void StartAttackPrepare()
    {
        //공격 준비 상태 초기화
        attackPrepareTimer = 0f;
        attackWarningStartTime = 0f;
        isAttackWarningStarted = false;
        isRetreating = false;

        //스프라이트, 공격 이펙트 x축 업데이트
        UpdateSpriteDirection();
        UpdateAttackPreparePosition();
        UpdateAttackDirection();

        //Animation Event가 발생하기 전까지 이펙트와 경고선 비활성화
        leftOuter.Hide();
        rightOuter.Hide();
        OnStartAttackPrepare();
    }

    //공격 준비 중, 투사체 스폰 위치 업데이트
    protected virtual void UpdateAttackPreparePosition()
    {
    }

    //공격 준비 시작 후, 추가 작업
    protected virtual void OnStartAttackPrepare()
    {
    }

    //공격 안내선 표시
    protected virtual void OnShowAttackWarning()
    {
    }

    //본체 준비 애니메이션의 지정 프레임에서 이펙트와 경고선 시작
    public override void ShowAttackWarning()
    {
        if (stateMachine.CurrentState != attackPrepareState || isAttackWarningStarted)
            return;

        isAttackWarningStarted = true;
        attackWarningStartTime = attackPrepareTimer;

        UpdateSpriteDirection();
        UpdateAttackPreparePosition();
        UpdateAttackDirection();

        SetupAttackWarning();
        OnShowAttackWarning();
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
        ApplyWarningLengthScale();
    }
    
    //선 길이 적용
    protected void ApplyWarningLengthScale()
    {
        float scale = GetWarningLengthScale();
        if (scale == 1f) return;

        Vector3 leftScale = leftOuter.transform.localScale;
        Vector3 rightScale = rightOuter.transform.localScale;
        leftScale.y *= scale;
        rightScale.y *= scale;
        leftOuter.transform.localScale = leftScale;
        rightOuter.transform.localScale = rightScale;
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
        UpdateAttackPreparePosition();
        UpdateAttackDirection();

        //Animation Event 이후부터 실제 발사 시점까지 경고선 진행
        UpdateAttackWarning();

        return attackPrepareTimer >= attackPrepareDuration;
    }
    
    //경고선 진행
    protected virtual void UpdateAttackWarning()
    {
        if (!isAttackWarningStarted) return;

        float duration = Mathf.Max(attackPrepareDuration - attackWarningStartTime, Mathf.Epsilon);
        float progress = Mathf.Clamp01((attackPrepareTimer - attackWarningStartTime) / duration);

        leftOuter.SetProgress(progress);
        rightOuter.SetProgress(progress);
        ApplyWarningLengthScale();
    }

    //공격 시작
    public override void StartAttack()
    {
        base.StartAttack();
        
        //실제 공격이 시작되면, 접근 상태 삭제
        needsApproach = false;

        leftOuter.Hide();
        rightOuter.Hide();

        OnStartAttack();
    }

    //자식 몬스터의 실제 원거리 공격 실행
    protected abstract void OnStartAttack();

    //공격 후 딜레이 적용
    public override void ApplyAttackEndDelay()
    {
        isRetreating = false;
        StartIdleLock(attackEndDelay);
    }

    //공격 후 딜레이 도중 움직임(도망)처리
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

        isAttackWarningStarted = false;
        attackWarningStartTime = 0f;
        isRetreating = false;
    }
}
