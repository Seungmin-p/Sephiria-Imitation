using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGuard : MonoBehaviour
{
    [Header("플레이어 컴포넌트")]
    [SerializeField] Player player;

    [Header("플레이어 장비")]
    [SerializeField] Animator equipmentAnimator;
    
    [Header("방어 관련")]
    [SerializeField, Range(0f, 180f)] private float guardAngle = 180f; //각도
    [SerializeField] Transform guardDirectionEffect; //이펙트
    [SerializeField] float perfectGuardDuration = 0.2f; //퍼펙트 가드 시간
    [SerializeField] float guardManaCost = 10f; //일반 가드 마나
    [SerializeField] float perfectGuardManaCost = 5f; //퍼펙트 가드 마나
    [SerializeField] float guardRewardDuration = 5f; //가드 보상 시간
    [SerializeField] float guardBreakDuration = 5f; //가드 브레이크 시간
    [SerializeField] float guardEffectDistance = 0.5f;

    private bool isGuardHeld; //방어버튼 누르고 있는지 체크하는 용도
    private float perfectGuardTimer;

    public bool IsGuardHeld => isGuardHeld;

    //플레이어 방어 입력 받기
    public void OnGuard(InputAction.CallbackContext context)
    {
        //꾹 누르면 방어 상태를 의미
        if (context.performed)
        {
            isGuardHeld = true;
            player.ApplyDirection(player.LookDirection);
        }
        //떼면 방어 해제를 의미
        else if (context.canceled)
        {
            isGuardHeld = false;
        }
    }
    
    //가드 진행
    public void UpdateGuard()
    {
        if (perfectGuardTimer > 0f)
            perfectGuardTimer -= Time.deltaTime;
        
        Vector2 lookDirection = player.LookDirection;

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        guardDirectionEffect.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        
        guardDirectionEffect.position = (Vector2)player.transform.position + lookDirection * guardEffectDistance;
    }
    
    //공격 방어 시도
    public DamageResult.HitResultType TryGuard(AttackData attackData)
    {
        //가드 브레이크 중이라면 실패
        if (player.StatusEffect.Has(StatusEffectType.GuardBreak))
            return DamageResult.HitResultType.Hit;
        
        //가드중이 아닌 경우
        if (!player.IsGuarding) 
            return DamageResult.HitResultType.Hit;

        //공격 방향 확보
        Vector2 attackDirection = -attackData.direction.normalized;

        //플레이어가 보는 방향과 공격이 들어온 방향을 비교
        float angle = Vector2.Angle(player.LookDirection, attackDirection);

        //방향이 90도를 넘어가면 방어 실패
        if (angle > guardAngle * 0.5f)
            return DamageResult.HitResultType.Hit;

        //퍼펙트 가드 여부
        bool isPerfectGuard = perfectGuardTimer > 0f;

        float manaCost = isPerfectGuard ? perfectGuardManaCost : guardManaCost;

        //방어 마나 소비
        player.Stats.UseMana(manaCost);
        
        //마나가 전부 소모되면 가드 브레이크
        if (player.Stats.CurrentMana <= 0f)
        {
            //가드 브레이크 디버프 부여
            player.StatusEffect.Add(StatusEffectType.GuardBreak, guardBreakDuration);
            return DamageResult.HitResultType.GuardBreak;
        }

        //다음 특수공격 보상 부여
        ApplyGuardReward(isPerfectGuard);

        //가드 종류에 따른 반환
        return isPerfectGuard
            ? DamageResult.HitResultType.PerfectGuard
            : DamageResult.HitResultType.Guard;
    }
    
    //방어 성공 시
    private void ApplyGuardReward(bool isPerfectGuard)
    {
        //퍼펙트 가드에 맞는 버프 부여
        player.StatusEffect.Add(
            isPerfectGuard ? StatusEffectType.PerfectGuardManaFree : StatusEffectType.GuardManaDiscount,
            guardRewardDuration);
    }
    
    //방어 성공 버프에 따른 특수공격 마나 비용 처리
    public float GetStrikeAttackManaCost(float baseCost)
    {
        //현재 버프에 따른 특수공격 소모 마나 반환
        if (player.StatusEffect.Has(StatusEffectType.PerfectGuardManaFree))
            return 0f;

        if (player.StatusEffect.Has(StatusEffectType.GuardManaDiscount))
            return baseCost * 0.5f;

        return baseCost;
    }

    //상태머신용
    public void StartGuard()
    {
        perfectGuardTimer = perfectGuardDuration;
        guardDirectionEffect.gameObject.SetActive(true);
        equipmentAnimator.Play("SwordAndShield_StartGuard");
    }

    //상태머신용
    public void StopGuard()
    {
        guardDirectionEffect.gameObject.SetActive(false);
        equipmentAnimator.Play("SwordAndShield_StopGuard");
    }

    public void OnGuardingEnd()
    {
        equipmentAnimator.Play("SwordAndShield_Idle");
    }
}