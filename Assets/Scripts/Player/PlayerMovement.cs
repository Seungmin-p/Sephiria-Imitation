using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("플레이어 컴포넌트")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Player player;
    [SerializeField] Collider2D collisionCollider;
    [SerializeField] LayerMask collisionMask;

    [Header("플레이어 속성")]
    [SerializeField] float dashSpeed = 25f; //대시 속도
    [SerializeField] float dashDuration = 0.15f; //대시 지속 시간
    [SerializeField] float dashRechargeTime = 1f; //대시 쿨타임
    [SerializeField] int maxDashCount = 2; //최대 대시 회수

    //기타 변수
    private Vector2 inputVec; //방향 입력값

    //대시 관련
    private Vector2 dashDirection; //대시 방향
    private bool dashActionTrigger;
    private float dashRechargeTimer = 0f; //대시 쿨타임 타이머
    private int currentDashCount; //현재 대시 회수
    private bool dashFinishedCondition; //그래프 툴킷 조건으로 사용할 대시 종료 데이터
    private Rigidbody2D.SlideMovement slideMovement;

    public bool HasMoveInput => inputVec.sqrMagnitude > 0.01f;
    public float DashDuration => dashDuration;
    public bool DashFinishedCondition => dashFinishedCondition;
    public bool DashActionTrigger => dashActionTrigger;
    public LayerMask CollisionMask => collisionMask;

    private void Awake()
    {
        currentDashCount = maxDashCount;
        
        slideMovement = new Rigidbody2D.SlideMovement
        {
            selectedCollider = collisionCollider, //플레이어 콜라이더
            gravity = Vector2.zero, //중력은 없음
            surfaceUp = Vector2.zero, //위 아래 기준도 없음
            surfaceAnchor = Vector2.zero, //그에 따른 바닥도 없음
            maxIterations = 2, //슬라이드 연산 2회
            useSimulationMove = true //내부적으로 MovePosition 방식 사용
        };

        slideMovement.SetLayerMask(collisionMask);
        
        int environmentLayer = LayerMask.NameToLayer("Obstacle");
        bool containsEnvironment = environmentLayer >= 0 && (collisionMask.value & (1 << environmentLayer)) != 0;

        Debug.Log($"Obstacle Layer 번호 : {environmentLayer}");
        Debug.Log($"Collision Mask에 Obstacle 포함 : {containsEnvironment}");
        Debug.Log($"Collision Collider의 Rigidbody 일치 : {collisionCollider.attachedRigidbody == rb}");
        Debug.Log($"실제 연결된 Collider : {collisionCollider.name}");
    }

    //플레이어 이동 입력 받기
    public void OnMove(InputAction.CallbackContext context)
    {
        inputVec = context.ReadValue<Vector2>();
    }

    //플레이어 대시 입력 받기
    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (player.IsDashing || currentDashCount <= 0) return;

        //이동 입력이 있다면 이동 방향, 아니라면 보고있는 방향
        dashDirection = inputVec.sqrMagnitude > 0.01f ? inputVec.normalized : player.LookDirection;

        currentDashCount--;
        dashActionTrigger = true;
    }

    //플레이어 움직임 처리 - 상태머신
    public void ExecuteMove()
    {
        Vector2 velocity = inputVec * player.Stats.MoveSpeed;
        rb.Slide(velocity, Time.fixedDeltaTime, slideMovement);
    }

    //플레이어 대시 처리 - 상태머신
    public void ExecuteDash()
    {
        rb.Slide(dashDirection * dashSpeed, Time.fixedDeltaTime, slideMovement);
    }

    //대시 트리거 비활성화 - 상태머신
    public void DashActionTriggerDisable()
    {
        dashActionTrigger = false;
    }

    //플레이어 대시 종료 확인용 - 상태머신
    public void SetDashFinishedCondition(bool value)
    {
        dashFinishedCondition = value;
    }

    //대시 재충전
    public void UpdateDashRecharge()
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
    
    //이동 입력 초기화
    public void ClearInput()
    {
        inputVec = Vector2.zero;
        dashActionTrigger = false;
    }
}