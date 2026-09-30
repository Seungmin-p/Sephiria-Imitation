using UnityEngine;

public class MonsterAnimationEventReceiver : MonoBehaviour
{
    [SerializeField] Monster monster;

    public void OnDeathAnimationEnd()
    {
        monster.OnDeathAnimationEnd();
    }

    public void OnAttackPrepareCue()
    {
        monster.OnAttackPrepareCue();
    }
}
