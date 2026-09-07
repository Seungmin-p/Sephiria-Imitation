using UnityEngine;

public class FragmentEffect : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Shadow shadow;

    [Header("에어본")]
    [SerializeField] private float airborneDuration = 0.4f;
    [SerializeField] private float airborneDistance = 1.2f;
    [SerializeField] private float airborneHeight = 2f;

    [Header("페이드")]
    [SerializeField] private float fadeDelay = 1f;
    [SerializeField] private float fadeDuration = 0.2f;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private float airborneTimer;

    private Color originalColor;
    private bool isLanded;
    private float fadeDelayTimer;
    private float fadeTimer;

    private void Awake()
    {
        //이미지 지정이 안된경우, 이미지 가져오기
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        //기본 색상 저장
        originalColor = spriteRenderer.color;
    }

    //파편 스폰처리 시작
    public void Initialize(Vector2 direction)
    {
        //전달받은 방향 정규화
        direction.Normalize();

        //시작, 끝 위치 저장
        startPosition = transform.position;
        endPosition = startPosition + direction * airborneDistance;

        //각종 타이머 및 착지상태 초기화
        airborneTimer = 0f;
        fadeDelayTimer = 0f;
        fadeTimer = 0f;
        isLanded = false;
    }

    private void Update()
    {
        //착지 전이라면 에어본, 착지 이후엔 페이드 처리 진행
        if (!isLanded)
            UpdateAirborne();
        else
            UpdateFade();
    }

    //에어본 진행
    private void UpdateAirborne()
    {
        airborneTimer += Time.deltaTime;

        //진행도 계산
        float t = Mathf.Clamp01(airborneTimer / airborneDuration);

        //초기에 빠르게 이동하고, 도착에 가까워질수록 감속
        float moveT = 1f - Mathf.Pow(1f - t, 2f);
        
        //최종 진행도 값을 기반으로 좌표 도출
        Vector2 groundPosition = Vector2.Lerp(startPosition, endPosition, moveT);

        //에어본 높이에 따른 높이 보정
        float height = airborneHeight * t * (1f - t);
        
        //그림자가 있다면 높이만큼 그림자 내리기
        if (shadow != null)
            shadow.SetAirborneHeight(height);

        //이동 하면서 높이 추가 보정 진행
        transform.position = groundPosition + Vector2.up * height;

        //전부 진행 되면 착지처리
        if (t >= 1f)
            Land();
    }

    //착지
    private void Land()
    {
        //위치 조정
        transform.position = endPosition;
        
        //그림자 위치 초기화(발 밑)
        if (shadow != null)
            shadow.ResetPosition();
        
        //착지 전환
        isLanded = true;
    }

    //페이드 처리
    private void UpdateFade()
    {
        //페이드 시작 딜레이
        if (fadeDelayTimer < fadeDelay)
        {
            fadeDelayTimer += Time.deltaTime;
            return;
        }

        fadeTimer += Time.deltaTime;

        float t = Mathf.Clamp01(fadeTimer / fadeDuration);

        //진행도에 따라서 페이드 처리 진행
        Color color = originalColor;
        color.a = Mathf.Lerp(originalColor.a, 0f, t);
        spriteRenderer.color = color;

        //진행 완료 시 오브젝트 삭제
        if (t >= 1f)
            Destroy(gameObject);
    }
}