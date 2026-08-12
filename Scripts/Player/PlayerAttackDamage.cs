using UnityEngine;

public class PlayerAttackDamage : MonoBehaviour
{
    [Header("플레이어 컴포넌트")]
    [SerializeField] private Player player;

    [Header("공격 데미지 계수")]
    [SerializeField] private float firstAttackCoefficient = 1f;
    [SerializeField] private float secondAttackCoefficient = 1f;
    [SerializeField] private float thirdAttackCoefficient = 1.7f;
    [SerializeField] private float dashAttackCoefficient = 0.8f;
    [SerializeField] private float strikeAttackCoefficient = 2.5f;

    //공격 데이터 생성
    public AttackData CreateAttackData(Vector2 attackDirection, int attackCombo)
    {
        float damage = CalculatePhysicalDamage(attackCombo);
        bool isCritical = IsCritical();

        if (isCritical)
            damage *= player.Stats.CriticalDamage;

        return new AttackData(damage, attackDirection, isCritical, ElementType.Physical, DamageSource.Player);
    }

    //물리 기반 데미지 공식
    private float CalculatePhysicalDamage(int attackCombo)
    {
        float damage = player.Stats.PhysicalAttackPower;

        damage *= GetAttackCoefficient(attackCombo);
        damage *= player.Stats.WeaponDamageMultiplier;
        damage *= GetAttackTypeMultiplier();
        damage *= player.Stats.AllDamageMultiplier;

        return damage;
    }

    //공격 모션별 데미지 계수
    private float GetAttackCoefficient(int attackCombo)
    {
        if (player.IsDashAttacking)
            return dashAttackCoefficient;

        if (player.IsStrikeAttacking)
            return strikeAttackCoefficient;

        return attackCombo switch
        {
            1 => firstAttackCoefficient,
            2 => secondAttackCoefficient,
            3 => thirdAttackCoefficient,
            _ => firstAttackCoefficient
        };
    }

    //공격 타입별 데미지 계수
    private float GetAttackTypeMultiplier()
    {
        if (player.IsDashAttacking)
            return player.Stats.DashAttackDamageMultiplier;

        if (player.IsStrikeAttacking)
            return player.Stats.StrikeAttackDamageMultiplier;

        return player.Stats.NormalAttackDamageMultiplier;
    }

    //치명타 판정
    private bool IsCritical()
    {
        return Random.value < player.Stats.CriticalChance;
    }
}