using UnityEngine;

namespace PlayerSystem
{
    public class Equipment : MonoBehaviour
    {
        [SerializeField] Player player;
    
        //공격 모션 종료 정보 전달
        public void OnAttackAnimationEnd()
        {
            player.OnAttackAnimationEnd();
        }
    
        //공격 후 대기 모션 종료 전달
        public void OnAttackEnd()
        {
            player.OnAttackEnd();
        }
    
        //방어 후 방어 모션 종료 전달
        public void OnGuardingEnd()
        {
            player.OnGuardingEnd();
        }

        //특수 공격 모션 종료 전달
        public void OnStrikeAttackEnd()
        {
            player.OnStrikeAttackEnd();
        }
    
        //공격 이펙트 및 실제 공격 판정 진행
        public void OnAttackActive()
        {
            player.OnAttackActive();
        }
    }
}

