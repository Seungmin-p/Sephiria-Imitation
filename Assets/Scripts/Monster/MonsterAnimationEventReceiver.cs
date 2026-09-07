using UnityEngine;

public class MonsterAnimationEventReceiver : MonoBehaviour
{
    [SerializeField] Monster monster;

    public void OnDeathAnimationEnd()
    {
        monster.OnDeathAnimationEnd();
    }
}