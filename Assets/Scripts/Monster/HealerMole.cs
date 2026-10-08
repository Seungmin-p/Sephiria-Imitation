using UnityEngine;
using System.Collections.Generic;

public class HealerMole : Monster
{
    [Header("힐 관련")]
    [SerializeField] LayerMask monsterLayer;
    [SerializeField] GameObject healEffect;
    [SerializeField] GameObject healReceiveEffectPrefab;
    [SerializeField] float healSearchRange = 8f;
    [SerializeField] float healCastDistance = 1f;
    [SerializeField] float distanceScoreWeight = 3f;
    [SerializeField] float healAmount = 999f;
    [SerializeField] float attackPrepareDuration = 0.5f;
    [SerializeField] Vector2 healAreaSize = new Vector2(1.5f, 0.8f);
    
    [Header("도망 관련")]
    [SerializeField] float retreatStartDistance = 4.5f;
    [SerializeField] float retreatStopDistance = 6f;
    
    private Monster healTarget;
    private Vector2 healPosition;
    private Vector2 healDirection;
    private float attackPrepareTimer;
    private bool isAttackEnd;
    private bool isRetreating;
    
    //힐 대상에게 접근, 대상이 없으면 플레이어와의 거리에 따라 후퇴
    public override void ExecuteMove()
    {
        //힐 대상이 없으면 플레이어한테서 도망
        if (healTarget == null)
        {
            MoveAwayFromTarget();
            return;
        }
        
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Move"))
            PlayAnimation("Move");

        //힐 대상 방향 확보 후, 이동 진행
        Vector2 targetDirection = ((Vector2)healTarget.transform.position - rb.position).normalized;

        currentMoveDirection = Vector2.Lerp(
            currentMoveDirection,
            targetDirection,
            turnSpeed * Time.fixedDeltaTime).normalized;

        rb.Slide(currentMoveDirection * moveSpeed, Time.fixedDeltaTime, slideMovement);

        UpdateSpriteDirection();
    }
    
    //힐 대상을 바라보며 이동 진행, 힐 대상이 없으면 플레이어에게서 등돌려 도망
    public override void UpdateSpriteDirection()
    {
        Transform lookTarget = healTarget != null ? healTarget.transform : target;
        if (lookTarget == null) return;

        bool shouldFlip = healTarget != null
            ? lookTarget.position.x < transform.position.x   //힐 대상은 바라봄
            : lookTarget.position.x > transform.position.x;  //플레이어는 등돌림

        if (spriteRenderer.flipX == shouldFlip) return;

        spriteRenderer.flipX = shouldFlip;

        if (shadow != null)
            shadow.FlipXPosition();
    }
    
    //힐 가능 여부
    public override bool CanAttack()
    {
        if (!base.CanAttack()) return false;
        
        //대상이 없거나 사망했다면 새 대상 탐색
        if (healTarget == null || healTarget.IsDying)
            healTarget = FindHealTarget();

        //타겟을 못찾았으면 false
        if (healTarget == null) return false;

        //타겟이 힐 사거리까지 접근했을 때 true 반환 -> 공격(힐) 상태 진입
        return Vector2.Distance(rb.position, healTarget.transform.position) <= healCastDistance;
    }
    
    //힐 대상 탐색
    private Monster FindHealTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, healSearchRange, monsterLayer);

        Monster bestTarget = null;
        float bestScore = float.MinValue;
        bool hasDamagedTarget = false;

        foreach (Collider2D hit in hits)
        {
            Monster monster = hit.GetComponentInParent<Monster>();

            //힐 불가능한 대상 제외
            if (monster == null || monster == this || monster is HealerMole || monster.IsDying)
                continue;

            bool isDamaged = monster.HealthRatio < 1f;

            //부상 몬스터가 발견되면 풀피 몬스터보다 우선
            if (isDamaged && !hasDamagedTarget)
            {
                hasDamagedTarget = true;
                bestScore = float.MinValue;
            }

            //부상 몬스터가 존재하면 풀피 몬스터 제외
            if (!isDamaged && hasDamagedTarget)
                continue;

            //대상까지 거리 확보 후, 가까울 수록 점수를 높게줌
            float distance = Vector2.Distance(rb.position, monster.transform.position);
            float distanceScore = (healSearchRange - distance) * distanceScoreWeight;

            //체력이 많이 닳았을수록 높은 점수(1%당 1점)
            float healthScore = (1f - monster.HealthRatio) * 100f;

            //점수 합산
            float score = healthScore + distanceScore;

            //최고 점수를 넘기지 못했으면 패스, 최고점수를 넘었으면 점수 및 타겟 재지정
            if (score <= bestScore) continue;

            bestScore = score;
            bestTarget = monster;
        }

        return bestTarget;
    }
    
    //힐 준비 시작
    public override void StartAttackPrepare()
    {
        attackPrepareTimer = 0f;

        //현재 힐 대상 위치 및 방향 고정
        healPosition = healTarget.transform.position;
        healDirection = (healPosition - rb.position).normalized;

        //힐 대상 방향 바라보기
        UpdateSpriteDirection();

        //힐 이펙트 위치 및 방향 고정
        if (healEffect != null)
        {
            healEffect.transform.position = healPosition;
            healEffect.transform.up = healDirection;
        }
    }
    
    //힐 준비 진행(힐 준비 타이머)
    public override bool ExecuteAttackPrepare()
    {
        attackPrepareTimer += Time.fixedDeltaTime;

        return attackPrepareTimer >= attackPrepareDuration;
    }
    
    //힐 시작
    public override void StartAttack()
    {
        isAttackEnd = false;

        //힐 쿨타임 적용
        base.StartAttack();

        //힐 이펙트 출력
        if (healEffect != null)
            healEffect.SetActive(true);

        //고정해둔 위치에 광역 힐
        ApplyAreaHeal();
    }
    
    //범위 내 힐 진행
    private void ApplyAreaHeal()
    {
        //힐 방향에 맞는 각도 확보
        float angle = Vector2.SignedAngle(Vector2.up, healDirection);

        //고정된 힐 위치를 중심으로 범위 판정
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            healPosition,
            healAreaSize,
            angle,
            monsterLayer);

        HashSet<Monster> healedTargets = new();

        foreach (Collider2D hit in hits)
        {
            Monster monster = hit.GetComponentInParent<Monster>();

            //힐이 불가능하거나 이미 힐을 진행한 대상 제외
            if (monster == null || monster is HealerMole || monster.IsDying || !healedTargets.Add(monster))
                continue;

            //힐 진행
            monster.Heal(healAmount);

            //힐 받은 위치에 이펙트 생성
            if (healReceiveEffectPrefab != null)
                Instantiate(healReceiveEffectPrefab, monster.ShadowPosition, Quaternion.identity);
        }
    }
    
    //힐 진행, 공격 상태 종료용
    public override bool ExecuteAttack()
    {
        return isAttackEnd;
    }
    
    //힐 종료
    public override void AttackEnd()
    {
        isAttackEnd = true;
        healTarget = null;
    }
    
    //힐 행동 취소 시 타겟만 제거
    protected override void CancelCurrentAction()
    {
        healTarget = null;
    }
    
    //플레이어와의 거리에 따른 후퇴 처리
    protected override void MoveAwayFromTarget()
    {
        if (target == null) return;

        float distance = Vector2.Distance(rb.position, target.position);

        //후퇴 중 충분한 거리를 확보했다면 중지
        if (isRetreating)
        {
            if (distance >= retreatStopDistance)
            {
                isRetreating = false;
                PlayAnimation("Idle");
                return;
            }
        }
        //후퇴 중이 아니면 시작 거리 확인
        else
        {
            if (distance > retreatStartDistance)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    PlayAnimation("Idle");

                return;
            }

            isRetreating = true;
            PlayAnimation("Move");
        }

        //실제 후퇴 이동
        base.MoveAwayFromTarget();
    }
    
    //힐 범위 확인용
    private void OnDrawGizmosSelected()
    {
        if (healEffect == null) return;

        Matrix4x4 previousMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            healEffect.transform.position,
            healEffect.transform.rotation,
            Vector3.one);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, healAreaSize);

        Gizmos.matrix = previousMatrix;
    }
}