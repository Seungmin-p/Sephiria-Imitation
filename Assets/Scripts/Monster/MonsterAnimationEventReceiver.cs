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

    //공격 애니메이션 종료
    public void AttackEnd()
    {
        monster.AttackEnd();
    }
}
