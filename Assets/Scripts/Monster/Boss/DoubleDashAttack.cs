using System;
using UnityEngine;

public sealed class DoubleDashAttack : BossAttackPattern
{
    [Serializable]
    public class Settings
    {
        [Header("실제 공격 관련")]
        public AttackHitbox attackHitbox;
        public Animator swingAnimator; //SwingFX 자식 오브젝트의 Animator
        public Vector2 attackHitboxSize = new(1f, 1f);
        public Vector2 attackHitboxOffset = new(0f, 1f);
        
        public float attackDistance = 3f; //공격 시작 사거리
        public float firstAttackMoveDistance = 3f; //첫 번째 돌진 거리
        [Min(0.01f)] public float firstAttackDuration = 0.3f;
        [Min(0.01f)] public float firstPrepareDuration = 0.5f;
        [Min(0f)] public float transitionDelay = 0.2f; //첫 돌진과 두 번째 준비 사이 대기
        public float secondAttackMoveDistance = 4.5f; //두 번째 돌진 거리
        [Min(0.01f)] public float secondAttackDuration = 0.3f;
        [Min(0.01f)] public float secondPrepareDuration = 0.4f;
        public float attackEndDelay = 1.5f; //공격 후 딜레이

        [Header("애니메이션")]
        public string prepareAnimation = "AttackPrepare";
        public string attackAnimation = "Attack";
        public string secondPrepareAnimation = "AttackPrepare";
        public string secondAttackAnimation = "Attack";
        public string firstSwingAnimation = "Swing1";
        public string secondSwingAnimation = "Swing2";
        public Vector2 swingOffset = Vector2.zero; //스윙 이펙트 위치 보정

        [Header("공격 범위 표시 선")]
        public Transform attackWarning;
        public Vector2 warningOffset = Vector2.zero;
        public AttackWarningLine leftOuter;
        public AttackWarningLine leftInner;
        public AttackWarningLine rightInner;
        public AttackWarningLine rightOuter;

        [Header("공격 범위 표시 설정")]
        public float warningWidth = 1.5f;
        public float innerStartOffset = 0.7f;
        public float outerStartOffset = 0.1f;
        public float warningStartLength = 1.5f;
        public float warningEndLength = 3f;
    }

    private enum AttackPhase
    {
        FirstDash,
        Transition,
        SecondPrepare,
        SecondDash
    }

    private readonly Settings settings;
    private AttackPhase attackPhase;
    private float attackPrepareTimer;
    private float attackTimer;
    private Vector2 attackDirection;
    private Vector2 attackStartPosition;
    private Vector2 attackEndPosition;
    private bool isPreparing; //실제 준비 진행중인지 확인, 사거리로 인한 준비 중 이동 상태인지 아닌지 체크

    public DoubleDashAttack(GodfatherMole owner, Settings settings) : base(owner)
    {
        this.settings = settings;
    }

    //===================== 2중 돌진 =====================
    //공격 준비 시작
    public override void StartAttackPrepare()
    {
        isPreparing = false;
        settings.swingAnimator.gameObject.SetActive(false);

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

        UpdateAttackDirection();
        SetupAttackWarning();
    }

    //공격 방향 갱신
    private void UpdateAttackDirection()
    {
        owner.UpdateSpriteDirection();

        attackDirection = ((Vector2)owner.AttackTarget.position - owner.AttackRigidbody.position).normalized;

        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        //공격 선과 판정, 이펙트의 방향 통일
        settings.attackWarning.rotation = rotation;
        settings.attackHitbox.transform.rotation = rotation;
        UpdateEffectTransform(settings.swingAnimator.transform, rotation, settings.swingOffset);
    }

    //공격 준비 애니메이션
    public override void PlayAttackPrepareAnimation() => owner.PlayAnimation(isPreparing ? settings.prepareAnimation : "Move");

    //공격 준비 진행
    public override bool ExecuteAttackPrepare()
    {
        if (!isPreparing)
        {
            if (!ApproachAttackRange(settings.attackDistance)) return false;

            SetupAttackPrepare();
            PlayAttackPrepareAnimation();
        }

        //준비 중 플레이어 방향 추적
        UpdateAttackDirection();

        attackPrepareTimer += Time.fixedDeltaTime;
        float progress = Mathf.Clamp01(attackPrepareTimer / settings.firstPrepareDuration);
        UpdateAttackWarning(progress);

        return progress >= 1f;
    }

    //공격 애니메이션
    public override void PlayAttackAnimation() => owner.PlayAnimation(settings.attackAnimation);

    //공격 시작
    public override void StartAttack()
    {
        attackPhase = AttackPhase.FirstDash;
        StartDash(settings.firstAttackMoveDistance, settings.firstSwingAnimation);
    }

    //공격 진행
    public override bool ExecuteAttack()
    {
        switch (attackPhase)
        {
            case AttackPhase.FirstDash:
                if (!ExecuteDash(settings.firstAttackDuration)) return false;

                ClearAttackVisuals();
                attackTimer = 0f;
                attackPhase = AttackPhase.Transition;
                return false;

            case AttackPhase.Transition:
                attackTimer += Time.fixedDeltaTime;
                if (attackTimer < settings.transitionDelay) return false;

                //두 번째 준비는 첫 돌진이 끝난 위치에서 바로 시작
                SetupAttackPrepare();
                owner.PlayAnimation(settings.secondPrepareAnimation);
                attackPhase = AttackPhase.SecondPrepare;
                return false;

            case AttackPhase.SecondPrepare:
                //두 번째 공격 준비 중에도 플레이어 방향 추적
                UpdateAttackDirection();
                attackPrepareTimer += Time.fixedDeltaTime;
                float progress = Mathf.Clamp01(attackPrepareTimer / settings.secondPrepareDuration);
                UpdateAttackWarning(progress);
                if (progress < 1f) return false;

                owner.PlayAnimation(settings.secondAttackAnimation);
                StartDash(settings.secondAttackMoveDistance, settings.secondSwingAnimation);
                attackPhase = AttackPhase.SecondDash;
                return false;

            case AttackPhase.SecondDash:
                if (!ExecuteDash(settings.secondAttackDuration)) return false;

                ClearAttackVisuals();
                return true;
        }

        return false;
    }

    //돌진 시작
    private void StartDash(float moveDistance, string swingAnimation)
    {
        attackTimer = 0f;
        attackStartPosition = owner.AttackRigidbody.position;
        attackEndPosition = attackStartPosition + attackDirection * moveDistance;

        //공격 중에는 안내선을 시작 위치에 고정
        settings.attackWarning.SetParent(null, true);

        settings.swingAnimator.gameObject.SetActive(true);
        settings.swingAnimator.Play(swingAnimation, 0, 0f);
        settings.attackHitbox.ResetHitTargets();
    }

    //돌진 진행
    private bool ExecuteDash(float duration)
    {
        attackTimer += Time.fixedDeltaTime;
        float progress = Mathf.Clamp01(attackTimer / duration);

        Vector2 attackPosition = Vector2.Lerp(attackStartPosition, attackEndPosition, progress);
        owner.AttackRigidbody.MovePosition(attackPosition);
        DetectAttackTarget();

        settings.leftOuter.SetFadeProgress(progress);
        settings.leftInner.SetFadeProgress(progress);
        settings.rightInner.SetFadeProgress(progress);
        settings.rightOuter.SetFadeProgress(progress);

        return progress >= 1f;
    }

    //공격 후딜레이 지정
    public override void ApplyAttackEndDelay() => owner.StartIdleLock(settings.attackEndDelay);

    //사망 등으로 공격이 중단될 때 정리
    public override void Cancel() => ClearAttackVisuals();

    //===================== 공격 세부 로직 =====================
    //공격 범위 표시 설정
    private void SetupAttackWarning()
    {
        settings.leftOuter.transform.localRotation = Quaternion.identity;
        settings.leftInner.transform.localRotation = Quaternion.identity;
        settings.rightInner.transform.localRotation = Quaternion.identity;
        settings.rightOuter.transform.localRotation = Quaternion.identity;
        
        float halfWidth = settings.warningWidth * 0.5f;
        Vector3 leftEnd = new(-halfWidth, 0f, 0f);
        Vector3 rightEnd = new(halfWidth, 0f, 0f);
        Vector3 leftOuterStart = leftEnd + Vector3.left * settings.outerStartOffset;
        Vector3 leftInnerStart = leftEnd + Vector3.right * settings.innerStartOffset;
        Vector3 rightInnerStart = rightEnd + Vector3.left * settings.innerStartOffset;
        Vector3 rightOuterStart = rightEnd + Vector3.right * settings.outerStartOffset;

        settings.leftOuter.Setup(leftOuterStart, leftEnd, settings.warningStartLength, settings.warningEndLength);
        settings.leftInner.Setup(leftInnerStart, leftEnd, settings.warningStartLength, settings.warningEndLength);
        settings.rightInner.Setup(rightInnerStart, rightEnd, settings.warningStartLength, settings.warningEndLength);
        settings.rightOuter.Setup(rightOuterStart, rightEnd, settings.warningStartLength, settings.warningEndLength);
    }

    //공격 범위 표시 진행
    private void UpdateAttackWarning(float progress)
    {
        settings.leftOuter.SetProgress(progress);
        settings.leftInner.SetProgress(progress);
        settings.rightInner.SetProgress(progress);
        settings.rightOuter.SetProgress(progress);
    }

    //돌진 중 적을 한 번만 타격
    private void DetectAttackTarget()
    {
        AttackData attackData = new(owner.AttackDamage, attackDirection, false, ElementType.Physical, DamageSource.Enemy);
        settings.attackHitbox.DetectTargets(settings.attackHitboxSize, settings.attackHitboxOffset, attackData, null);
    }

    //정상 종료와 강제 취소에 공통으로 사용하는 정리
    private void ClearAttackVisuals()
    {
        settings.leftOuter.Hide();
        settings.leftInner.Hide();
        settings.rightInner.Hide();
        settings.rightOuter.Hide();
        settings.swingAnimator.gameObject.SetActive(false);

        settings.attackWarning.SetParent(owner.transform, true);
        settings.attackWarning.localPosition = settings.warningOffset;
    }
}
