using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGuard : MonoBehaviour
{
    [Header("플레이어 컴포넌트")]
    [SerializeField] Player player;

    [Header("플레이어 장비")]
    [SerializeField] Animator equipmentAnimator;

    private Vector2 guardDirection; //방어 방향
    private bool isGuardHeld; //방어버튼 누르고 있는지 체크하는 용도

    public bool IsGuardHeld => isGuardHeld;

    //플레이어 방어 입력 받기
    public void OnGuard(InputAction.CallbackContext context)
    {
        //꾹 누르면 방어 상태를 의미
        if (context.performed)
        {
            isGuardHeld = true;
            player.ApplyDirection(player.LookDirection);
        }
        //떼면 방어 해제를 의미
        else if (context.canceled)
        {
            isGuardHeld = false;
        }
    }

    //상태머신용
    public void StartGuard()
    {
        equipmentAnimator.Play("SwordAndShield_StartGuard");
    }

    //상태머신용
    public void StopGuard()
    {
        equipmentAnimator.Play("SwordAndShield_StopGuard");
    }

    public void OnGuardingEnd()
    {
        equipmentAnimator.Play("SwordAndShield_Idle");
    }
}