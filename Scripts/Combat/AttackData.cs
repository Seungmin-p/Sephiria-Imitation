using UnityEngine;

public struct AttackData
{
    public float damage; //피해량
    public Vector2 direction; //피격 방향
    public bool isCritical; //치명타 여부
    public ElementType element; //공격 속성
    public DamageSource source; //공격 출처

    public AttackData(float damage, Vector2 direction, bool isCritical, ElementType element, DamageSource source)
    {
        this.damage = damage;
        this.direction = direction;
        this.isCritical = isCritical;
        this.element = element;
        this.source = source;
    }
}