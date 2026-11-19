using System;
using UnityEngine;
using Unity.Properties;
using UnityEngine.UIElements;

public class PlayerStats : MonoBehaviour, INotifyBindablePropertyChanged
{
    [Header("기본 능력치")]
    [SerializeField] private float maxHealth = 100; //HP
    [SerializeField] private float maxMana = 50; //MP
    [SerializeField] private float manaRegen = 3; //마나 재생

    [Header("공격 능력치")]
    [SerializeField] private float physicalDamage = 20; //공격력
    [SerializeField] private float flameDamage; //불 속성 공격력
    [SerializeField] private float iceDamage; //얼음 속성 공격력
    [SerializeField] private float lightningDamage; //전기 속성 공격력

    [Header("방어 능력치")]
    [SerializeField] private int defense; //방어력
    [SerializeField] private int evasion; //회피
    [SerializeField] private float hitInvincibilityDuration = 0.2f; //피격 후 무적 시간

    [Header("치명타 능력치")]
    [SerializeField] private float criticalChance; //치명타 확률
    [SerializeField] private float criticalDamage = 0.5f; //치명타 피해

    [Header("행동 능력치")]
    [SerializeField] private float baseAttackSpeed = 1f; //기본 공격 속도
    [SerializeField] private float attackSpeedMultiplier = 1f; //공격 속도 비율
    [SerializeField] private float baseMoveSpeed = 5f; //기본 이동 속도
    [SerializeField] private float moveSpeedMultiplier = 1f; //이동 속도 비율

    [Header("피해 증폭 비율")]
    [SerializeField] private float finalDamage = 1f; //모든 피해 증폭 비율
    [SerializeField] private float weaponDamageMultiplier = 1f; //무기 피해량 비율
    [SerializeField] private float normalAttackDamageMultiplier = 1f; //일반 공격 피해량 비율
    [SerializeField] private float dashAttackDamageMultiplier = 1f; //대시 공격 피해량 비율
    [SerializeField] private float strikeAttackDamageMultiplier = 1f; //특수 공격 피해량 비율

    //이벤트
    public event Action OnHealthChanged;
    public event Action OnManaChanged;
    
    //인터페이스 구현
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
    
    private float currentHealth; //현재 HP
    private float currentMana; //현재 MP

    //최대 및 현재 체력 마나
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float MaxMana => maxMana;
    public float CurrentMana => currentMana;
    [CreateProperty] public float ManaRegen => manaRegen;

    //물리, 속성별 데미지
    [CreateProperty] public float PhysicalDamage => physicalDamage;
    [CreateProperty] public float FlameDamage => flameDamage;
    [CreateProperty] public float IceDamage => iceDamage;
    [CreateProperty] public float LightningDamage => lightningDamage;

    //방어력, 회피
    [CreateProperty] public int Defense => defense;
    [CreateProperty] public int Evasion => evasion;
    public float HitInvincibilityDuration => hitInvincibilityDuration; //피격 무적

    //크리티컬
    public float CriticalChance => criticalChance;
    [CreateProperty] public string UICriticalChance => $"{Mathf.RoundToInt(CriticalChance * 100f)}%";

    public float CriticalDamage => criticalDamage;
    [CreateProperty] public string UICriticalDamage => $"{Mathf.RoundToInt(CriticalDamage * 100f)}%";

    //공격속도
    public float AttackSpeed => baseAttackSpeed * attackSpeedMultiplier;
    public float AttackSpeedMultiplier => attackSpeedMultiplier;
    [CreateProperty] public string UIAttackSpeedMultiplier => $"{Mathf.RoundToInt(AttackSpeedMultiplier * 100f)}%";

    //이동속도
    public float MoveSpeed => baseMoveSpeed * moveSpeedMultiplier;
    public float MoveSpeedMultiplier => moveSpeedMultiplier;
    [CreateProperty] public string UIMoveSpeedMultiplier => $"{Mathf.RoundToInt(MoveSpeedMultiplier * 100f)}%";

    //최종 및 무기, 공격 방식별 데미지
    public float FinalDamage => finalDamage;
    [CreateProperty] public string UIFinalDamage => $"{Mathf.RoundToInt(FinalDamage * 100f)}%";

    public float WeaponDamageMultiplier => weaponDamageMultiplier;
    [CreateProperty] public string UIWeaponDamageMultiplier => $"{Mathf.RoundToInt(WeaponDamageMultiplier * 100f)}%";

    public float NormalAttackDamageMultiplier => normalAttackDamageMultiplier;
    [CreateProperty] public string UINormalAttackDamageMultiplier => $"{Mathf.RoundToInt(NormalAttackDamageMultiplier * 100f)}%";

    public float DashAttackDamageMultiplier => dashAttackDamageMultiplier;
    [CreateProperty] public string UIDashAttackDamageMultiplier => $"{Mathf.RoundToInt(DashAttackDamageMultiplier * 100f)}%";

    public float StrikeAttackDamageMultiplier => strikeAttackDamageMultiplier;
    [CreateProperty] public string UIStrikeAttackDamageMultiplier => $"{Mathf.RoundToInt(StrikeAttackDamageMultiplier * 100f)}%";

    private void Awake()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
    }
    
    //스탯 변경 알림
    private void NotifyStatChanged(string propertyName)
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(propertyName));
    }

    //HP 감소
    public void ReduceHealth(float amount)
    {
        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        
        if (previousHealth != currentHealth)
            OnHealthChanged?.Invoke();
    }

    //HP 회복
    public void RecoverHealth(float amount)
    {
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        if (previousHealth != currentHealth)
            OnHealthChanged?.Invoke();
    }

    //MP 소모
    public void UseMana(float amount)
    {
        float previousMana = currentMana;
        currentMana = Mathf.Max(currentMana - amount, 0);
        
        if (previousMana != currentMana)
            OnManaChanged?.Invoke();
    }

    //MP 회복
    private void RecoverMana(float amount)
    {
        float previousMana = currentMana;
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        
        if (previousMana != currentMana)
            OnManaChanged?.Invoke();
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