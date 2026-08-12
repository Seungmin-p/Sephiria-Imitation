using UnityEngine;
using Random = UnityEngine.Random;

public class Slime : Monster
{
    [Header("분열 처리")]
    [SerializeField] private Slime smallSlimePrefab;
    [SerializeField] private bool splitAction;
    [SerializeField] private int splitCount = 3;

    [Header("분열 에어본")]
    [SerializeField] private float splitAirborneDuration = 0.65f;
    [SerializeField] private float splitAirborneDistance = 2f;
    [SerializeField] private float splitAirborneHeight = 8f;
    [SerializeField] private float splitWaitDuration = 0.9f;
    
    [Header("사망 파티클")]
    [SerializeField] private SlimeFragment fragmentPrefab;
    [SerializeField] private int fragmentCount = 3;

    private bool isSplitSpawn;
    private bool isSpawnWaiting;
    private float spawnWaitTimer;

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

            SlimeFragment fragment = Instantiate(fragmentPrefab, transform.position, Quaternion.identity);
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
    public override void ExecuteDeath()
    {
        PlayAnimation("Die");
    }
    
    public void OnDeathAnimationEnd()
    {
        base.ExecuteDeath();
    }
}