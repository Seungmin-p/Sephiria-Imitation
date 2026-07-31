using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("플레이어 컴포넌트")]
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D playerCollider;
    [SerializeField] SpriteRenderer playerRenderer;
    [SerializeField] Camera mainCamera; //플레이어 시선처리용 메인 카메라
    
    [Header("플레이어 속성")]
    [SerializeField] float moveSpeed = 5f; //이동 속도
    [SerializeField] float dashSpeed = 25f; //대시 속도
    [SerializeField] float dashDuration = 0.15f; //대시 지속 시간
    [SerializeField] float dashRechargeTime = 1f; //대시 쿨타임
    [SerializeField] int maxDashCount = 2; //최대 대시 회수
    [SerializeField] float attackMoveSpeed = 5f; //공격 시 이동되는 속도
    [SerializeField] float dashAttackMoveSpeed = 15f; //공격 시 이동되는 속도
    [SerializeField] float attackMoveDuration = 0.1f; //공격 이동 지속 시간
    
    [Header("플레이어 장비")]
    [SerializeField] Transform weaponHand;
    [SerializeField] Transform shieldHand;
    [SerializeField] SpriteRenderer weaponRenderer;
    [SerializeField] Animator equipmentAnimator;
    

    [Header("장비 손 위치")]
    [SerializeField] Vector2 weaponHandDownPosition = new(0.21f, 0.165f);
    [SerializeField] Vector2 shieldHandDownPosition = new(-0.23f, -0.175f);

    [SerializeField] Vector2 weaponHandUpPosition = new(0.26f, 0.27f);
    [SerializeField] Vector2 shieldHandUpPosition = new(-0.14f, -0.18f);
    
    //기타 변수
    private Vector2 inputVec; //방향 입력값
    private Vector2 lookDirection = Vector2.down; //보는 방향
    
    //대시 관련
    private Vector2 dashDirection; //대시 방향
    private bool isDashing; //대시중 플래그 변수
    private float dashTimer = 0f; //대시 유지 타이머
    private float dashRechargeTimer = 0f; //대시 쿨타임 타이머
    private int currentDashCount; //현재 대시 회수
    
    //공격 관련
    private Vector2 attackDirection; //공격 방향
    private bool isAttacking; //공격중 플래그 변수
    private float attackMoveTimer; //공격으로 인해 이동하는 시간
    private int attackCombo; //콤보 int
    // private bool isComboWaiting; //공격 모션 종료 후 콤보 연계 대기 시간
    private bool hasBufferedAttack; //공격 입력 버퍼
    private Vector2 bufferedAttackDirection; //공격 입력 방향 버퍼
    private bool isStrikeAttacking; //특수공격 플래그 변수
    private bool isDashAttacking; //대시 공격 플래그 변수
    
    //방어 관련
    private Vector2 guardDirection; //방어 방향
    private bool isGuarding; //방어중 플래그 변수
    private bool isGuardHeld; //방어버튼 누르고 있는지 체크하는 용도
    
    private void Awake()
    {
        currentDashCount = maxDashCount;
    }
    
    private void Update()
    {
        //플레이어 본체, 손 방향 및 무기 회전 업데이트
        UpdateDirection();
        ApplyDirection(lookDirection);
        
        //업데이트, 대시 충전
        UpdateAnimation();
        UpdateDashRecharge();
    }
    
    private void FixedUpdate()
    {
        if (isDashing)
        {
            PlayerDash();
            
            dashTimer -= Time.fixedDeltaTime;
            
            if(dashTimer <= 0f)
                isDashing = false;
        }
        else if(isAttacking || isStrikeAttacking || isDashAttacking)
        {
            PlayerAttackMove();
        }
        else
        {
            PlayerMove();
        }
    }
    
    //=================================== 방향 업데이트 ===================================
    //마우스 위치 방향
    private void UpdateDirection()
    {
        //마우스 좌표 가져오기
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        
        //마우스 좌표를 월드 좌표로 변환
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        
        //마우스의 좌표에서 플레이어의 좌표를 빼주고, 정규화
        lookDirection = (mouseWorldPosition - rb.position).normalized;
    }

    //종합 방향 업데이트 메소드
    private void ApplyDirection(Vector2 direction)
    {
        if (isAttacking || isStrikeAttacking || isDashAttacking) return;
        
        UpdatePlayerDirection(direction);
        UpdateEquipmentPosition(direction);
        UpdateWeaponHandDirection(direction);
        UpdateShieldHandDirection(direction);
    }

    private void UpdatePlayerDirection(Vector2 direction)
    {
        //보는 방향에 따라서 플립 진행
        playerRenderer.flipX = direction.x < 0f;
    }

    //플레이어 장비 위치 컨트롤
    private void UpdateEquipmentPosition(Vector2 direction)
    {
        //보는 방향 체크
        bool isLookingUp = direction.y > 0f;
        float directionSign = direction.x < 0f ? -1f : 1f;

        //보는 방향(위아래)에 맞는 무기, 방패 위치 설정
        Vector2 weaponPosition = isLookingUp ? weaponHandUpPosition : weaponHandDownPosition;
        Vector2 shieldPosition = isLookingUp ? shieldHandUpPosition : shieldHandDownPosition;
        
        //보는 방향(좌우)에 맞는 무기, 방패 위치 설정
        weaponPosition.x *= directionSign;
        shieldPosition.x *= directionSign;

        weaponHand.localPosition = weaponPosition;
        shieldHand.localPosition = shieldPosition;
    }

    //마우스 위치에 따른 검 회전처리
    private void UpdateWeaponHandDirection(Vector2 direction)
    {
        //방향 벡터를 라디안으로 전환하고, 이를 각도로 전환하여 회전해야하는 각도를 구함
        float referenceAngle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
        
        //클립에 따른 각도 보정
        float handAngle = referenceAngle - 70f;
        
        bool isLookingLeft = direction.x < 0f;
        
        if (isLookingLeft)
        {
            //Weapon 애니메이션 전체를 수평 대칭
            weaponHand.localScale = new Vector3(-1f, 1f, 1f);

            //X축 반전에 맞춰 회전 방향도 반전
            handAngle = -handAngle;
        }
        else
        {
            weaponHand.localScale = Vector3.one;
        }
        
        weaponHand.rotation = Quaternion.Euler(0f, 0f, handAngle);
    }

    //=================================== 플레이어 이동 및 애니메이션 ===================================
    //플레이어 이동 입력 받기
    public void OnMove(InputAction.CallbackContext context)
    {
        inputVec = context.ReadValue<Vector2>();
    }
    
    //플레이어 대시 입력 받기
    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if(isDashing || currentDashCount <= 0) return;
        
        //이동 입력이 있다면 이동 방향, 아니라면 보고있는 방향
        dashDirection = inputVec.sqrMagnitude > 0.01f ? inputVec.normalized : lookDirection;
        
        currentDashCount--;

        isDashing = true;
        dashTimer = dashDuration;
    }
    
    //플레이어 움직임 처리
    private void PlayerMove()
    {
        Vector2 nextVec = inputVec * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + nextVec);
    }
    
    //플레이어 대시 처리
    private void PlayerDash()
    {
        //TODO : 추후 벽 우회 로직 추가
        rb.MovePosition( rb.position + dashDirection * (dashSpeed * Time.fixedDeltaTime) );
    }
    
    //대시 재충전
    private void UpdateDashRecharge()
    {
        if(currentDashCount >= maxDashCount) return;
        
        //현재 대시 카운트가 최대가 아니라면
        dashRechargeTimer += Time.deltaTime;

        if (dashRechargeTimer >= dashRechargeTime)
        {
            currentDashCount++;
            dashRechargeTimer = 0f;
            Debug.Log($"대시 +1, 현재 대시 : {currentDashCount}");
        }
    }
    
    //애니메이션 업데이트
    private void UpdateAnimation()
    {
        //위 아래 보는 방향 구분
        bool isAttack = isAttacking || isStrikeAttacking || isDashAttacking;
        Vector2 animationDirection = isAttack ? attackDirection : lookDirection;
        float lookY = animationDirection.y > 0f ? 1f : -1f;
        animator.SetFloat("LookY", lookY);
        
        //움직임 구분
        animator.SetBool("IsMoving", isAttack ? false : inputVec.sqrMagnitude > 0.01f);
        animator.SetBool("IsAttacking",isAttacking);
        animator.SetBool("IsStrikeAttacking",isStrikeAttacking);
    }
    
    //=================================== 공격 ===================================
    //플레이어 공격 입력 받기
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if(isStrikeAttacking || isDashAttacking) return;

        //가드 중 공격은 특수 공격
        if (isGuarding)
        {
            //TODO : 마나 있는지 체크
            
            isGuarding = false;
            
            attackDirection = lookDirection;
            StrikeAttackStart();
        } 
        //대시 중 공격은 대시 공격
        else if (isDashing)
        {
            isDashing = false;
            attackDirection = lookDirection;

            DashAttackStart();
        }
        //공격 중이라면 버퍼처리
        else if (isAttacking)
        {
            //공격 버퍼 true
            hasBufferedAttack = true;
            bufferedAttackDirection = lookDirection;
        }
        //공격 중이 아니라면 바로 공격 실행
        else
        {
            attackDirection = lookDirection;
            AttackStart();
        }
    }
    
    //공격 실행
    private void AttackStart()
    {
        //공격 상태 전환
        ApplyDirection(attackDirection);
        isAttacking = true;
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

        //TODO : 실제 공격 판정?
    }
    
    //공격 모션 종료
    public void OnAttackAnimationEnd()
    {
        //공격 상태 해제
        isAttacking = false;
        
        //3타 공격이 아직 아니라면
        if (attackCombo < 3)
        {
            //입력 버퍼 처리
            if (hasBufferedAttack)
            {
                attackDirection = bufferedAttackDirection;
                hasBufferedAttack = false;
                AttackStart();
            }
            else
            {
                //다음 모션 전 방향 업데이트
                ApplyDirection(lookDirection);
                
                //버퍼가 없다면 다음 공격 입력 대기
                equipmentAnimator.Play($"SwordAndShield_New_WaitForAttack{attackCombo+1}");
            }
        }
        //즉시 공격 종료
        else
        {
            OnAttackEnd();
        }
    }
    
    //특수 공격 실행
    private void StrikeAttackStart()
    {
        ApplyDirection(attackDirection);
        isStrikeAttacking = true;
        
        //공격 모션 실행
        equipmentAnimator.Play("SwordAndShield_Sweep3(Charge)");

        attackMoveTimer = attackMoveDuration * 2;
    }

    private void DashAttackStart()
    {
        ApplyDirection(attackDirection);
        isDashAttacking = true;
        
        //공격 모션 실행
        equipmentAnimator.Play("SwordAndShield_New_DashAttack");

        attackMoveTimer = attackMoveDuration;
    }
    
    //특수, 대시 공격 모션 종료
    public void OnStrikeAttackEnd()
    {
        isDashAttacking = false;
        isStrikeAttacking = false;
        CancelNormalAttack();
        attackDirection = Vector2.zero;
        
        ApplyDirection(lookDirection);
        
        //가드 버튼이 아직 해제 안됐다면
        if (isGuardHeld)
        {
            StartGuard();
        }
        //해제됐다면 기본 상태로 변경
        else
        {
            equipmentAnimator.Play("SwordAndShield_Idle");
        }
    }

    //공격 후 대기 모션 종료
    public void OnAttackEnd()
    {
        //공격에 사용되는 모든 변수 초기화
        CancelNormalAttack();
        equipmentAnimator.Play("SwordAndShield_Idle");
    }

    private void CancelNormalAttack()
    {
        isAttacking = false;
        hasBufferedAttack = false;
        attackDirection = Vector2.zero;
        bufferedAttackDirection = Vector2.zero;
        attackMoveTimer = 0f;
        attackCombo = 0;
    }

    //공격 도중 움직임 처리
    private void PlayerAttackMove()
    {
        if (attackMoveTimer <= 0f) return;

        float speed = isDashAttacking ? dashAttackMoveSpeed : attackMoveSpeed;

        Vector2 nextVec = attackDirection * (speed * Time.fixedDeltaTime);

        rb.MovePosition(rb.position + nextVec);

        attackMoveTimer -= Time.fixedDeltaTime;
    }
    
    //=================================== 방어 ===================================
    //플레이어 방어 입력 받기
    public void OnGuard(InputAction.CallbackContext context)
    {
        //꾹 누르면 방어 상태를 의미
        if (context.performed)
        {
            isGuardHeld = true;
            
            //만약 특수 공격중이라면 가드는 실행하지 않음
            //일반 공격은 무시하고 바로 가드 상태로 전환
            if (isStrikeAttacking || isDashAttacking) return;
            
            if (isAttacking)
                CancelNormalAttack();

            ApplyDirection(lookDirection);
            StartGuard();
        }
        //떼면 방어 해제를 의미
        else if (context.canceled)
        {
            isGuardHeld = false;

            //특수 공격 등으로 인해 가드가 이미 해제된 경우 -> 방어 해제 애니메이션 무력화
            if (!isGuarding) return;
            
            StopGuard();
        }
    }

    private void StartGuard()
    {
        isGuarding = true;
        equipmentAnimator.Play("SwordAndShield_StartGuard");
    }

    private void StopGuard()
    {
        isGuarding = false;
        equipmentAnimator.Play("SwordAndShield_StopGuard");
    }

    //가드 시 방패 이동 애니메이션을 위한 방패 방향 업데이트
    private void UpdateShieldHandDirection(Vector2 direction)
    {
        bool isLookingLeft = direction.x < 0f;
        
        if (isLookingLeft)
        {
            shieldHand.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            shieldHand.localScale = Vector3.one;
        }
    }

    public void OnGuardingEnd()
    {
        equipmentAnimator.Play("SwordAndShield_Idle");
    }
}
