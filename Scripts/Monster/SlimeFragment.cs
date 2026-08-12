using UnityEngine;

public class SlimeFragment : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Shadow shadow;

    [Header("에어본")]
    [SerializeField] private float airborneDuration = 0.4f;
    [SerializeField] private float airborneDistance = 1.2f;
    [SerializeField] private float airborneHeight = 2f;

    [Header("페이드")]
    [SerializeField] private float fadeDuration = 0.2f;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private float airborneTimer;

    private Color originalColor;
    private bool isLanded;
    private float fadeTimer;

    private void Awake()
    {
        originalColor = spriteRenderer.color;
    }

    //점액 조각 시작
    public void Initialize(Vector2 direction)
    {
        direction.Normalize();

        startPosition = transform.position;
        endPosition = startPosition + direction * airborneDistance;

        airborneTimer = 0f;
        fadeTimer = 0f;
        isLanded = false;
    }

    private void Update()
    {
        if (!isLanded)
            UpdateAirborne();
        else
            UpdateFade();
    }

    //에어본 진행
    private void UpdateAirborne()
    {
        airborneTimer += Time.deltaTime;

        float t = Mathf.Clamp01(airborneTimer / airborneDuration);

        float moveT = 1f - Mathf.Pow(1f - t, 2f);
        Vector2 groundPosition = Vector2.Lerp(startPosition, endPosition, moveT);

        float height = airborneHeight * t * (1f - t);
        
        if (shadow != null)
            shadow.SetAirborneHeight(height);

        transform.position = groundPosition + Vector2.up * height;

        if (t >= 1f)
            Land();
    }

    //착지
    private void Land()
    {
        transform.position = endPosition;
        
        if (shadow != null)
            shadow.ResetPosition();
        
        isLanded = true;
    }

    //페이드
    private void UpdateFade()
    {
        fadeTimer += Time.deltaTime;

        float t = Mathf.Clamp01(fadeTimer / fadeDuration);

        Color color = originalColor;
        color.a = Mathf.Lerp(originalColor.a, 0f, t);
        spriteRenderer.color = color;

        if (t >= 1f)
            Destroy(gameObject);
    }
}