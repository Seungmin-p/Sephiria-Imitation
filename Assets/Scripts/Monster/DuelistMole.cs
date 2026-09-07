using UnityEngine;

public class DuelistMole : Monster
{
    [Header("실제 공격 관련")]
    [SerializeField] AttackHitbox attackHitbox;
    [SerializeField] GameObject attackEffect; //공격 이펙트
    [SerializeField] Vector2 attackHitboxSize = new Vector2(1f, 1f);
    [SerializeField] Vector2 attackHitboxOffset = new Vector2(0f, 1f);
    [SerializeField] float attackDistance = 3f; //공격 시작 사거리
    [SerializeField] float attackMoveDistance = 2.4f; //공격하면서 이동할 거리
    [SerializeField] int attackDamage = 10;
    [SerializeField] float attackDuration = 0.3f;
    [SerializeField] float attackPrepareDuration = 0.45f;
    [SerializeField] private float attackEndDelay = 1.5f; //공격 후 딜레이
    [SerializeField] private float attackEffectOffset = 0.5f; //공격 이펙트 오프셋

    [Header("공격 범위 표시 선")]
    [SerializeField] Transform attackWarning;
    [SerializeField] AttackWarningLine leftOuter;
    [SerializeField] AttackWarningLine leftInner;
    [SerializeField] AttackWarningLine rightInner;
    [SerializeField] AttackWarningLine rightOuter;
    
    [Header("공격 범위 표시 설정")]
    [SerializeField] float warningWidth = 1.5f;
    [SerializeField] float innerStartOffset = 0.7f;
    [SerializeField] float outerStartOffset = 0.1f;
    [SerializeField] float warningStartLength = 1.5f;
    [SerializeField] float warningEndLength = 3f;


    private float attackTimer;
    private Vector2 attackStartPosition;
    private Vector2 attackEndPosition;

    private float attackPrepareTimer;
    private Vector2 attackDirection;

    public override bool CanAttack()
    {
        if (target == null)
            return false;

        //플레이어가 공격 사거리내에 있는지 확인해서 bool 반환
        return Vector2.Distance(transform.position, target.position) <= attackDistance;
    }

    public override void StartAttackPrepare()
    {
        //공격 대기 시간 초기화
        attackPrepareTimer = 0f;
        
        //attackWarning 객체 부모 재지정 
        attackWarning.SetParent(transform, false);
        attackWarning.localPosition = Vector3.zero;

        //공격 방향 지정
        attackDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;

        //공격 각도 조정
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg - 90f;
        attackWarning.rotation = Quaternion.Euler(0f, 0f, angle);
        attackHitbox.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        attackEffect.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        SetupAttackWarning();
    }
    
    //공격표시 선들 초기화
    private void SetupAttackWarning()
    {
        float halfWidth = warningWidth * 0.5f;

        Vector3 leftEnd = new Vector3(-halfWidth, 0f, 0f);
        Vector3 rightEnd = new Vector3(halfWidth, 0f, 0f);

        //4개 선에 대한 시작 위치, 도착 지점, 시작 및 종료 길이 부여
        leftOuter.Setup(
            leftEnd + Vector3.left * outerStartOffset,
            leftEnd,
            warningStartLength,
            warningEndLength);

        leftInner.Setup(
            leftEnd + Vector3.right * innerStartOffset,
            leftEnd,
            warningStartLength,
            warningEndLength);

        rightInner.Setup(
            rightEnd + Vector3.left * innerStartOffset,
            rightEnd,
            warningStartLength,
            warningEndLength);

        rightOuter.Setup(
            rightEnd + Vector3.right * outerStartOffset,
            rightEnd,
            warningStartLength,
            warningEndLength);
    }
    
    //공격 준비(차징)
    public override bool ExecuteAttackPrepare()
    {
        attackPrepareTimer += Time.fixedDeltaTime;

        //시간 경과에 따른 진행도 전달
        float progress = Mathf.Clamp01(attackPrepareTimer / attackPrepareDuration);

        leftOuter.SetProgress(progress);
        leftInner.SetProgress(progress);
        rightInner.SetProgress(progress);
        rightOuter.SetProgress(progress);

        //진행도가 100%가 되면 true 반환
        return progress >= 1f;
    }
    
    //공격 시작
    public override void StartAttack()
    {
        //공격 타이머 초기화
        attackTimer = 0f;

        //공격 시작, 끝 위치 지정
        attackStartPosition = rb.position;
        attackEndPosition = attackStartPosition + attackDirection * attackMoveDistance;
        
        //부모를 분리해서 공격 시작 위치에 공격 범위 표시 고정
        attackWarning.SetParent(null, true);
        
        //공격 이펙트 활성화
        attackEffect.transform.position = (Vector2)transform.position + attackDirection * attackEffectOffset;
        attackEffect.SetActive(true);

        attackHitbox.ResetHitTargets();
    }
    
    //공격 진행
    public override bool ExecuteAttack()
    {
        attackTimer += Time.fixedDeltaTime;
        
        float progress = Mathf.Clamp01(attackTimer / attackDuration);

        //진행도에 따른 위치 이동
        Vector2 attackPosition = Vector2.Lerp(attackStartPosition, attackEndPosition, progress);
        rb.MovePosition(attackPosition);

        DetectAttackTarget();
        
        //진행도에 따른 공격 표시 페이드아웃
        leftOuter.SetFadeProgress(progress);
        leftInner.SetFadeProgress(progress);
        rightInner.SetFadeProgress(progress);
        rightOuter.SetFadeProgress(progress);

        //진행도가 100%가 되면 true 반환
        if (progress >= 1f)
        {
            leftOuter.Hide();
            leftInner.Hide();
            rightInner.Hide();
            rightOuter.Hide();

            return true;
        }

        return false;
    }
    
    private void DetectAttackTarget()
    {
        AttackData attackData = new(
            attackDamage,
            attackDirection,
            false,
            ElementType.Physical,
            DamageSource.Enemy
        );

        attackHitbox.DetectTargets(
            attackHitboxSize,
            attackHitboxOffset,
            attackData,
            null
        );
    }
    
    //공격 이후 딜레이 시간 지정
    public override void StartAttackEndDelay()
    {
        StartIdleLock(attackEndDelay);
    }
    
    //행동 취소처리
    protected override void CancelCurrentAction()
    {
        leftOuter.Hide();
        leftInner.Hide();
        rightInner.Hide();
        rightOuter.Hide();

        attackEffect.SetActive(false);

        //공격 중 부모가 분리됐을 수도 있으므로 원래 구조로 복구
        attackWarning.SetParent(transform, false);
        attackWarning.localPosition = Vector3.zero;
    }
    
    private void OnDrawGizmosSelected()
    {
        //공격 시작 사거리
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        //최종 공격 범위 표시
        if (attackWarning != null)
        {
            Gizmos.color = Color.cyan;

            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = attackWarning.localToWorldMatrix;

            Vector3 warningCenter = new Vector3(0f, warningEndLength * 0.5f, 0f);
            Vector3 warningSize = new Vector3(warningWidth, warningEndLength, 0f);

            Gizmos.DrawWireCube(warningCenter, warningSize);

            Gizmos.matrix = previousMatrix;
        }

        //실제 공격 판정
        if (attackHitbox != null)
            attackHitbox.DrawHitboxGizmo(attackHitboxSize, attackHitboxOffset);
    }
}