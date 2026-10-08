using System.Collections.Generic;
using UnityEngine;

public class Dynamite : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] SpriteRenderer visual;
    [SerializeField] Animator animator;
    [SerializeField] Animator explosionAnimator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] CapsuleCollider2D col;
    [SerializeField] Shadow shadow;

    [Header("기타 데이터")]
    [SerializeField] LayerMask obstacleLayer; //몬스터에는 충돌하지 않도록 별도 충돌 레이어 확보
    
    private Vector2 startPosition;
    private Vector2 finalTarget;
    private Vector2 groundPosition;
    private LayerMask damageLayer;
    private float arcHeight;
    private float landingPointRatio;
    private float explosionDamage;
    private Vector2 explosionSize;
    private float explosionTimer;
    private float explosionDuration;
    private float flightDuration;
    private float flightTimer;
    private float flightSpeed;
    private bool isExploded;
    private readonly HashSet<IDamageable> damagedTargets = new();

    private Rigidbody2D.SlideMovement slideMovement;

    public void Initialize(
        Vector2 targetPosition,
        float explosionDuration,
        float flightDuration,
        float explosionDamage,
        float arcHeight,
        float landingPointRatio,
        Vector2 explosionSize,
        LayerMask damageLayer)
    {
        this.explosionDamage = explosionDamage;
        this.explosionDuration = explosionDuration;
        explosionTimer = 0f;
        this.explosionSize = explosionSize;
        this.damageLayer = damageLayer;
        
        startPosition = transform.position;
        finalTarget = targetPosition;
        groundPosition = startPosition;
        this.arcHeight = arcHeight;
        this.landingPointRatio = landingPointRatio;
        this.flightDuration = Mathf.Max(0.01f, flightDuration);
        
        //시간 내 목표 좌표까지 이동하기 위한 속도 확보
        flightSpeed = Vector2.Distance(startPosition, finalTarget) / flightDuration;
        
        //Slide 이동용 데이터
        slideMovement = new Rigidbody2D.SlideMovement
        {
            selectedCollider = col,
            gravity = Vector2.zero,
            surfaceUp = Vector2.zero,
            surfaceAnchor = Vector2.zero,
            maxIterations = 3,
            useNoMove = true,
            useSimulationMove = false
        };
        slideMovement.SetLayerMask(obstacleLayer);
        
        animator.Play("Dynamite", 0, 0f);
    }

    private void FixedUpdate()
    {
        //터진 상태면 패스
        if (isExploded) return;

        //시간을 누적하면서 조건에 다다르면 폭발 진행
        explosionTimer += Time.fixedDeltaTime;
        if (explosionTimer >= explosionDuration)
        {
            Explode();
            return;
        }

        //현재 위치에서 최종 목표까지의 위치 차이 확보
        Vector2 direction = finalTarget - groundPosition;
        
        //진행도 확보
        flightTimer += Time.fixedDeltaTime;
        float progress = Mathf.Clamp01(flightTimer / flightDuration);
        
        //비행 시간이 남아있다면
        if (progress < 1f)
        {
            //높이 보정을 제외한 그라운드 기준 좌표 사용
            slideMovement.SetStartPosition(groundPosition);

            //Slide 기반 이동처리 진행
            Rigidbody2D.SlideResults result = rb.Slide(direction.normalized * flightSpeed, Time.fixedDeltaTime, slideMovement);
            groundPosition = result.position;
        }
        
        //진행도를 포물선 비율에 기반해서 0~1로 변환,
        //포물선 세팅값이 0.5면 진행도가 0.5인 순간부터 1 반환 -> 착지를 의미
        //착지부터는 1로 고정
        float arcProgress = Mathf.Clamp01(progress / Mathf.Clamp(landingPointRatio, 0.01f, 1f));
        
        //포물선 진행도에 따른 0 -> 최대높이 -> 0 포물선 높이 적용
        ApplyPositionWithHeight(4f * arcHeight * arcProgress * (1f - arcProgress));
    }

    //높이 적용
    private void ApplyPositionWithHeight(float height)
    {
        transform.position = new Vector3(groundPosition.x, groundPosition.y + height, transform.position.z);
        shadow.SetAirborneHeight(height);
    }

    //폭발 진행
    private void Explode()
    {
        if (isExploded) return;

        isExploded = true;
        ApplyPositionWithHeight(0f);
        
        //본체 및 그림자 비활성화
        col.enabled = false;
        visual.enabled = false;
        shadow.gameObject.SetActive(false);
        
        animator.Play("Empty", 0, 0f);
        explosionAnimator.Play("Dynamite_Boom", 0, 0f);
        
        //폭발 데미지 적용
        ApplyExplosionDamage();
        
        //5초 후 오브젝트 삭제
        Destroy(gameObject, 5f);
    }

    private void ApplyExplosionDamage()
    {
        //대상 탐색
        Collider2D[] hits = Physics2D.OverlapCapsuleAll(groundPosition, explosionSize, CapsuleDirection2D.Horizontal, 0f, damageLayer);
        foreach (Collider2D hit in hits)
        {
            //데미지 처리
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null || !damagedTargets.Add(damageable)) continue;

            Vector2 direction = ((Vector2)hit.transform.position - groundPosition).normalized;
            if (direction == Vector2.zero) direction = (finalTarget - startPosition).normalized;
            AttackData attackData = new(explosionDamage, direction, false, ElementType.Physical, DamageSource.Enemy);
            DamageResult result = damageable.TakeDamage(attackData);
            if (result.hitResultType != DamageResult.HitResultType.Ignored)
                AttackHitbox.NotifyDamageResolved(hit.ClosestPoint(groundPosition), direction, result);
        }
    }
    
    //대략적인 폭발 범위 확인용
    private void OnDrawGizmosSelected()
    {
        Vector2 center = Application.isPlaying ? groundPosition : (Vector2)transform.position;
        Gizmos.DrawWireCube(center, explosionSize);
    }
}