using System;
using UnityEngine;

public sealed class DynamiteAttack : BossAttackPattern
{
    [Serializable]
    public class Settings
    {
        [Header("점프, 착지")]
        [Min(0f)] public float jumpPrepareDuration = 0.6f;
        [Min(0.01f)] public float jumpDuration = 0.4f;
        [Min(0.01f)] public float landingDuration = 0.4f;
        public float attackEndDelay = 1.5f;

        [Header("다이너마이트 공격")]
        public Dynamite dynamitePrefab;
        public BoxCollider2D dropArea; //다이너마이트가 떨어질 범위
        [Min(1)] public int dynamiteCount = 16;
        [Min(0.01f)] public float spreadDuration = 4f;

        [Header("다이너마이트 낙하 및 폭발")]
        [Min(0f)] public float dropHeight = 5f;
        [Min(0.01f)] public float dropDuration = 0.4f;
        [Min(0f)] public float explosionDelay = 1f; //착지 후 폭발까지의 시간
        public Vector2 explosionSize = new(2.4f, 1.8f);
        public LayerMask damageLayer;
    }

    private enum Phase { Jump, Offscreen, Landing, LandingEnd }

    private readonly Settings settings;
    private Phase phase;
    private float jumpPrepareTimer;
    private float offscreenTimer;
    private int spawnedCount;
    private bool isAttackEnd;

    public DynamiteAttack(GodfatherMole owner, Settings settings) : base(owner)
    {
        this.settings = settings;
    }

    //===================== 대량 다이너마이트 =====================
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

    //공격(점프) 시작
    public override void StartAttack()
    {
        phase = Phase.Jump;
        isAttackEnd = false;
        spawnedCount = 0;
        owner.StartJump(settings.jumpDuration);
    }

    //공격 진행
    public override bool ExecuteAttack()
    {
        switch (phase)
        {
            case Phase.Jump:
                if (!owner.ExecuteJump()) return false;

                phase = Phase.Offscreen;
                offscreenTimer = 0f;
                return false;

            case Phase.Offscreen:
                offscreenTimer += Time.fixedDeltaTime;

                //수량과 시간을 이용해서 간격 계산
                float interval = settings.dynamiteCount > 1
                    ? settings.spreadDuration / (settings.dynamiteCount - 1) : 0f;

                //예정된 다이너마이트 전부 스폰
                while (spawnedCount < settings.dynamiteCount && offscreenTimer >= spawnedCount * interval)
                {
                    SpawnDynamite();
                    spawnedCount++;
                }

                if (offscreenTimer < settings.spreadDuration) return false;

                //다이너마이트 스폰 시간이 끝나면 착지 시작
                owner.StartLanding(owner.JumpStartPosition, settings.landingDuration);
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

    //전투 구역 내 무작위 좌표에 다이너마이트 투하
    private void SpawnDynamite()
    {
        BoxCollider2D area = settings.dropArea;
        Vector2 halfSize = area.size * 0.5f;
        Vector2 localPosition = area.offset + new Vector2(
            UnityEngine.Random.Range(-halfSize.x, halfSize.x),
            UnityEngine.Random.Range(-halfSize.y, halfSize.y));
        Vector2 targetPosition = area.transform.TransformPoint(localPosition);

        Dynamite dynamite = UnityEngine.Object.Instantiate(settings.dynamitePrefab, targetPosition, Quaternion.identity);
        dynamite.InitializeDrop(targetPosition, settings.dropHeight, settings.dropDuration,
            settings.explosionDelay, owner.AttackDamage, settings.explosionSize, settings.damageLayer);
    }

    //착지 마무리 애니메이션 종료 이벤트
    public override void AttackEnd()
    {
        if (phase == Phase.LandingEnd)
            isAttackEnd = true;
    }

    //공격 후딜레이 지정
    public override void ApplyAttackEndDelay() => owner.StartIdleLock(settings.attackEndDelay);
}
