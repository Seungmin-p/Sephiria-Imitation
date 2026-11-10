using UnityEngine;

public class GodfatherMole : Monster
{
    [Header("기본 돌진")] 
    [SerializeField] private SingleDashAttack.Settings singleDashSettings = new();

    [Header("2중 돌진")] 
    [SerializeField] private DoubleDashAttack.Settings doubleDashSettings = new();

    [Header("차지 공격")] 
    [SerializeField] private ChargeAttack.Settings chargeSettings = new();
    
    [Header("다이너마이트 공격")] 
    [SerializeField] private DynamiteAttack.Settings dynamiteSettings = new();
    
    [Header("드릴 공격")]
    [SerializeField] private DrillAttack.Settings drillSettings = new();

    [Header("점프, 착지")] 
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject jumpLandingEffect; //이펙트
    [SerializeField, Min(0f)] private float offscreenMargin = 2f;
    [SerializeField] private string jumpPrepareAnimation;
    [SerializeField] private string jumpAnimation;
    [SerializeField] private string landingAnimation;
    [SerializeField] private string landingEndAnimation;

    [Header("각 패턴별 가중치")] 
    [SerializeField, Min(0)] private int singleWeight;
    [SerializeField, Min(0)] private int doubleWeight;
    [SerializeField, Min(0)] private int chargeWeight;
    [SerializeField, Min(0)] private int dynamiteWeight;
    [SerializeField, Min(0)] private int drillWeight;

    [Header("기즈모")] 
    [SerializeField] private bool showDashAttackGizmos;
    [SerializeField] private bool showChargeAttackGizmos;

    private BossAttackPattern singleAttack;
    private BossAttackPattern doubleAttack;
    private BossAttackPattern drillAttack;
    private BossAttackPattern chargeAttack;
    private BossAttackPattern dynamiteAttack;
    private BossAttackPattern currentAttack;

    private Vector2 aerialStartPosition; //상승 or 하강 시작 위치
    private Vector2 aerialEndPosition; //상승 or 하강 도착 위치
    private Vector2 jumpStartPosition; //점프 시작 위치
    private float aerialTimer;
    private float aerialDuration;

    internal Rigidbody2D AttackRigidbody => rb;
    internal Transform AttackTarget => target;
    internal float AttackDamage => attackDamage;
    internal Vector2 JumpStartPosition => jumpStartPosition;
    internal Camera AttackCamera => playerCamera;

    private int TotalAttackWeight => singleWeight + doubleWeight + drillWeight + chargeWeight + dynamiteWeight;

    //===================== 기본 로직 =====================
    protected override void Awake()
    {
        base.Awake();

        //각 패턴별 클래스 등록
        singleAttack = new SingleDashAttack(this, singleDashSettings);
        doubleAttack = new DoubleDashAttack(this, doubleDashSettings);
        chargeAttack = new ChargeAttack(this, chargeSettings);
        dynamiteAttack = new DynamiteAttack(this, dynamiteSettings);
        drillAttack = new DrillAttack(this, drillSettings);
    }

    //비워둠으로써 피격 경직 제거
    protected override void ShieldAndHitReaction(bool hadShield)
    {
    }

    //===================== 점프, 착지 =====================
    //점프 준비
    internal void PlayJumpPrepareAnimation()
    {
        PlayAnimation(jumpPrepareAnimation);
    }
    
    //점프 시작
    internal void StartJump(float duration)
    {
        //시작 위치 저장 및 타이머 초기화
        jumpStartPosition = rb.position;
        aerialStartPosition = rb.position;
        aerialTimer = 0f;
        aerialDuration = Mathf.Max(0.01f, duration);

        //스프라이트 아래쪽까지 화면 밖으로 나가도록 목표 높이 계산
        float bottomOffset = rb.position.y - spriteRenderer.bounds.min.y;
        float cameraTop = playerCamera.transform.position.y + playerCamera.orthographicSize;
        float targetY = cameraTop + bottomOffset + offscreenMargin;
        
        //최종 도착 좌표
        aerialEndPosition = new Vector2(rb.position.x, targetY);
        
        //그림자 위치, 알파값 초기화
        if (shadow != null)
        {
            shadow.ResetPosition();
            shadow.SetOpacity(1f);
        }

        PlayJumpLandingEffect();
        PlayAnimation(jumpAnimation);
    }

    //점프 진행
    internal bool ExecuteJump() => ExecuteAerialMovement(false);

    //착지 시작
    internal void StartLanding(Vector2 landingPosition, float duration)
    {
        aerialStartPosition = rb.position;
        aerialEndPosition = landingPosition;
        aerialTimer = 0f;
        aerialDuration = Mathf.Max(0.01f, duration);

        PlayAnimation(landingAnimation);
    }

    //착지 진행
    internal bool ExecuteLanding() => ExecuteAerialMovement(true);
    
    //착지 마무리 애니메이션
    internal void PlayLandingEndAnimation()
    {
        PlayAnimation(landingEndAnimation);
    }
    
    //점프 및 착지 이펙트 재생
    private void PlayJumpLandingEffect()
    {
        jumpLandingEffect.gameObject.SetActive(true);
    }

    //점프와 착지에 공통으로 사용하는 강제 위치 이동
    private bool ExecuteAerialMovement(bool isLanding)
    {
        //정해진 시간에 맞춰서 시작 -> 도착까지 위치이동
        aerialTimer += Time.fixedDeltaTime;
        float progress = Mathf.Clamp01(aerialTimer / aerialDuration);
        
        rb.position = Vector2.Lerp(aerialStartPosition, aerialEndPosition, progress);

        if (shadow != null)
        {
            //지면에 그림자 고정
            float groundY = isLanding ? aerialEndPosition.y : jumpStartPosition.y;
            shadow.SetAirborneHeight(rb.position.y - groundY);

            //점프 시에는 그림자가 서서히 사라지며, 착지 시에는 그림자가 서서히 나타남
            shadow.SetOpacity(isLanding ? progress : 1f - progress);

            //착지 완료 시 그림자 초기화
            if (isLanding && progress >= 1f)
            {
                shadow.ResetPosition();
                shadow.SetOpacity(1f);
            }
        }
        
        //착지 완료 시 이펙트 출력
        if (isLanding && progress >= 1f)
            PlayJumpLandingEffect();

        return progress >= 1f;
    }

    //===================== 공격 패턴 선택 =====================
    //공격 가능 여부
    public override bool CanAttack()
    {
        return target != null && base.CanAttack() && TotalAttackWeight > 0;
    }

    //가중치에 따른 랜덤 패턴 선택
    private BossAttackPattern SelectAttackPattern()
    {
        int roll = Random.Range(0, TotalAttackWeight);

        //랜덤값이 가중치보다 적은지 확인하고, 아니라면 그 가중치를 빼주면서 가중치 비교 진행
        //최종적으로 가중치에 맞춰서 5개 패턴중 하나 확정
        if (roll < singleWeight) return singleAttack;
        roll -= singleWeight;
        if (roll < doubleWeight) return doubleAttack;
        roll -= doubleWeight;
        if (roll < chargeWeight) return chargeAttack;
        roll -= chargeWeight;
        if (roll < dynamiteWeight) return dynamiteAttack;

        return drillAttack;
    }

    //===================== 공격 로직 =====================
    //공격 준비 시작
    public override void StartAttackPrepare()
    {
        //패턴 선택 및 해당 패턴의 준비 시작
        currentAttack = SelectAttackPattern();
        currentAttack?.StartAttackPrepare();
    }

    //공격은 준비와 공격 단계로 나눠짐
    //준비 시작 -> 준비 진행 -> 공격 시작 -> 공격 진행 -> 상태 전환
    //시작은 StartAttackPrepare에서 시작함

    //공격 준비 애니메이션
    public override void PlayAttackPrepareAnimation() => currentAttack?.PlayAttackPrepareAnimation();

    //공격 준비 애니메이션 이벤트
    public override void ShowAttackWarning() => currentAttack?.ShowAttackWarning();

    //공격 준비 진행 및 공격 전환
    //true 반환 시 공격 시작
    public override bool ExecuteAttackPrepare() => currentAttack?.ExecuteAttackPrepare() ?? false;

    //공격 애니메이션
    public override void PlayAttackAnimation() => currentAttack?.PlayAttackAnimation();

    //공격 시작
    public override void StartAttack()
    {
        base.StartAttack();
        currentAttack?.StartAttack();
    }

    //공격 진행 및 공격 종료 전환
    //true 반환 시 공격 종료, 후딜 적용 및 Idle 전환
    public override bool ExecuteAttack() => currentAttack?.ExecuteAttack() ?? false;

    //공격 애니메이션 종료 이벤트
    public override void AttackEnd() => currentAttack?.AttackEnd();

    //공격 후딜레이 지정
    public override void ApplyAttackEndDelay()
    {
        currentAttack?.ApplyAttackEndDelay();
        currentAttack = null;
    }

    //행동 캔슬 메소드
    protected override void CancelCurrentAction()
    {
        currentAttack?.Cancel();
        currentAttack = null;
    }
    
    //드릴 이동 방향에 맞춰 좌우 반전
    internal void SetSpriteDirection(bool faceLeft)
    {
        if (spriteRenderer.flipX == faceLeft) return;

        spriteRenderer.flipX = faceLeft;
        if (shadow != null) shadow.FlipXPosition();
    }
    
    //===================== 기즈모 =====================
    private void OnDrawGizmosSelected()
    {
        //대시 공격 기즈모
        if (showDashAttackGizmos && singleDashSettings != null)
        {
            //공격 시작 사거리
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, singleDashSettings.attackDistance);

            //공격 예고 범위 (실제 공격 방향은 준비 시작 시 결정됨)
            Quaternion direction = singleDashSettings.attackWarning != null
                ? singleDashSettings.attackWarning.rotation
                : transform.rotation;
            Vector3 warningPosition = transform.TransformPoint(singleDashSettings.warningOffset);
            Vector3 warningScale = singleDashSettings.attackWarning != null
                ? singleDashSettings.attackWarning.lossyScale
                : transform.lossyScale;
            Matrix4x4 previousMatrix = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(warningPosition, direction, warningScale);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(
                new Vector3(0f, singleDashSettings.warningEndLength * 0.5f, 0f),
                new Vector3(singleDashSettings.warningWidth, singleDashSettings.warningEndLength, 0f));

            //돌진 이동 거리
            Gizmos.matrix = Matrix4x4.TRS(transform.position, direction, Vector3.one);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, Vector3.up * singleDashSettings.attackMoveDistance);
            Gizmos.DrawWireSphere(Vector3.up * singleDashSettings.attackMoveDistance, 0.1f);

            //실제 OverlapBoxAll의 중심/회전/크기와 동일한 공격 판정
            if (singleDashSettings.attackHitbox != null)
            {
                Transform hitboxTransform = singleDashSettings.attackHitbox.transform;
                Vector3 hitboxCenter = hitboxTransform.TransformPoint(singleDashSettings.attackHitboxOffset);
                Gizmos.matrix = Matrix4x4.TRS(hitboxCenter, hitboxTransform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, singleDashSettings.attackHitboxSize);
            }

            Gizmos.matrix = previousMatrix;
        }

        //차지 공격 기즈모
        if (showChargeAttackGizmos && chargeSettings != null)
        {
            //공격 시작 사거리
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chargeSettings.attackDistance);

            //예고선 최종 범위
            Gizmos.color = Color.cyan;
            Vector3 warningCenter = transform.TransformPoint(chargeSettings.warningOffset);
            Gizmos.DrawWireCube(warningCenter, chargeSettings.warningSize);

            //차지 공격 범위
            if (chargeSettings.attackHitbox != null)
            {
                Transform hitboxTransform = chargeSettings.attackHitbox.transform;
                Vector3 hitboxCenter = hitboxTransform.TransformPoint(chargeSettings.attackHitboxOffset);
                Matrix4x4 previousMatrix = Gizmos.matrix;

                Gizmos.matrix = Matrix4x4.TRS(hitboxCenter, hitboxTransform.rotation, Vector3.one);
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(Vector3.zero, chargeSettings.attackHitboxSize);

                Gizmos.matrix = previousMatrix;
            }
        }
    }
}