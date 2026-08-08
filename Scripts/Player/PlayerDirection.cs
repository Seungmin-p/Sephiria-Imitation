using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDirection : MonoBehaviour
{
    [Header("각종 컴포넌트")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Camera mainCamera;

    [Header("플레이어 장비")]
    [SerializeField] private Transform weaponHand;
    [SerializeField] private Transform shieldHand;

    [Header("장비 위치")]
    [SerializeField] private Vector2 weaponHandDownPosition = new(0.21f, 0.165f);
    [SerializeField] private Vector2 shieldHandDownPosition = new(-0.23f, -0.175f);
    [SerializeField] private Vector2 weaponHandUpPosition = new(0.26f, 0.27f);
    [SerializeField] private Vector2 shieldHandUpPosition = new(-0.14f, -0.18f);

    private Vector2 lookDirection = Vector2.down;

    public Vector2 LookDirection => lookDirection;

    //마우스 위치 방향 업데이트
    public void UpdateDirection()
    {
        //마우스 좌표 가져오기
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        
        //마우스 좌표를 월드 좌표로 변환
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        
        //마우스의 좌표에서 플레이어의 좌표를 빼주고, 정규화
        lookDirection = (mouseWorldPosition - rb.position).normalized;
    }

    //종합 방향 업데이트 메소드
    public void ApplyDirection(Vector2 direction)
    {
        UpdatePlayerDirection(direction);
        UpdateEquipmentPosition(direction);
        UpdateWeaponHandDirection(direction);
        UpdateShieldHandDirection(direction);
    }

    private void UpdatePlayerDirection(Vector2 direction)
    {
        //보는 방향에 따라서 플립 진행
        playerRenderer.flipX = direction.x < 0f;
    }

    //플레이어 장비 위치 컨트롤
    private void UpdateEquipmentPosition(Vector2 direction)
    {
        //보는 방향 체크
        bool isLookingUp = direction.y > 0f;
        float directionSign = direction.x < 0f ? -1f : 1f;

        //보는 방향(위아래)에 맞는 무기, 방패 위치 설정
        Vector2 weaponPosition = isLookingUp ? weaponHandUpPosition : weaponHandDownPosition;
        Vector2 shieldPosition = isLookingUp ? shieldHandUpPosition : shieldHandDownPosition;
        
        //보는 방향(좌우)에 맞는 무기, 방패 위치 설정
        weaponPosition.x *= directionSign;
        shieldPosition.x *= directionSign;

        weaponHand.localPosition = weaponPosition;
        shieldHand.localPosition = shieldPosition;
    }

    //마우스 위치에 따른 검 회전처리
    private void UpdateWeaponHandDirection(Vector2 direction)
    {
        //방향 벡터를 라디안으로 전환하고, 이를 각도로 전환하여 회전해야하는 각도를 구함
        float referenceAngle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
        
        //클립에 따른 각도 보정
        float handAngle = referenceAngle - 70f;
        
        bool isLookingLeft = direction.x < 0f;
        
        if (isLookingLeft)
        {
            //Weapon 애니메이션 전체를 수평 대칭
            weaponHand.localScale = new Vector3(-1f, 1f, 1f);

            //X축 반전에 맞춰 회전 방향도 반전
            handAngle = -handAngle;
        }
        else
        {
            weaponHand.localScale = Vector3.one;
        }
        
        weaponHand.rotation = Quaternion.Euler(0f, 0f, handAngle);
    }

    //가드 시 방패 이동 애니메이션을 위한 방패 방향 업데이트
    private void UpdateShieldHandDirection(Vector2 direction)
    {
        bool isLookingLeft = direction.x < 0f;
        
        if (isLookingLeft)
        {
            shieldHand.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            shieldHand.localScale = Vector3.one;
        }
    }
}