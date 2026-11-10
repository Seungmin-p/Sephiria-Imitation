using System;
using UnityEngine;

public sealed class ChargeAttack : BossAttackPattern
{
    [Serializable]
    public class Settings
    {
        [Header("실제 공격 관련")]
        public AttackHitbox attackHitbox;
        public Vector2 attackHitboxSize = new(6f, 6f);
        public Vector2 attackHitboxOffset = Vector2.zero;
        
        public float attackDistance = 2.5f; //공격 시작 사거리
        [Min(0.01f)] public float attackPrepareDuration = 1.7f;
        [Min(0.01f)] public float attackDuration = 0.3f;
        public float attackEndDelay = 1.5f; //공격 후 딜레이

        [Header("애니메이션, 이펙트")]
        public string prepareAnimation = "AttackPrepare";
        public string attackAnimation = "ChargeAttack";
        
        public Animator chargeAnimator; //ChargeFX 자식 오브젝트의 Animator
        public string chargeEffectAnimation = "Charge";

        [Header("공격 범위 표시 선")]
        public Transform attackWarning;
        public Vector2 warningOffset = Vector2.zero;
        public AttackWarningLine topOuter;
        public AttackWarningLine topInner;
        public AttackWarningLine bottomOuter;
        public AttackWarningLine bottomInner;
        public AttackWarningLine leftOuter;
        public AttackWarningLine leftInner;
        public AttackWarningLine rightOuter;
        public AttackWarningLine rightInner;

        [Header("공격 범위 표시 설정")]
        public Vector2 warningSize = new(7f, 7f);
        public float warningLineLength = 1.5f; //선 길이
        public float innerStartOffset = 0.7f;
        public float outerStartOffset = 0f;
    }

    private readonly Settings settings;
    private readonly AttackWarningLine[] warningLines;
    private float attackPrepareTimer;
    private float attackTimer;
    private Vector2 attackDirection;
    private bool isPreparing; //실제 준비 진행중인지 확인, 사거리로 인한 준비 중 이동 상태인지 아닌지 체크\
    private bool isAttackEnd; //공격 모션이 마무리 되는 타이밍에 변경되어서 실제 공격 상태 종료

    public ChargeAttack(GodfatherMole owner, Settings settings) : base(owner)
    {
        this.settings = settings;
        warningLines = new[]
        {
            settings.topOuter, settings.topInner, settings.bottomOuter, settings.bottomInner,
            settings.leftOuter, settings.leftInner, settings.rightOuter, settings.rightInner
        };
    }

    //===================== 차지 광역 공격 =====================
    //공격 준비 시작
    public override void StartAttackPrepare()
    {
        isPreparing = false;
        settings.chargeAnimator.gameObject.SetActive(false);

        if (IsInAttackRange(settings.attackDistance))
            SetupAttackPrepare();
    }

    //실제 공격 준비 설정
    private void SetupAttackPrepare()
    {
        isPreparing = true;
        attackPrepareTimer = 0f;

        //공격 선의 부모 및 위치 설정
        settings.attackWarning.SetParent(owner.transform, true);
        settings.attackWarning.localPosition = settings.warningOffset;
        settings.attackWarning.rotation = Quaternion.identity;

        //차지 공격은 월드 기준 사각형 범위를 사용
        settings.attackHitbox.transform.rotation = Quaternion.identity;
        owner.UpdateSpriteDirection();

        //공격 준비 시작 시 플레이어 방향 확정
        attackDirection = ((Vector2)owner.AttackTarget.position - owner.AttackRigidbody.position).normalized;

        SetupAttackWarning();
    }

    //공격 준비 애니메이션
    public override void PlayAttackPrepareAnimation() => owner.PlayAnimation(isPreparing ? settings.prepareAnimation : "Move");

    //공격 준비 진행
    public override bool ExecuteAttackPrepare()
    {
        //실제 공격 준비를 시작하지 않았다면 사거리 확인 및 이동처리
        if (!isPreparing)
        {
            if (!ApproachAttackRange(settings.attackDistance)) return false;

            SetupAttackPrepare();
            PlayAttackPrepareAnimation();
        }

        attackPrepareTimer += Time.fixedDeltaTime;
        float progress = Mathf.Clamp01(attackPrepareTimer / settings.attackPrepareDuration);

        foreach (AttackWarningLine line in warningLines)
            line.SetProgress(progress);

        return progress >= 1f;
    }

    //공격 애니메이션
    public override void PlayAttackAnimation() => owner.PlayAnimation(settings.attackAnimation);

    //공격 시작
    public override void StartAttack()
    {
        attackTimer = 0f;
        isAttackEnd = false;
        
        settings.attackHitbox.ResetHitTargets();
        
        //플레이어 방향에 맞춰 이펙트 좌우 반전
        Transform effect = settings.chargeAnimator.transform;
        effect.rotation = Quaternion.identity;
        
        Vector3 scale = effect.localScale;
        scale.x = Mathf.Abs(scale.x) * (attackDirection.x < 0f ? -1f : 1f);
        effect.localScale = scale;

        //보스 공격 모션과 이펙트를 동시에 재생
        settings.chargeAnimator.gameObject.SetActive(true);
        settings.chargeAnimator.Play(settings.chargeEffectAnimation, 0, 0f);
    }

    //공격 진행
    public override bool ExecuteAttack()
    {
        //공격 판정 시간 동안만 처리
        if (attackTimer < settings.attackDuration)
        {
            attackTimer += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(attackTimer / settings.attackDuration);

            DetectAttackTarget();

            foreach (AttackWarningLine line in warningLines)
                line.SetFadeProgress(progress);

            //이펙트 정리
            if (progress >= 1f)
                ClearAttackVisuals();
        }

        return isAttackEnd;
    }
    
    public override void AttackEnd() => isAttackEnd = true;

    //공격 후딜레이 지정
    public override void ApplyAttackEndDelay() => owner.StartIdleLock(settings.attackEndDelay);

    //사망 등으로 공격이 중단될 때 정리
    public override void Cancel() => ClearAttackVisuals();

    //===================== 공격 세부 로직 =====================
    //공격 범위 표시 설정
    private void SetupAttackWarning()
    {
        float halfWidth = settings.warningSize.x * 0.5f;
        float halfHeight = settings.warningSize.y * 0.5f;
        Quaternion horizontal = Quaternion.Euler(0f, 0f, 90f);
        Quaternion vertical = Quaternion.identity;

        //몬스터 중심 사각형으로 8개의 선 배치
        SetupWarningPair(settings.topOuter, settings.topInner, new Vector3(0f, halfHeight, 0f), Vector3.up, horizontal);
        SetupWarningPair(settings.bottomOuter, settings.bottomInner, new Vector3(0f, -halfHeight, 0f), Vector3.down, horizontal);
        SetupWarningPair(settings.leftOuter, settings.leftInner, new Vector3(-halfWidth, 0f, 0f), Vector3.left, vertical);
        SetupWarningPair(settings.rightOuter, settings.rightInner, new Vector3(halfWidth, 0f, 0f), Vector3.right, vertical);
    }

    //선 위치 이동
    private void SetupWarningPair(AttackWarningLine outer, AttackWarningLine inner, Vector3 end, Vector3 outward, Quaternion rotation)
    {
        outer.transform.localRotation = rotation;
        inner.transform.localRotation = rotation;

        float length = settings.warningLineLength;
        outer.Setup(end + outward * settings.outerStartOffset, end, length, length, true);
        inner.Setup(end - outward * settings.innerStartOffset, end, length, length, true);
    }

    //공격 판정 진행
    private void DetectAttackTarget()
    {
        AttackData attackData = new(owner.AttackDamage, attackDirection, false, ElementType.Physical, DamageSource.Enemy);
        settings.attackHitbox.DetectTargets(settings.attackHitboxSize, settings.attackHitboxOffset, attackData, null);
    }

    //정상 종료와 강제 취소에 공통으로 사용하는 정리
    private void ClearAttackVisuals()
    {
        foreach (AttackWarningLine line in warningLines)
            line.Hide();

        settings.chargeAnimator.gameObject.SetActive(false);
    }
}
