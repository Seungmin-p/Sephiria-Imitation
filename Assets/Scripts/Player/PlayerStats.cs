using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("기본 능력치")]
    [SerializeField] private float maxHealth = 100; //HP
    [SerializeField] private float maxMana = 75; //MP
    [SerializeField] private float manaRegen = 10f; //마나 재생

    [Header("공격 능력치")]
    [SerializeField] private float physicalAttackPower = 20; //공격력
    [SerializeField] private float fireAttackPower; //불 속성 공격력
    [SerializeField] private float iceAttackPower; //얼음 속성 공격력
    [SerializeField] private float electricAttackPower; //전기 속성 공격력

    [Header("방어 능력치")]
    [SerializeField] private int defense; //방어력
    [SerializeField] private int evasion; //회피
    [SerializeField] private float hitInvincibilityDuration = 0.2f; //피격 후 무적 시간

    [Header("치명타 능력치")]
    [SerializeField] private float criticalChance; //치명타 확률
    [SerializeField] private float criticalDamage = 1.5f; //치명타 피해

    [Header("행동 능력치")]
    [SerializeField] private float baseAttackSpeed = 1f; //기본 공격 속도
    [SerializeField] private float attackSpeedMultiplier = 1f; //공격 속도 비율
    [SerializeField] private float baseMoveSpeed = 5f; //기본 이동 속도
    [SerializeField] private float moveSpeedMultiplier = 1f; //이동 속도 비율

    [Header("피해 증폭 비율")]
    [SerializeField] private float allDamageMultiplier = 1f; //모든 피해 증폭 비율
    [SerializeField] private float weaponDamageMultiplier = 1f; //무기 피해량 비율
    [SerializeField] private float normalAttackDamageMultiplier = 1f; //일반 공격 피해량 비율
    [SerializeField] private float dashAttackDamageMultiplier = 1f; //대시 공격 피해량 비율
    [SerializeField] private float strikeAttackDamageMultiplier = 1f; //특수 공격 피해량 비율

    private float currentHealth; //현재 HP
    private float currentMana; //현재 MP

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public float MaxMana => maxMana;
    public float CurrentMana => currentMana;
    public float ManaRegen => manaRegen;

    public float PhysicalAttackPower => physicalAttackPower;
    public float FireAttackPower => fireAttackPower;
    public float IceAttackPower => iceAttackPower;
    public float ElectricAttackPower => electricAttackPower;

    public int Defense => defense;
    public int Evasion => evasion;
    public float HitInvincibilityDuration => hitInvincibilityDuration; //피격 무적

    public float CriticalChance => criticalChance;
    public float CriticalDamage => criticalDamage;

    public float AttackSpeed => baseAttackSpeed * attackSpeedMultiplier;
    public float AttackSpeedMultiplier => attackSpeedMultiplier;

    public float MoveSpeed => baseMoveSpeed * moveSpeedMultiplier;
    public float MoveSpeedMultiplier => moveSpeedMultiplier;

    public float AllDamageMultiplier => allDamageMultiplier;
    public float WeaponDamageMultiplier => weaponDamageMultiplier;
    public float NormalAttackDamageMultiplier => normalAttackDamageMultiplier;
    public float DashAttackDamageMultiplier => dashAttackDamageMultiplier;
    public float StrikeAttackDamageMultiplier => strikeAttackDamageMultiplier;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    //HP 감소
    public void ReduceHealth(float amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
    }

    //HP 회복
    public void RecoverHealth(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    //MP 소모
    public void UseMana(float amount)
    {
        currentMana = Mathf.Max(currentMana - amount, 0);
        Debug.Log($"마나 소모 : {amount}, 현재 마나 {currentMana}");
    }

    //MP 회복
    public void RecoverMana(float amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
    }
    
    //MP 사용 가능 여부
    public bool CanUseMana(float amount)
    {
        return currentMana >= amount;
    }

    //MP 자동 회복
    public void UpdateManaRegen()
    {
        RecoverMana(manaRegen * Time.deltaTime);
    }
}