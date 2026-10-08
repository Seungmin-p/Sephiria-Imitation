using UnityEngine;

public class AnimationSimpleController : MonoBehaviour
{
    public void DisableObject()
    {
        gameObject.SetActive(false);
    }
    
    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}