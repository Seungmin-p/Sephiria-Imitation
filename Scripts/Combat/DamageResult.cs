public struct DamageResult
{
    public float damage;
    public bool isCritical;
    public bool isEvaded;
    public bool isGuarded;
    public ElementType element;
    public DamageSource source;

    public DamageResult(float damage, bool isCritical, bool isEvaded, bool isGuarded, ElementType element, DamageSource source)
    {
        this.damage = damage;
        this.isCritical = isCritical;
        this.isEvaded = isEvaded;
        this.isGuarded = isGuarded;
        this.element = element;
        this.source = source;
    }
}