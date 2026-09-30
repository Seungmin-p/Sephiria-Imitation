using UnityEngine;

public class BoomMoleProjectile : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D col;
    [SerializeField] Animator animator;
    [SerializeField] LayerMask impactLayer;

    private Vector2 moveDirection;
    private float moveSpeed;
    private float damage;
    private float lifetime;
    private float lifetimeTimer;

    private bool isImpacted;

    //생성된 투사체 초기값 설정
    public void Initialize(Vector2 direction, float speed, float damage, float lifetime)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        this.damage = damage;
        this.lifetime = lifetime;

        lifetimeTimer = 0f;
        isImpacted = false;
    }

    private void FixedUpdate()
    {
        //아직 터지기 전이라면
        if (isImpacted) return;

        lifetimeTimer += Time.fixedDeltaTime;

        //최대 유지시간이 지나면 파괴 이펙트
        if (lifetimeTimer >= lifetime)
        {
            Impact();
            return;
        }

        //투사체 이동
        Vector2 nextPosition = rb.position + moveDirection * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isImpacted) return;

        //충돌 대상으로 지정하지 않은 레이어라면 패스
        if ((impactLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        //데미지를 받을 수 있는 대상인지 확인
        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            AttackData attackData = new(
                damage,
                moveDirection,
                false,
                ElementType.Physical,
                DamageSource.Enemy
            );

            DamageResult result = damageable.TakeDamage(attackData);

            //무시된 공격이 아니라면 기존 타격 이벤트 전달
            if (result.hitResultType != DamageResult.HitResultType.Ignored)
            {
                Vector2 hitPosition = other.ClosestPoint(rb.position);
                AttackHitbox.NotifyDamageResolved(hitPosition, moveDirection, result);
            }
        }

        //데미지 가능 여부와 상관없이 충돌했다면 투사체 파괴 처리
        Impact();
    }

    private void Impact()
    {
        if (isImpacted) return;

        isImpacted = true;

        //추가 충돌 방지
        col.enabled = false;

        //파괴 이펙트 재생
        animator.Play("Boom_Mole_Bullet_FX");
    }
}