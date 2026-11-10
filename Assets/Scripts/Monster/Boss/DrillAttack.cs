using System;
using UnityEngine;

public sealed class DrillAttack : BossAttackPattern
{
    [Serializable]
    public class Settings
    {
        [Header("점프, 착지")]
        [Min(0f)] public float jumpPrepareDuration = 0.6f;
        [Min(0.01f)] public float jumpDuration = 0.4f;
        [Min(0.01f)] public float landingDuration = 0.4f;
        public float attackEndDelay = 1.5f;

        [Header("드릴 돌진")]
        public BoxCollider2D attackArea; //전투 구역
        public ParticleSystem drillWindParticle; //드릴 파티클
        public ParticleSystemRenderer drillWindRenderer; //드릴 파티클 렌더러
        public SpriteRenderer drillWallEffectPrefab; //드릴 벽 진입 이펙트
        [Min(0f)] public float wallEffectOffset = 1f; //드릴 벽 이펙트 오프셋
        [Min(1)] public int dashCount = 3;
        [Min(0f)] public float dashPrepareDuration = 1.5f;
        [Min(0.01f)] public float dashDuration = 0.5f;
        [Min(0f)] public float offscreenMargin = 5f;
        public string drillAnimation = "Drill";

        [Header("실제 공격 판정")]
        public AttackHitbox attackHitbox;
        public Vector2 attackHitboxSize = new(2f, 1.5f);
        public Vector2 attackHitboxOffset = Vector2.zero;

        [Header("공격 안내선")]
        public Transform attackWarning;
        public AttackWarningLine topLine;
        public AttackWarningLine bottomLine;
        [Min(0.01f)] public float warningLength = 10f;
        [Min(0f)] public float warningWidth = 1.5f;
    }

    private enum Phase { Jump, DashPrepare, Dash, Landing, LandingEnd }

    private readonly Settings settings;
    private Phase phase;
    private float jumpPrepareTimer;
    private float phaseTimer;
    private float jumpOffscreenY;
    private int completedDashes;
    private bool fromLeft;
    private bool isAttackEnd;

    private Vector2 dashStartPosition;
    private Vector2 dashEndPosition;
    private Vector2 dashDirection;

    public DrillAttack(GodfatherMole owner, Settings settings) : base(owner)
    {
        this.settings = settings;
    }

    //===================== 드릴 돌진 =====================
    //점프 준비 시작
    public override void StartAttackPrepare() => jumpPrepareTimer = 0f;

    //점프 준비 애니메이션
    public override void PlayAttackPrepareAnimation() => owner.PlayJumpPrepareAnimation();

    //점프 준비 진행
    public override bool ExecuteAttackPrepare()
    {
        jumpPrepareTimer += Time.fixedDeltaTime;
        return jumpPrepareTimer >= settings.jumpPrepareDuration;
    }

    //공격 시작
    public override void StartAttack()
    {
        completedDashes = 0;
        fromLeft = true;
        isAttackEnd = false;
        phase = Phase.Jump;
        owner.StartJump(settings.jumpDuration);
    }

    //공격 진행
    public override bool ExecuteAttack()
    {
        switch (phase)
        {
            case Phase.Jump:
                if (!owner.ExecuteJump()) return false;

                jumpOffscreenY = owner.AttackRigidbody.position.y;
                
                //대시 공격 준비 후 DashPrepare 상태로 변경
                StartDashPrepare();
                return false;

            case Phase.DashPrepare:
                phaseTimer += Time.fixedDeltaTime;
                float prepareProgress = Mathf.Clamp01(phaseTimer / Mathf.Max(0.01f, settings.dashPrepareDuration));
                settings.topLine.SetProgress(prepareProgress);
                settings.bottomLine.SetProgress(prepareProgress);

                if (phaseTimer < settings.dashPrepareDuration) return false;
                StartDash();
                return false;

            case Phase.Dash:
                phaseTimer += Time.fixedDeltaTime;
                float dashProgress = Mathf.Clamp01(phaseTimer / Mathf.Max(0.01f, settings.dashDuration));
                
                //이동 전후 X좌표 확인
                float previousX = owner.AttackRigidbody.position.x;
                owner.AttackRigidbody.position = Vector2.Lerp(dashStartPosition, dashEndPosition, dashProgress);
                float currentX = owner.AttackRigidbody.position.x;

                //벽 통과 시, 방향에 맞춰 해당 벽에 이펙트 출력
                Bounds bounds = settings.attackArea.bounds;
                if (CrossedWall(previousX, currentX, bounds.min.x))
                    SpawnDrillWallEffect(bounds.min.x, true);
                if (CrossedWall(previousX, currentX, bounds.max.x))
                    SpawnDrillWallEffect(bounds.max.x, false);
                
                DetectAttackTarget();

                if (dashProgress < 1f) return false;
                
                //돌진이 완료되면 파티클 정지, 이미 출력중인 파티클은 마지막까지 재생
                settings.drillWindParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

                //대시 완료 카운트를 늘리고, 카운트가 부족하면 방향 바꿔서 추가 진행
                completedDashes++;
                if (completedDashes < settings.dashCount)
                {
                    fromLeft = !fromLeft;
                    //대시 공격 준비 후 DashPrepare 상태로 변경
                    StartDashPrepare();
                    return false;
                }

                //마지막 공격 이후 화면 위로 복귀한 뒤 기존 착지 동작 실행
                ClearAttackWarning();
                Vector2 landingPosition = owner.JumpStartPosition;
                owner.AttackRigidbody.position = new Vector2(landingPosition.x, jumpOffscreenY);
                owner.UpdateSpriteDirection();
                owner.StartLanding(landingPosition, settings.landingDuration);
                phase = Phase.Landing;
                return false;

            case Phase.Landing:
                if (!owner.ExecuteLanding()) return false;

                owner.PlayLandingEndAnimation();
                phase = Phase.LandingEnd;
                return false;

            case Phase.LandingEnd:
                return isAttackEnd;
        }

        return false;
    }
    
    //드릴이 벽의 X좌표를 통과했는지 확인
    private bool CrossedWall(float previousX, float currentX, float wallX)
    {
        return fromLeft
            ? previousX < wallX && currentX >= wallX
            : previousX > wallX && currentX <= wallX;
    }
    
    //드릴 벽 진입 및 탈출 이펙트
    private void SpawnDrillWallEffect(float wallX, bool isLeftWall)
    {
        float effectX = wallX + (isLeftWall ? settings.wallEffectOffset : -settings.wallEffectOffset);
        Vector3 position = new(effectX, dashStartPosition.y, settings.drillWallEffectPrefab.transform.position.z);

        SpriteRenderer effect = UnityEngine.Object.Instantiate(settings.drillWallEffectPrefab, position, Quaternion.identity);
        effect.flipX = isLeftWall;
    }

    //착지 마무리 애니메이션 종료 이벤트
    public override void AttackEnd()
    {
        if (phase == Phase.LandingEnd)
            isAttackEnd = true;
    }

    //공격 후딜레이 지정
    public override void ApplyAttackEndDelay()
    {
        ClearAttackWarning();
        owner.StartIdleLock(settings.attackEndDelay);
    }

    //사망 등으로 공격이 중단될 때 정리
    public override void Cancel() 
    {
        ClearAttackWarning();
        settings.drillWindParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    //===================== 공격 세부 로직 =====================
    //대시 준비, 공격 시작 ~ 끝 위치, 공격할 Y축과 시작할 방향, 안내선 세팅 진행
    private void StartDashPrepare()
    {
        //공격 범위와 카메라 범위의 절반을 이용해서 시작 ~ 끝 X 좌표 구하기
        Bounds bounds = settings.attackArea.bounds;
        float halfCameraWidth = owner.AttackCamera.orthographicSize * owner.AttackCamera.aspect;
        float leftX = bounds.min.x - halfCameraWidth - settings.offscreenMargin;
        float rightX = bounds.max.x + halfCameraWidth + settings.offscreenMargin;
        
        //드릴 공격의 Y좌표는 타겟의 위치 고정
        float attackY = owner.AttackTarget.position.y;

        //fromLeft에 따라서 공격 진행 방향 확보
        dashStartPosition = new Vector2(fromLeft ? leftX : rightX, attackY);
        dashEndPosition = new Vector2(fromLeft ? rightX : leftX, attackY);
        dashDirection = fromLeft ? Vector2.right : Vector2.left;

        //공격 시작 위치 고정
        owner.AttackRigidbody.position = dashStartPosition;
        
        //스프라이트 및 안내선 방향 확정
        owner.SetSpriteDirection(!fromLeft);
        SetupAttackWarning(fromLeft ? bounds.min.x : bounds.max.x, attackY);

        //다음 단계로 진입
        phaseTimer = 0f;
        phase = Phase.DashPrepare;
    }

    //실제 드릴 돌진 시작
    private void StartDash()
    {
        //선 숨김처리 및 공격 히트박스 초기화
        ClearAttackWarning();
        settings.attackHitbox.transform.rotation = Quaternion.identity;
        settings.attackHitbox.ResetHitTargets();
        
        owner.PlayAnimation(settings.drillAnimation);
        
        settings.drillWindRenderer.flip = new Vector3(fromLeft ? 0f : 1f, 0f, 0f);
        settings.drillWindParticle.Play();

        phaseTimer = 0f;
        phase = Phase.Dash;
    }

    //안내선을 진입 벽에 고정하고 수평으로 늘어날 준비
    private void SetupAttackWarning(float wallX, float attackY)
    {
        Transform warning = settings.attackWarning;
        
        settings.topLine.transform.localRotation = Quaternion.identity;
        settings.bottomLine.transform.localRotation = Quaternion.identity;

        warning.SetParent(null, true);        
        warning.position = new Vector3(wallX, attackY, warning.position.z);
        warning.rotation = Quaternion.Euler(0f, 0f, fromLeft ? -90f : 90f);

        float halfWidth = settings.warningWidth * 0.5f;

        Vector3 topPosition = new(-halfWidth, 0f, 0f);
        Vector3 bottomPosition = new(halfWidth, 0f, 0f);

        settings.topLine.Setup(topPosition, Vector3.zero, settings.warningLength, settings.warningLength);
        settings.bottomLine.Setup(bottomPosition, Vector3.zero, settings.warningLength, settings.warningLength);
    }

    //한 번의 횡단에서 대상당 한 번만 타격
    private void DetectAttackTarget()
    {
        AttackData attackData = new(owner.AttackDamage, dashDirection, false, ElementType.Physical, DamageSource.Enemy);
        settings.attackHitbox.DetectTargets(settings.attackHitboxSize, settings.attackHitboxOffset, attackData, null);
    }

    //안내선 숨김
    private void ClearAttackWarning()
    {
        settings.topLine.Hide();
        settings.bottomLine.Hide();
        settings.attackWarning.SetParent(owner.transform, true);
    }
}