using UnityEngine;
using System;
using System.Collections.Generic;

public class AttackHitbox : MonoBehaviour
{
    [Header("공격 판정 대상")]
    [SerializeField] private LayerMask targetLayer;
    
    //공격 적중 대상
    private readonly HashSet<IDamageable> hitTargets = new();
    
    //타격 결과 이벤트(타격 위치, 공격 방향, 데미지 결과)
    public static event Action<Vector2, Vector2, DamageResult> OnDamageResolved;

    //공격 판정 시작
    public void ResetHitTargets()
    {
        hitTargets.Clear();
    }
    
    //타격 결과 전달
    public static void NotifyDamageResolved(Vector2 hitPosition, Vector2 hitDirection, DamageResult result)
    {
        OnDamageResolved?.Invoke(hitPosition, hitDirection, result);
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

        bool includesBreakable = (targetLayer.value & LayerMask.GetMask("BreakableObject")) != 0;

        Debug.Log(
            $"공격 판정 실행 / 감지 수: {targets.Length} / " +
            $"BreakableObject 포함: {includesBreakable}"
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
            
            //무시된 공격이라면 이후 처리하지 않음
            if (result.hitResultType == DamageResult.HitResultType.Ignored)
                continue;
            
            //이벤트 호출
            NotifyDamageResolved(hitPosition, attackData.direction, result);
            
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