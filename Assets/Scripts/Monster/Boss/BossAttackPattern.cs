using UnityEngine;

public abstract class BossAttackPattern
{
    protected readonly GodfatherMole owner;

    protected BossAttackPattern(GodfatherMole owner)
    {
        this.owner = owner;
    }

    //패턴별 공격 조건, 미구현 패턴 제거용으로, 추후 패턴에 별 다른 조건이 없으면 삭제 예정
    public virtual bool CanUse() => false;

    //===================== 공격 준비 =====================
    public virtual void StartAttackPrepare() { }
    public virtual void PlayAttackPrepareAnimation() { }
    public virtual void ShowAttackWarning() { }
    public virtual bool ExecuteAttackPrepare() => false;

    //===================== 공격 =====================
    public virtual void PlayAttackAnimation() { }
    public virtual void StartAttack() { }
    public virtual bool ExecuteAttack() => false;

    //===================== 공격 종료 및 취소 =====================
    public virtual void AttackEnd() { }
    public virtual void ApplyAttackEndDelay() { }
    public virtual void Cancel() { }
    
    //공격 사거리 확인
    protected bool IsInAttackRange(float attackDistance)
    {
        return Vector2.Distance(owner.AttackRigidbody.position, owner.AttackTarget.position) <= attackDistance;
    }

    //공격 사거리까지 접근
    protected bool ApproachAttackRange(float attackDistance)
    {
        if (IsInAttackRange(attackDistance)) return true;

        owner.ExecuteMove();
        return false;
    }

    //공격 방향에 따른 이펙트 회전 및 위치 보정
    protected void UpdateEffectTransform(Transform effectTransform, Quaternion rotation, Vector2 offset)
    {
        effectTransform.rotation = rotation;

        Vector3 rotatedOffset = rotation * (Vector3)offset;
        Vector2 origin = owner.AttackRigidbody.position;
        Vector3 position = effectTransform.position;

        effectTransform.position = new Vector3(origin.x + rotatedOffset.x, origin.y + rotatedOffset.y, position.z);
    }
}
