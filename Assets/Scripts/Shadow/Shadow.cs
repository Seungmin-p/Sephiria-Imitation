using UnityEngine;

public class Shadow : MonoBehaviour
{
    [Header("그림자 데이터")]
    [SerializeField] ShadowData shadowData;
    [SerializeField] ShadowType shadowType;

    [Header("별도 그림자")]
    [SerializeField] Sprite customShadowSprite;

    [Header("컴포넌트")]
    [SerializeField]  SpriteRenderer spriteRenderer;
    
    private Vector3 originalLocalPosition;
    private float originalAlpha;

    //초기 위치 저장 후, 그림자 적용
    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
        originalAlpha = spriteRenderer.color.a;
        ApplyShadow();
    }
    
    //그림자 적용
    private void ApplyShadow()
    {
        Sprite shadowSprite = customShadowSprite != null
            ? customShadowSprite
            : shadowData.GetShadowSprite(shadowType);

        spriteRenderer.sprite = shadowSprite;
        spriteRenderer.enabled = shadowSprite != null;
    }
    
    //그림자 X축 위치 반전
    public void FlipXPosition()
    {
        Vector3 position = transform.localPosition;
        position.x *= -1f;
        transform.localPosition = position;
    }
    
    //초기 위치값 기준으로 y축만 조정
    public void SetAirborneHeight(float height)
    {
        Vector3 position = transform.localPosition;
        
        //스케일 고려
        float localHeight = transform.parent != null
            ? transform.parent.InverseTransformVector(Vector3.up * height).y : height;
        
        position.y = originalLocalPosition.y - localHeight;
        transform.localPosition = position;
    }

    //초기 위치값 기준으로 y축만 복구
    public void ResetPosition()
    {
        Vector3 position = transform.localPosition;
        position.y = originalLocalPosition.y;
        transform.localPosition = position;
    }
    
    //그림자의 알파값 지정
    public void SetOpacity(float opacity)
    {
        Color color = spriteRenderer.color;
        color.a = originalAlpha * Mathf.Clamp01(opacity);
        spriteRenderer.color = color;
    }
}