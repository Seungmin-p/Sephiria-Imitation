using UnityEngine;

public class BoomMole : RangedMonster
{
    [Header("실제 공격 관련")]
    [SerializeField] GameObject attackEffect;
    [SerializeField] BoomMoleProjectile projectilePrefab;

    [SerializeField] int projectileCount = 4;
    [SerializeField] float projectileSpeed = 5f;
    [SerializeField] float projectileLifetime = 3f;

    [SerializeField] Vector2 projectilePositionRandomRange = new Vector2(0.1f, 0.05f);
    [SerializeField] float projectileAngleRandomRange = 10f;
    [SerializeField] float projectileSpeedRandomRange = 1.5f;

    private Animator attackEffectAnimator;
    private Vector3 attackEffectLocalPosition;

    protected override void Awake()
    {
        base.Awake();

        if (attackEffect != null)
        {
            attackEffectAnimator = attackEffect.GetComponent<Animator>();
            attackEffectLocalPosition = attackEffect.transform.localPosition;
        }
    }

    //공격 이펙트 x축 보정
    private void UpdateAttackEffectPosition()
    {
        if (attackEffect == null) return;

        Vector3 position = attackEffectLocalPosition;
        position.x = Mathf.Abs(position.x) * (spriteRenderer.flipX ? -1f : 1f);

        attackEffect.transform.localPosition = position;
    }

    protected override void UpdateAttackPreparePosition()
    {
        UpdateAttackEffectPosition();
    }

    protected override void OnStartAttackPrepare()
    {
        //이펙트 활성화
        if (attackEffect != null)
            attackEffect.SetActive(false);
    }

    protected override void OnShowAttackWarning()
    {
        if (attackEffect == null) return;

        attackEffect.SetActive(true);

        if (attackEffectAnimator != null)
            attackEffectAnimator.Play("BoomPrepare_1");
    }

    public override void PlayAttackAnimation()
    {
        PlayAnimation("Idle");
    }

    protected override void OnStartAttack()
    {
        if (attackEffectAnimator != null)
            attackEffectAnimator.Play("BoomFire");

        SpawnProjectiles();
    }
    
    //총알 생성 즉시 공격 종료
    public override bool ExecuteAttack()
    {
        return true;
    }

    //투사체 생성
    private void SpawnProjectiles()
    {
        if (projectilePrefab == null) return;

        //기본 위치
        Vector2 basePosition = attackEffect != null
            ? attackEffect.transform.position
            : transform.position;

        //공격 방향에 수직인 방향, 좌우로 총알을 흩뿌리기 위해 사용
        Vector2 sideDirection = new Vector2(-attackDirection.y, attackDirection.x);

        for (int i = 0; i < projectileCount; i++)
        {
            //발사 위치 랜덤 오차
            float sideOffset = Random.Range(-projectilePositionRandomRange.x, projectilePositionRandomRange.x);
            float forwardOffset = Random.Range(-projectilePositionRandomRange.y, projectilePositionRandomRange.y);

            //랜덤값 적용해서 스폰 진행
            Vector2 spawnPosition =
                basePosition +
                sideDirection * sideOffset +
                attackDirection * forwardOffset;

            //발사 각도 랜덤 오차, 최종 각도 도출
            float angleOffset = Random.Range(-projectileAngleRandomRange, projectileAngleRandomRange);
            Vector2 projectileDirection = Quaternion.Euler(0f, 0f, angleOffset) * (Vector3)attackDirection;

            //속도 랜덤 오차, 최종 속도 도출
            float speed = projectileSpeed + Random.Range(-projectileSpeedRandomRange, projectileSpeedRandomRange);

            //총알 생성
            BoomMoleProjectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            //총알에 각도, 속도, 데미지, 유지시간 부여
            projectile.Initialize(projectileDirection, speed, attackDamage, projectileLifetime);
        }
    }

    //공격 캔슬 당할 시
    protected override void CancelCurrentAction()
    {
        base.CancelCurrentAction();

        if (attackEffect != null)
            attackEffect.SetActive(false);
    }
}
