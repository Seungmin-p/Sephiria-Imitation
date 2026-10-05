using UnityEngine;

public class MonsterAnimationEventReceiver : MonoBehaviour
{
    [SerializeField] Monster monster;

    public void OnDeathAnimationEnd()
    {
        monster.OnDeathAnimationEnd();
    }

    public void ShowAttackWarning()
    {
        monster.ShowAttackWarning();
    }

    public void AttackEnd()
    {
        monster.AttackEnd();
    }
}
