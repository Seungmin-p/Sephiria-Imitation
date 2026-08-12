using UnityEngine;

public class PlayerHit : MonoBehaviour, IDamageable
{
    [Header("플레이어")]
    [SerializeField] private Player player;

    //피격 처리
    public DamageResult TakeDamage(AttackData attackData)
    {
        //회피 판정
        if (IsEvaded())
        {
            return new DamageResult(0f, attackData.isCritical, true, false, attackData.element, attackData.source);
        }

        //방어력 적용
        float finalDamage = CalculateFinalDamage(attackData.damage);

        //HP 감소
        player.Stats.ReduceHealth(finalDamage);

        Debug.Log($"Player Damage : {finalDamage}, Current HP : {player.Stats.CurrentHealth}");

        if (player.Stats.CurrentHealth <= 0f)
        {
            PlayerDeath();
            //PlayerDeath 디테일 추후 구현
        }

        return new DamageResult(finalDamage, attackData.isCritical, false, false, attackData.element, attackData.source);
    }
    
    //회피 판정
    private bool IsEvaded()
    {
        return Random.value < CalculateEvasionRate();
    }
    
    //회피율 계산
    private float CalculateEvasionRate()
    {
        return 80f * Mathf.Log(player.Stats.Evasion / 62f + 1f) / 100f;
    }
    
    //방어력 적용 피해랑 계산
    private float CalculateFinalDamage(float damage)
    {
        float damageReduction = 44.5f * Mathf.Log(player.Stats.Defense / 40f + 1f) / 100f;
        return damage * (1f - Mathf.Clamp01(damageReduction));
    }

    //플레이어 사망 처리
    private void PlayerDeath()
    {
        Debug.Log("플레이어 사망");

        //TODO : 사망 상태 전환
    }
}