using UnityEngine;

public class ChargeMonster : Monster
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
    [SerializeField] Vector2 warningOffset = Vector2.zero;
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
        attackWarning.SetParent(transform, true);
        attackWarning.localPosition = warningOffset;

        //공격 방향 지정
        attackDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;

        //공격 각도 조정
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg - 90f;
        attackWarning.rotation = Quaternion.Euler(0f, 0f, angle);
        attackHitbox.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        if (attackEffect != null)
            attackEffect.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        SetupAttackWarning();
    }
    
    //공격표시 선들 초기화
    private void SetupAttackWarning()
    {
        GetWarningLinePositions(
            out Vector3 leftOuterStart,
            out Vector3 leftInnerStart,
            out Vector3 rightInnerStart,
            out Vector3 rightOuterStart,
            out Vector3 leftEnd,
            out Vector3 rightEnd);

        //4개 선에 대한 시작 위치, 도착 지점, 시작 및 종료 길이 부여
        leftOuter.Setup(
            leftOuterStart,
            leftEnd,
            warningStartLength,
            warningEndLength);

        leftInner.Setup(
            leftInnerStart,
            leftEnd,
            warningStartLength,
            warningEndLength);

        rightInner.Setup(
            rightInnerStart,
            rightEnd,
            warningStartLength,
            warningEndLength);

        rightOuter.Setup(
            rightOuterStart,
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
        if (attackEffect != null)
        {
            attackEffect.transform.position = (Vector2)transform.position + attackDirection * attackEffectOffset;
            attackEffect.SetActive(true);
        }

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
        // animator.Play("AttackEnd");
        StartIdleLock(attackEndDelay);
    }
    
    //행동 취소처리
    protected override void CancelCurrentAction()
    {
        leftOuter.Hide();
        leftInner.Hide();
        rightInner.Hide();
        rightOuter.Hide();

        if (attackEffect != null)
            attackEffect.SetActive(false);

        //공격 중 부모가 분리됐을 수도 있으므로 원래 구조로 복구
        attackWarning.SetParent(transform, true);
        attackWarning.localPosition = warningOffset;
    }
    
    private void OnDrawGizmosSelected()
    {
        //공격 시작 사거리
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        //최종 공격 범위 표시
        if (attackWarning != null)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Matrix4x4 warningMatrix = transform.localToWorldMatrix * Matrix4x4.TRS(
                warningOffset,
                Quaternion.Inverse(transform.rotation) * attackWarning.rotation,
                attackWarning.localScale);
            Gizmos.matrix = warningMatrix;

            GetWarningLinePositions(
                out Vector3 leftOuterStart,
                out Vector3 leftInnerStart,
                out Vector3 rightInnerStart,
                out Vector3 rightOuterStart,
                out Vector3 leftEnd,
                out Vector3 rightEnd);

            Gizmos.color = Color.cyan;
            Vector3 warningCenter = new Vector3(0f, warningEndLength * 0.5f, 0f);
            Vector3 warningSize = new Vector3(warningWidth, warningEndLength, 0f);
            Gizmos.DrawWireCube(warningCenter, warningSize);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(Vector3.zero, 0.06f);

            DrawWarningLineGizmo(leftOuterStart, leftEnd);
            DrawWarningLineGizmo(leftInnerStart, leftEnd);
            DrawWarningLineGizmo(rightInnerStart, rightEnd);
            DrawWarningLineGizmo(rightOuterStart, rightEnd);

            Gizmos.matrix = Matrix4x4.TRS(transform.position, attackWarning.rotation, Vector3.one);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, Vector3.up * attackMoveDistance);
            Gizmos.DrawWireSphere(Vector3.up * attackMoveDistance, 0.1f);

            Gizmos.matrix = previousMatrix;
        }

        //실제 공격 판정
        if (attackHitbox != null)
            attackHitbox.DrawHitboxGizmo(attackHitboxSize, attackHitboxOffset);
    }

    private void GetWarningLinePositions(
        out Vector3 leftOuterStart,
        out Vector3 leftInnerStart,
        out Vector3 rightInnerStart,
        out Vector3 rightOuterStart,
        out Vector3 leftEnd,
        out Vector3 rightEnd)
    {
        float halfWidth = warningWidth * 0.5f;

        leftEnd = new Vector3(-halfWidth, 0f, 0f);
        rightEnd = new Vector3(halfWidth, 0f, 0f);

        leftOuterStart = leftEnd + Vector3.left * outerStartOffset;
        leftInnerStart = leftEnd + Vector3.right * innerStartOffset;
        rightInnerStart = rightEnd + Vector3.left * innerStartOffset;
        rightOuterStart = rightEnd + Vector3.right * outerStartOffset;
    }

    private void DrawWarningLineGizmo(Vector3 startPosition, Vector3 endPosition)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(startPosition, 0.05f);
        Gizmos.DrawWireCube(
            startPosition + Vector3.up * (warningStartLength * 0.5f),
            new Vector3(0.08f, warningStartLength, 0f));

        Gizmos.color = Color.white;
        Gizmos.DrawLine(startPosition, endPosition);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(endPosition, 0.05f);
        Gizmos.DrawWireCube(
            endPosition + Vector3.up * (warningEndLength * 0.5f),
            new Vector3(0.08f, warningEndLength, 0f));
    }
}
