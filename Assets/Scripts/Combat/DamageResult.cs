public struct DamageResult
{
    public enum HitResultType
    {
        Ignored, //사망 혹은 파괴상태 등 판정 무시에 사용
        Hit,
        Evade,
        Guard,
        PerfectGuard,
        GuardBreak,
        ObjectHit
    }
    
    public float damage;
    public bool isCritical;
    public HitResultType hitResultType;
    public ElementType element;
    public DamageSource source;

    public DamageResult(float damage, bool isCritical, HitResultType hitResultType, ElementType element, DamageSource source)
    {
        this.damage = damage;
        this.isCritical = isCritical;
        this.hitResultType = hitResultType;
        this.element = element;
        this.source = source;
    }
}