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

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
        ApplyShadow();
    }
    
    public void SetAirborneHeight(float height)
    {
        transform.localPosition = originalLocalPosition + Vector3.down * height;
    }

    public void ResetPosition()
    {
        transform.localPosition = originalLocalPosition;
    }

    private void ApplyShadow()
    {
        Sprite shadowSprite = customShadowSprite != null
            ? customShadowSprite
            : shadowData.GetShadowSprite(shadowType);

        spriteRenderer.sprite = shadowSprite;
        spriteRenderer.enabled = shadowSprite != null;
    }
}