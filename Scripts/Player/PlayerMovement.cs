using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("플레이어 컴포넌트")]
    [SerializeField] Rigidbody2D rb;

    [Header("플레이어 속성")]
    [SerializeField] float moveSpeed = 5f; //이동 속도
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

    public bool HasMoveInput => inputVec.sqrMagnitude > 0.01f;
    public float DashDuration => dashDuration;
    public bool DashFinishedCondition => dashFinishedCondition;
    public bool DashActionTrigger => dashActionTrigger;

    private void Awake()
    {
        currentDashCount = maxDashCount;
    }

    //플레이어 이동 입력 받기
    public void OnMove(InputAction.CallbackContext context)
    {
        inputVec = context.ReadValue<Vector2>();
    }

    //플레이어 대시 입력 받기
    public void OnDash(InputAction.CallbackContext context, bool isDashing, Vector2 lookDirection)
    {
        if (!context.performed) return;
        if(isDashing || currentDashCount <= 0) return;

        //이동 입력이 있다면 이동 방향, 아니라면 보고있는 방향
        dashDirection = inputVec.sqrMagnitude > 0.01f ? inputVec.normalized : lookDirection;

        currentDashCount--;
        dashActionTrigger = true;
    }

    //플레이어 움직임 처리 - 상태머신
    public void ExecuteMove()
    {
        Vector2 nextVec = inputVec * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + nextVec);
    }

    //플레이어 대시 처리 - 상태머신
    public void ExecuteDash()
    {
        //TODO : 추후 벽 우회 로직 추가
        rb.MovePosition( rb.position + dashDirection * (dashSpeed * Time.fixedDeltaTime) );
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
}