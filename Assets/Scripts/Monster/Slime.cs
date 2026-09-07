using UnityEngine;
using Random = UnityEngine.Random;

public class Slime : Monster
{
    [Header("분열 처리")]
    [SerializeField] Slime smallSlimePrefab;
    [SerializeField] bool splitAction;
    [SerializeField] int splitCount = 3;

    [Header("분열 에어본")]
    [SerializeField] float splitAirborneDuration = 0.65f;
    [SerializeField] float splitAirborneDistance = 1.5f;
    [SerializeField] float splitAirborneHeight = 6f;
    [SerializeField] float splitWaitDuration = 0.9f;
    
    [Header("사망 파티클")]
    [SerializeField] FragmentEffect fragmentPrefab;
    [SerializeField] int fragmentCount = 3;

    [Header("기타")]
    [SerializeField] int contactDamage = 5;
    
    private bool isSplitSpawn;
    private bool isSpawnWaiting;
    private float spawnWaitTimer;
    
    //플레이어 충돌 판정
    private void OnCollisionEnter2D(Collision2D other)
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

            DamageResult result = damageable.TakeDamage(attackData);

            Vector2 hitPosition = other.GetContact(0).point;
            AttackHitbox.NotifyDamageResolved(hitPosition, attackData.direction, result);
        }
    }

    //분열 처리
    private void Split()
    {
        for (int i = 0; i < splitCount; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector2 direction = Quaternion.Euler(0f, 0f, randomAngle) * Vector2.right;

            Slime smallSlime = Instantiate(smallSlimePrefab, transform.position, Quaternion.identity);
            smallSlime.StartSplitSpawn(direction);
        }
    }
    
    //파티클 생성
    private void SpawnFragments()
    {
        for (int i = 0; i < fragmentCount; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector2 direction = Quaternion.Euler(0f, 0f, randomAngle) * Vector2.right;

            FragmentEffect fragment = Instantiate(fragmentPrefab, transform.position, Quaternion.identity);
            fragment.Initialize(direction);
        }
    }

    //분열로 생성된 슬라임 에어본 시작
    private void StartSplitSpawn(Vector2 direction)
    {
        isSplitSpawn = true;
        isSpawnWaiting = false;
        spawnWaitTimer = 0f;

        hitDirection = direction.normalized;

        //에어본 및 착지 대기 중 상호작용 방지
        col.enabled = false;

        stateMachine.ChangeState(airborneState);
    }

    //에어본 시작
    public override void StartAirborne()
    {
        if (!isSplitSpawn)
        {
            base.StartAirborne();
            return;
        }

        airborneTimer = 0f;
        airborneStartPosition = rb.position;
        airborneEndPosition = airborneStartPosition + hitDirection * splitAirborneDistance;
    }

    //에어본 진행
    public override void ExecuteAirborne()
    {
        if (!isSplitSpawn)
        {
            base.ExecuteAirborne();
            return;
        }

        //착지 후 대기
        if (isSpawnWaiting)
        {
            spawnWaitTimer += Time.fixedDeltaTime;

            if (spawnWaitTimer >= splitWaitDuration)
                EndSplitSpawnWait();

            return;
        }

        airborneTimer += Time.fixedDeltaTime;

        float t = Mathf.Clamp01(airborneTimer / splitAirborneDuration);

        //수평 이동
        float moveT = 1f - Mathf.Pow(1f - t, 2f);
        Vector2 groundPosition = Vector2.Lerp(airborneStartPosition, airborneEndPosition, moveT);

        //공중 높이
        float height = splitAirborneHeight * t * (1f - t);
        
        if (shadow != null)
            shadow.SetAirborneHeight(height);

        rb.MovePosition(groundPosition + Vector2.up * height);

        if (t >= 1f)
            OnAirborneEnd();
    }

    //에어본 종료
    protected override void OnAirborneEnd()
    {
        //작은 슬라임 스폰 처리
        if (isSplitSpawn)
        {
            if (shadow != null)
                shadow.ResetPosition();
            
            PlayAnimation("Idle");
            
            isSpawnWaiting = true;
            spawnWaitTimer = 0f;
            return;
        }

        base.OnAirborneEnd();
    }

    //분열 스폰 대기 종료
    private void EndSplitSpawnWait()
    {
        isSpawnWaiting = false;
        isSplitSpawn = false;

        col.enabled = true;
        stateMachine.ChangeState(moveState);
    }

    //사망 처리 시작
    protected override void StartDeath()
    {
        base.StartDeath();
        
        SpawnFragments();

        if (splitAction)
            Split();
    }
    
    protected override void ApplyDeathVisual()
    {
    }
    
    //사망 상태 진입 시 처리
    public override void StartDeathState()
    {
        PlayAnimation("Die");
    }

    public override void ExecuteDeath()
    {
    }
    
    public override void OnDeathAnimationEnd()
    {
        Destroy(gameObject);
    }
}