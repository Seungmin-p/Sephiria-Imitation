using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class PlayerHit : MonoBehaviour, IDamageable
{
    [Header("플레이어")]
    [SerializeField] Player player;
    [SerializeField] CameraHitShake cameraController;
    
    [Header("피격 연출")]
    [SerializeField] private Volume postProcessingVolume;
    [SerializeField] private float hitVignetteIntensity = 0.6f;
    [SerializeField] private float hitVignetteDuration = 0.33f;

    private RectangleVignetteVolumeComponent hitVignette;
    private Coroutine hitVignetteCoroutine;
    
    private void Awake()
    {
        postProcessingVolume.profile.TryGet(out hitVignette);
    }

    //피격 처리
    public DamageResult TakeDamage(AttackData attackData)
    {
        //사망 상태라면 피격 패스
        if (player.IsDead)
        {
            return new DamageResult(
                0f,
                attackData.isCritical,
                DamageResult.HitResultType.Ignored,
                attackData.element,
                attackData.source
            );
        }
        
        //회피 판정
        if (IsEvaded())
        {
            return new DamageResult(0f, attackData.isCritical, DamageResult.HitResultType.Evade, attackData.element, attackData.source);
        }
        
        //방어 판정
        DamageResult.HitResultType guardResult = player.TryGuard(attackData);
        if (guardResult != DamageResult.HitResultType.Hit)
        {
            return new DamageResult(
                0f,
                attackData.isCritical,
                guardResult,
                attackData.element,
                attackData.source
            );
        }

        //방어력 적용
        float finalDamage = CalculateFinalDamage(attackData.damage);

        //HP 감소
        player.Stats.ReduceHealth(finalDamage);
        
        //플레이어 피격 연출
        PlayerHitVisual();

        Debug.Log($"Player Damage : {finalDamage}, Current HP : {player.Stats.CurrentHealth}");

        //체력이 0이면 사망 상태로 전환
        if (player.Stats.CurrentHealth <= 0f)
            player.ChangeDeathState(attackData.direction);

        return new DamageResult(finalDamage, attackData.isCritical, DamageResult.HitResultType.Hit, attackData.element, attackData.source);
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
    
    private void PlayerHitVisual()
    {
        //피격 연출(카메라 흔들림)
        cameraController.PlayHitShake();
        
        //피격 연출 포스트 프로세싱
        PlayHitVignette();
    }
    
    private void PlayHitVignette()
    {
        if (hitVignetteCoroutine != null)
            StopCoroutine(hitVignetteCoroutine);

        hitVignetteCoroutine = StartCoroutine(HitVignetteCoroutine());
    }

    private IEnumerator HitVignetteCoroutine()
    {
        float elapsedTime = 0f;

        hitVignette.intensity.value = hitVignetteIntensity;

        while (elapsedTime < hitVignetteDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsedTime / hitVignetteDuration);

            hitVignette.intensity.value = Mathf.Lerp(hitVignetteIntensity, 0f, progress);

            yield return null;
        }

        hitVignette.intensity.value = 0f;
        hitVignetteCoroutine = null;
    }
}