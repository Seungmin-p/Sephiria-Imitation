using UnityEngine;

[CreateAssetMenu(fileName = "ShadowData", menuName = "Data/Shadow Data")]
public class ShadowData : ScriptableObject
{
    [SerializeField] Sprite shadow8;
    [SerializeField] Sprite shadow16;
    [SerializeField] Sprite shadow32;
    [SerializeField] Sprite shadow64;

    public Sprite GetShadowSprite(ShadowType shadowType)
    {
        return shadowType switch
        {
            ShadowType.Shadow8 => shadow8,
            ShadowType.Shadow16 => shadow16,
            ShadowType.Shadow32 => shadow32,
            ShadowType.Shadow64 => shadow64,
            _ => null
        };
    }
}