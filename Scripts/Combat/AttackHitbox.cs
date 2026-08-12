using UnityEngine;
using System.Collections.Generic;

public class AttackHitbox : MonoBehaviour
{
    [Header("공격 판정 대상")]
    [SerializeField] private LayerMask targetLayer;
    
    //공격 적중 대상
    private readonly HashSet<IDamageable> hitTargets = new();

    //공격 판정 시작
    public void ResetHitTargets()
    {
        hitTargets.Clear();
    }
    
    //공격 대상 확인
    public void DetectTargets(
        Vector2 hitboxSize,
        Vector2 hitboxOffset,
        AttackData attackData,
        System.Action<Vector2, DamageResult> onHit)
    {
        Vector2 center = transform.TransformPoint(hitboxOffset);

        Collider2D[] targets = Physics2D.OverlapBoxAll(
            center,
            hitboxSize,
            transform.eulerAngles.z,
            targetLayer
        );

        foreach (Collider2D target in targets)
        {
            IDamageable damageable = target.GetComponentInParent<IDamageable>();

            //공격 가능한 대상이 아니거나 이미 적중한 대상이라면 제외
            if (damageable == null || hitTargets.Contains(damageable))
                continue;

            hitTargets.Add(damageable);
            
            //실제 타격 위치
            Vector2 hitPosition = target.ClosestPoint(center);
            
            //공격 결과
            DamageResult result = damageable.TakeDamage(attackData);
            
            //공격측에 타격 위치 및 공격 결과 전달
            onHit?.Invoke(hitPosition, result);
        }
    }
    
    public void DrawHitboxGizmo(Vector2 hitboxSize, Vector2 hitboxOffset)
    {
        Gizmos.color = Color.red;

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Gizmos.DrawWireCube(hitboxOffset, hitboxSize);

        Gizmos.matrix = previousMatrix;
    }
}