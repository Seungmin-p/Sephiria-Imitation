using UnityEngine;

public class PlayerInvincibility : MonoBehaviour
{
    private float invincibleEndTime;

    public bool IsInvincible => Time.time < invincibleEndTime;

    //무적 적용
    public void SetInvincibility(float duration)
    {
        invincibleEndTime = Mathf.Max(invincibleEndTime, Time.time + duration);
    }
}