using UnityEngine;

public class GrassInteraction : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Animator animator;
    [SerializeField] float maxBendX = 0.25f;
    [SerializeField] float maxBendY = 0.15f;
    [SerializeField] float bendMultiplier = 0.3f;
    [SerializeField, Range(0f, 1f)] float minHeightInfluence = 0.15f; //오브젝트 하단부의 버텍스 변형 영향 비율

    [Header("복원 처리")]
    [SerializeField] float springStrength = 100f;
    [SerializeField] float springDamping = 5f;

    private static readonly int BendId = Shader.PropertyToID("_Bend");

    private MaterialPropertyBlock propertyBlock;
    private Transform player;
    private Collider2D playerCollider;

    private Vector3 playerEnterPosition; //플레이어 진입 위치
    private Vector2 bendOnEnter; //진입 시의 변형값(반동 영향 고려)
    private Vector2 currentBend; //현재 변형값
    private Vector2 bendVelocity;

    private float previousAnimatorSpeed;

    private void Awake()
    {
        //MaterialPropertyBlock 생성
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        //플레이어 확보
        FindPlayer();
    }

    private void Update()
    {
        //플레이어와 접촉한 상태라면
        if (playerCollider != null && player != null)
        {
            //플레이어의 진입 위치를 이용해서 플레이어의 이동량 계산
            Vector2 playerMovement = transform.InverseTransformVector(player.position - playerEnterPosition);

            //플레이어가 오브젝트의 어느 높이쯤에 위치하는지 계산
            Bounds grassBounds = spriteRenderer.bounds;
            float contactHeight = Mathf.InverseLerp(grassBounds.min.y, grassBounds.max.y, player.position.y);

            //오브젝트 상단에 가까울수록 변형 강도 증가
            float heightInfluence = Mathf.Lerp(minHeightInfluence, 1f, contactHeight);

            //플레이어의 이동량을 버텍스 변형량에 즉시 반영
            //초기 변형값 + (플레이어 움직임 * 변형 배율(0.3) * Y축 배율)
            ApplyBend(bendOnEnter + playerMovement * bendMultiplier * heightInfluence);
            return;
        }

        //플레이어가 벗어난 후, 복원처리가 끝났으면 그대로 return
        if (currentBend == Vector2.zero && bendVelocity == Vector2.zero) return;

        //스프링 반동을 이용한 복원 가속도
        //-currentBend * springStrength => 원래 위치까지의 복원 수치
        //- bendVelocity * springDamping => 복원에 저항하는 수치
        Vector2 acceleration = -currentBend * springStrength - bendVelocity * springDamping;
        
        //가속도 중첩, 복원 처리 계산이 진행될수록 점점 커짐
        bendVelocity += acceleration * Time.deltaTime;
        
        //복원 진행(기존 변형 + (최종 가속도 * 시간)
        ApplyBend(currentBend + bendVelocity * Time.deltaTime);

        //충분히 복원되면 완전히 정지
        if (currentBend == Vector2.zero && bendVelocity == Vector2.zero)
        {
            //초기화
            bendVelocity = Vector2.zero;
            ApplyBend(Vector2.zero);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerCollider != null) return;

        if (player == null) FindPlayer();
        
        //범위에 들어온게 플레이어인지 확인
        if (player == null || !other.transform.IsChildOf(player)) return;

        //플레이어의 콜라이더, 진입 위치, 진입 시의 변형값 저장
        playerCollider = other;
        playerEnterPosition = player.position;
        bendOnEnter = currentBend;
        bendVelocity = Vector2.zero;

        //스프라이트 애니메이션 속도값 저장 후 0으로 변경해서 애니메이션 정지
        previousAnimatorSpeed = animator.speed;
        animator.speed = 0f;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //빠져나간게 플레이어인지 확인
        if (other != playerCollider) return;

        //빠져나간게 맞다면 콜라이더 저장 초기화
        playerCollider = null;

        //기존 애니메이션 속도로 재개
        animator.speed = previousAnimatorSpeed;
    }

    //플레이어를 찾아서 저장
    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    //버텍스 변형량 적용
    private void ApplyBend(Vector2 bend)
    {
        //최대 범위 제한
        bend.x = Mathf.Clamp(bend.x, -maxBendX, maxBendX);
        bend.y = Mathf.Clamp(bend.y, -maxBendY, maxBendY);

        currentBend = bend;

        //새로 적용할 값을 세팅하고 적용 진행
        propertyBlock.SetVector(BendId, new Vector4(bend.x, bend.y, 0f, 0f));
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }
}