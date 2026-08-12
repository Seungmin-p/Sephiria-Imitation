using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    //공격 모션별 데이터
    [System.Serializable]
    private struct HitboxData
    {
        public Vector2 size;
        public Vector2 offset;
    }
    
    //공격 모션별 히트박스 확인용
    private enum HitboxPreviewType
    {
        Attack1,
        Attack2,
        Attack3,
        StrikeAttack,
        DashAttack
    }
    
    [Header("플레이어 컴포넌트")]
    [SerializeField] Player player;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] PlayerAttackDamage attackDamage;
    
    [Header("공격 판정")]
    [SerializeField] AttackHitbox attackHitbox;
    [SerializeField] HitboxData attack1Hitbox;
    [SerializeField] HitboxData attack2Hitbox;
    [SerializeField] HitboxData attack3Hitbox;
    [SerializeField] HitboxData strikeAttackHitbox;
    [SerializeField] HitboxData dashAttackHitbox;
    [SerializeField] HitboxPreviewType hitboxPreview; //히트박스 확인용

    [Header("무기 이펙트")] [SerializeField] Animator attackEffectAnimator; //무기 이펙트 연출용
    [SerializeField] Transform attackEffect;
    [SerializeField] float attackEffectForwardOffset = 1f;
    [SerializeField] float attackEffectUpOffset = 0.2f;
    
    [Header("타격 이펙트")]
    [SerializeField] GameObject hitEffectPrefab;

    [Header("플레이어 속성")] [SerializeField] float attackMoveSpeed = 5f; //공격 시 이동되는 속도
    [SerializeField] float dashAttackMoveSpeed = 15f; //공격 시 이동되는 속도
    [SerializeField] float attackMoveDuration = 0.1f; //공격 이동 지속 시간

    [Header("플레이어 장비")] [SerializeField] Animator equipmentAnimator;

    private Vector2 attackDirection; //공격 방향
    private bool attackActionTrigger;
    private bool attackMotionEnd;
    private bool attackWaitMotionEnd;
    private bool strikeAttackMotionEnd;
    private bool dashAttackMotionEnd;
    private float attackMoveTimer; //공격으로 인해 이동하는 시간
    private bool isHitboxActive; //공격 판정 활성화

    private int attackCombo; //콤보 int

    // private bool isComboWaiting; //공격 모션 종료 후 콤보 연계 대기 시간
    private bool hasBufferedAttack; //공격 입력 버퍼
    private Vector2 bufferedAttackDirection; //공격 입력 방향 버퍼

    public Vector2 AttackDirection => attackDirection;
    public bool AttackActionTrigger => attackActionTrigger;
    public bool AttackMotionEnd => attackMotionEnd;
    public bool AttackWaitMotionEnd => attackWaitMotionEnd;
    public bool StrikeAttackMotionEnd => strikeAttackMotionEnd;
    public bool DashAttackMotionEnd => dashAttackMotionEnd;
    public bool IsThirdCombo => attackCombo == 3;

    //플레이어 공격 입력 받기
    public void HandleAttackInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (player.IsStrikeAttacking || player.IsDashAttacking) return;

        //대시중이라면 공격 트리거 활성화(버퍼에 들어가지 않기 위함)
        if (player.IsDashing)
        {
            //공격 트리거 활성화
            attackActionTrigger = true;
            attackDirection = player.LookDirection;
        }
        //기본 공격 중이라면 버퍼처리
        else if (player.IsAttacking)
        {
            //공격 버퍼 true
            hasBufferedAttack = true;
            bufferedAttackDirection = player.LookDirection;
        }
        //기본 공격 중이 아니라면 공격 트리거 활성화
        else
        {
            //공격 트리거 활성화
            attackActionTrigger = true;
            attackDirection = player.LookDirection;
        }
    }

    //일반 공격 실행
    public void ExecuteAttack()
    {
        //공격이 실행되면 공격 트리거 비활성화
        attackActionTrigger = false;

        //공격 시작
        AttackStart();
    }

    //일반 공격 시작
    private void AttackStart()
    {
        player.ApplyDirection(attackDirection);
        attackCombo++;

        //공격 모션 실행
        equipmentAnimator.Play($"SwordAndShield_New_Attack{attackCombo}");

        if (attackCombo == 3)
        {
            attackMoveTimer = attackMoveDuration * 1.3f;
        }
        else
        {
            attackMoveTimer = attackMoveDuration;
        }
    }

    //특수 공격 실행
    public void ExecuteStrikeAttack()
    {
        //공격이 실행되면 공격 트리거 비활성화
        attackActionTrigger = false;

        player.ApplyDirection(attackDirection);

        //공격 모션 실행
        equipmentAnimator.Play("SwordAndShield_Sweep3(Charge)");

        attackMoveTimer = attackMoveDuration * 2;
    }

    //대시 공격 실행
    public void ExecuteDashAttack()
    {
        //공격이 실행되면 공격 트리거 비활성화
        attackActionTrigger = false;

        player.ApplyDirection(attackDirection);

        //공격 모션 실행
        equipmentAnimator.Play("SwordAndShield_New_DashAttack");

        attackMoveTimer = attackMoveDuration;
    }

    //공격 판정 및 이펙트 실행
    public void AttackActive()
    {
        //공격 모션 이펙트
        PlayAttackEffect();
        
        //공격 판정 대상 리셋
        attackHitbox.ResetHitTargets();
        
        //히트박스 판정 활성화
        isHitboxActive = true;
    }
    
    //공격 판정 확인
    public void PlayerAttackHitCheck()
    {
        //히트박스 판정 활성화가 되어있는지 체크
    if (!isHitboxActive) return;

    HitboxData hitboxData = GetCurrentHitboxData();
    AttackData attackData = attackDamage.CreateAttackData(attackDirection, attackCombo);

    attackHitbox.DetectTargets(hitboxData.size, hitboxData.offset, attackData, PlayHitEffect);
    }
    
    //타격 이펙트 생성
    private void PlayHitEffect(Vector2 hitPosition, DamageResult result)
    {
        float hitEffectAngle = GetHitEffectAngle();
        Quaternion rotation = Quaternion.Euler(0f, 0f, hitEffectAngle);

        Instantiate(hitEffectPrefab, hitPosition, rotation);
    }
    
    //참격 이펙트 각도 조정
    private float GetHitEffectAngle()
    {
        float attackAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        float effectAngle;
        
        float basic = player.LookDirection.x < 0f ? 180f : 0f;
        float reverse = player.LookDirection.x < 0f ? 0f : 180f;

        if (player.IsDashAttacking)
        {
            effectAngle = 90f;
        }
        else if (player.IsStrikeAttacking)
        {
            effectAngle = reverse;
        }
        else
        {
            effectAngle = attackCombo switch
            {
                1 => basic,
                2 => reverse,
                3 => basic,
                _ => basic
            };
        }

        return attackAngle + effectAngle;
    }
    
    //공격 범위 가져오기
    private HitboxData GetCurrentHitboxData()
    {
        if (player.IsStrikeAttacking)
            return strikeAttackHitbox;
        
        if(player.IsDashAttacking)
            return dashAttackHitbox;
        
        return attackCombo switch
        {
            1 => attack1Hitbox,
            2 => attack2Hitbox,
            3 => attack3Hitbox,
            _ => attack1Hitbox
        };

    }
    
    private void OnDrawGizmosSelected()
    {
        if (attackHitbox == null) return;

        HitboxData hitboxData;

        if (Application.isPlaying)
        {
            hitboxData = GetCurrentHitboxData();
        }
        else
        {
            hitboxData = GetPreviewHitboxData();
        }

        attackHitbox.DrawHitboxGizmo(hitboxData.size, hitboxData.offset);
    }
    
    private HitboxData GetPreviewHitboxData()
    {
        return hitboxPreview switch
        {
            HitboxPreviewType.Attack1 => attack1Hitbox,
            HitboxPreviewType.Attack2 => attack2Hitbox,
            HitboxPreviewType.Attack3 => attack3Hitbox,
            HitboxPreviewType.StrikeAttack => strikeAttackHitbox,
            HitboxPreviewType.DashAttack => dashAttackHitbox,
            _ => attack1Hitbox
        };
    }

    //공격 이펙트
    private void PlayAttackEffect()
    {
        attackEffect.localRotation = Quaternion.identity;

        if (player.IsStrikeAttacking)
        {
            attackEffect.localPosition = Vector2.zero;
            attackEffectAnimator.Play("Sword_Strike_Attack");
        }
        else
        {
            attackEffect.localPosition = new Vector2(attackEffectForwardOffset, attackEffectUpOffset);

            if (player.IsAttacking)
            {
                attackEffectAnimator.Play($"Sword_Attack_Combo_{attackCombo}");
            }
            else if (player.IsDashAttacking)
            {
                attackEffectAnimator.Play("Sword_Dash_Attack");
            }
        }
    }

    //콤보 체크
    public void ComboCheck()
    {
        //3타 공격이 아직 아니라면
        if (attackCombo < 3)
        {
            //입력 버퍼 처리
            if (hasBufferedAttack)
            {
                attackDirection = bufferedAttackDirection;
                hasBufferedAttack = false;
                attackActionTrigger = true; //이로 인해 공격 상태머신이 추가 공격 진행해줄 예정
            }
            else
            {
                //다음 모션 전 방향 업데이트
                player.ApplyDirection(player.LookDirection);
            }
        }

        //공격 종료는 상태머신이 처리해줌
    }

    //공격 모션 종료
    public void AttackAnimationEnd()
    {
        attackMotionEnd = true;
        isHitboxActive = false; //히트박스 판정 비활성화
    }

    //특수, 대시 공격 모션 종료
    public void StrikeAttackEnd()
    {
        strikeAttackMotionEnd = true;
        dashAttackMotionEnd = true;
        isHitboxActive = false; //히트박스 판정 비활성화
    }

    //공격 후 대기 모션 종료
    public void AttackEnd()
    {
        attackWaitMotionEnd = true;
    }

    //공격 대기 상태에 들어갔기 때문에 공격 모션 종료 트리거는 다시 비활성화
    public void WaitForNextAttack()
    {
        attackMotionEnd = false;
        equipmentAnimator.Play($"SwordAndShield_New_WaitForAttack{attackCombo + 1}");
    }

    //공격 상태 및 애니메이션 초기화
    public void ExecuteAttackEnd()
    {
        //공격에 사용되는 모든 변수 초기화
        ClearAttackData();
        equipmentAnimator.Play("SwordAndShield_Idle");
    }

    //공격 상태 초기화
    public void ClearAttackData()
    {
        attackActionTrigger = false;
        attackMotionEnd = false;
        strikeAttackMotionEnd = false;
        dashAttackMotionEnd = false;
        attackWaitMotionEnd = false;
        hasBufferedAttack = false;
        attackDirection = Vector2.zero;
        bufferedAttackDirection = Vector2.zero;
        attackMoveTimer = 0f;
        attackCombo = 0;
    }

    //공격 도중 움직임 처리
    public void PlayerAttackMove()
    {
        if (attackMoveTimer <= 0f) return;

        float speed = player.IsDashAttacking ? dashAttackMoveSpeed : attackMoveSpeed;

        Vector2 nextVec = attackDirection * (speed * Time.fixedDeltaTime);

        rb.MovePosition(rb.position + nextVec);

        attackMoveTimer -= Time.fixedDeltaTime;
    }
}