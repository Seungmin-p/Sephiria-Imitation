using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDeath : MonoBehaviour
{
    [Header("플레이어 컴포넌트")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Shadow shadow;

    [Header("사망 연출")]
    [SerializeField] private float airborneDuration = 0.6f;
    [SerializeField] private float airborneDistance = 1.5f;
    [SerializeField] private float airborneHeight = 1f;
    [SerializeField] private float gameOverDelay = 2f;
    [SerializeField] private Color deathColor = Color.gray;

    private readonly RaycastHit2D[] collisionHits = new RaycastHit2D[4];

    private ContactFilter2D collisionFilter;

    private Vector2 deathDirection;
    private Vector2 airborneStartPosition;
    private Vector2 airborneEndPosition;

    private float airborneTimer;

    public float GameOverDelay => gameOverDelay;

    public static event Action<Vector2, Vector2> OnDeathStarted;
    public static event Action OnGameOver;

    //사망 충돌 대상 설정
    public void SetCollisionMask(LayerMask collisionMask)
    {
        collisionFilter = new ContactFilter2D();
        collisionFilter.SetLayerMask(collisionMask);
        collisionFilter.useTriggers = false;
    }

    //사망 처리에 사용할 방향 준비
    public void PrepareDeath(Vector2 hitDirection)
    {
        deathDirection = hitDirection.normalized;
    }

    //플레이어 사망 처리
    public void StartDeath()
    {
        //플레이어 입력 비활성화
        playerInput.enabled = false;
        playerMovement.ClearInput();

        //사망 색상 적용
        Color currentColor = playerRenderer.color;
        playerRenderer.color = new Color(deathColor.r, deathColor.g, deathColor.b, currentColor.a);

        //에어본 시작, 착지 위치
        airborneTimer = 0f;
        airborneStartPosition = rb.position;
        airborneEndPosition = airborneStartPosition + deathDirection * airborneDistance;
        
        //기존 애니메이션 상태 초기화
        animator.SetBool("IsMoving", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsStrikeAttacking", false);

        animator.Play("Airborne");

        //사망 이펙트 출력 이벤트
        OnDeathStarted?.Invoke(rb.position, deathDirection);
    }

    //사망 에어본 진행
    public bool ExecuteAirborne()
    {
        airborneTimer += Time.fixedDeltaTime;

        float t = Mathf.Clamp01(airborneTimer / airborneDuration);

        float moveT = 1f - Mathf.Pow(1f - t, 2f);
        Vector2 groundPosition = Vector2.Lerp(airborneStartPosition, airborneEndPosition, moveT);

        float height = airborneHeight * t * (1f - t);

        //높이에 맞는 그림자 설정
        if (shadow != null)
            shadow.SetAirborneHeight(height);

        //본체 이동
        Vector2 targetPosition = groundPosition + Vector2.up * height;
        MoveDeathPosition(targetPosition);

        if (t < 1f)
            return false;

        //그림자 정상화
        if (shadow != null)
            shadow.ResetPosition();

        return true;
    }

    //사망 중 충돌을 고려한 위치 이동
    private void MoveDeathPosition(Vector2 targetPosition)
    {
        Vector2 move = targetPosition - rb.position;

        if (move.sqrMagnitude <= Mathf.Epsilon)
            return;

        float distance = move.magnitude;
        Vector2 direction = move.normalized;

        int hitCount = playerCollider.Cast(direction, collisionFilter, collisionHits, distance);

        //만약 충돌 대상이 있다면 충돌 직전까지만 이동(벽 통과 방지)
        if (hitCount > 0)
            distance = Mathf.Max(collisionHits[0].distance - 0.01f, 0f);

        rb.MovePosition(rb.position + direction * distance);
    }

    //사망 착지
    public void StartDown()
    {
        animator.Play("Down");
    }

    //게임 오버
    public void GameOver()
    {
        OnGameOver?.Invoke();
    }
}