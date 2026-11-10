using UnityEngine;

public class AttackWarningLine : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    //시작, 종료 위치 및 길이
    private Vector3 startLocalPosition;
    private Vector3 endLocalPosition;
    private float startLength;
    private float endLength;

    //초기 크기, 색상
    private Vector3 originalScale;
    private Color originalColor;
    
    //피벗 중심 설정용
    private bool centerOnPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        originalScale = transform.localScale;
        originalColor = spriteRenderer.color;
    }

    //선 상태 설정
    public void Setup(Vector3 startLocalPosition, Vector3 endLocalPosition, float startLength, float endLength, bool centerOnPosition = false)
    {
        this.startLocalPosition = startLocalPosition;
        this.endLocalPosition = endLocalPosition;
        this.startLength = startLength;
        this.endLength = endLength;
        
        this.centerOnPosition = centerOnPosition;

        gameObject.SetActive(true);
        
        spriteRenderer.color = originalColor;
        SetProgress(0f);
    }

    //진행도에 따른 안내선 동작
    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        Vector3 position = Vector3.Lerp(startLocalPosition, endLocalPosition, progress);

        Vector3 scale = originalScale;
        scale.y *= Mathf.Lerp(startLength, endLength, progress);
        transform.localScale = scale;
        
        //스프라이트의 Pivot이 아닌 실제 중심을 기준으로 배치
        if (centerOnPosition)
        {
            Vector3 centerOffset = Vector3.Scale(spriteRenderer.sprite.bounds.center, scale);
            position -= transform.localRotation * centerOffset;
        }

        transform.localPosition = position;
    }
    
    //진행도에 따른 Fade 처리
    public void SetFadeProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        Color color = originalColor;
        color.a *= 1f - progress;
        spriteRenderer.color = color;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}