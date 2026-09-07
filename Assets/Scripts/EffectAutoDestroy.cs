using UnityEngine;

public class EffectAutoDestroy : MonoBehaviour
{
    public void DestroyEffect()
    {
        Destroy(gameObject);
    }
}