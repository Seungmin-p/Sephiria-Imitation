using UnityEngine;

public class DynamiteMole : RangedMonster
{
    [Header("실제 공격 관련")]
    [SerializeField] Transform throwPoint;
    [SerializeField] Dynamite dynamitePrefab;
    [SerializeField] float targetRandomRange = 2f;
    [SerializeField] float minExplosionDuration = 1f;
    [SerializeField] float maxExplosionDuration = 1.5f;
    
    [Header("다이너마이트 설정")]
    [SerializeField, Min(0f)] float arcHeight = 0.5f; //포물선 최대 높이
    [SerializeField, Range(0.01f, 1f)] float landingPointRatio = 0.8f; //착지 타이밍
    [SerializeField] Vector2 explosionSize = new(2.4f, 1.8f); //폭발 범위
    [SerializeField] LayerMask damageLayer; //데미지 대상
    
    [Header("이동 관련")]
    [SerializeField, Range(0f, 1f)] float randomMoveChance = 0.3f;
    [SerializeField, Min(0.01f)] float moveDirectionDuration = 1.5f;

    [Header("안내선 최소, 최대 길이")]
    [SerializeField] float warningMinLengthScale = 1f;
    [SerializeField] float warningMaxLengthScale = 2f;

    private Vector3 throwPointLocalPosition;
    private Vector2 randomMoveDirection;
    private float moveDirectionTimer;
    private bool useRandomMoveDirection;
    private bool isAttackEnd; //공격 종료 플래그

    protected override void Awake()
    {
        base.Awake();
        if (throwPoint != null) throwPointLocalPosition = throwPoint.localPosition;
    }

    protected override void MoveAwayFromTarget()
    {
        if (target == null) return;

        if (moveDirectionTimer <= 0f)
        {
            moveDirectionTimer = Mathf.Max(0.01f, moveDirectionDuration);
            
            //랜덤이동인 경우, 새 이동방향 확보
            useRandomMoveDirection = Random.value < randomMoveChance;
            if (useRandomMoveDirection)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                randomMoveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
        }

        moveDirectionTimer -= Time.fixedDeltaTime;
        
        //랜덤이동이 아니면 기본 원거리 이동
        if (!useRandomMoveDirection)
        {
            base.MoveAwayFromTarget();
            return;
        }

        //랜덤 이동처리
        rb.Slide(randomMoveDirection * moveSpeed, Time.fixedDeltaTime, slideMovement);
        UpdateSpriteDirection();
    }
    
    //공격 시작 시, 공격 종료 플래그 초기화
    public override void StartAttack()
    {
        isAttackEnd = false;
        base.StartAttack();
    }

    protected override void OnStartAttackPrepare()
    {
        //안내 라인 표시
        ShowAttackWarning();
    }

    //다이너마이트 스폰 위치 X축
    protected override void UpdateAttackPreparePosition()
    {
        if (throwPoint == null) return;

        Vector3 position = throwPointLocalPosition;
        position.x = spriteRenderer.flipX ? -position.x : position.x;
        throwPoint.localPosition = position;
    }

    //현재 공격 사거리에 따른 선 길이 정도 구하기
    protected override float GetWarningLengthScale()
    {
        if (target == null) return warningMinLengthScale;

        //공격의 최소 ~ 최대 사거리와 현재 거리를 이용해서 0~1의 비율값 확보
        float distance = Vector2.Distance(rb.position, target.position);
        float distanceRatio = Mathf.InverseLerp(ApproachDistance, AttackDistance, distance);
        
        //비율값 기준으로 그에 맞는 선 길이 스케일값 전달
        return Mathf.Lerp(warningMinLengthScale, warningMaxLengthScale, distanceRatio);
    }
    
    //경고선 진행
    protected override void UpdateAttackWarning()
    {
        if (!isAttackWarningStarted) return;

        float progress = Mathf.Clamp01(attackPrepareTimer / attackPrepareDuration);

        leftOuter.SetProgress(progress);
        rightOuter.SetProgress(progress);
        
        ApplyWarningLengthScale();
    }
    
    //공격 종료 이벤트를 받으면 플래그 변경
    public override void AttackEnd()
    {
        isAttackEnd = true;
    }
    
    //공격 종료처리
    public override bool ExecuteAttack()
    {
        return isAttackEnd;
    }

    //다이너마이트 두더지 전용 공격 로직
    protected override void OnStartAttack()
    {
        if (target == null || throwPoint == null || dynamitePrefab == null) return;

        //방향 및 다이너마이트 스폰 지점 업데이트
        UpdateSpriteDirection();
        UpdateAttackPreparePosition();
        
        //플레이어 위치에 랜덤값 적용한 최종 좌표
        Vector2 finalTarget = (Vector2)target.position + new Vector2(
            Random.Range(-targetRandomRange, targetRandomRange),
            Random.Range(-targetRandomRange, targetRandomRange));
        
        //공격의 최소 ~ 최대 사거리와 현재 거리를 이용해서 0~1의 비율값 확보
        float distance = Vector2.Distance(throwPoint.position, finalTarget);
        float distanceRatio = Mathf.InverseLerp(ApproachDistance, AttackDistance, distance);
        
        //비율값 기준으로 그에 맞는 폭발 및 투척 시간 확보
        float explosionDuration = Mathf.Lerp(minExplosionDuration, maxExplosionDuration, distanceRatio);
        float flightDuration = explosionDuration * 0.5f;

        //다이너마이트 스폰 및 목표 좌표와 터지기까지의 시간 전달
        Dynamite dynamite = Instantiate(dynamitePrefab, throwPoint.position, Quaternion.identity);
        dynamite.Initialize(finalTarget, explosionDuration, flightDuration, attackDamage, arcHeight, landingPointRatio, explosionSize, damageLayer);
    }
}
