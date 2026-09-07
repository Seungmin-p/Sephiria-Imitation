using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraFixedArea : MonoBehaviour
{
    [Header("카메라")]
    [SerializeField] private MainCamera mainCamera;
    [SerializeField] private Transform fixedPoint; //카메라를 이동시킬 위치

    [Header("이동 시간")]
    [SerializeField] private float moveDuration = 0.2f;

    private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        //플레이어가 아니라면 패스
        if (!IsPlayer(other))
            return;

        //플레이어 콜라이더가 새로 추가되고, 콜라이더 목록에 1개만 존재할 때만 로직 유지
        if (!playerColliders.Add(other) || playerColliders.Count != 1)
            return;

        //카메라 위치 이동하기
        if (mainCamera != null && fixedPoint != null)
            mainCamera.EnterFixedArea(this, fixedPoint.position, moveDuration);
    }

    //콜라이더 영역에서 빠져나가면
    private void OnTriggerExit2D(Collider2D other)
    {
        //콜라이더 목록에서 제거되면서 그로인해 목록에 아무것도 남지 않으면 로직 유지
        if (!playerColliders.Remove(other) || playerColliders.Count > 0)
            return;

        //카메라가 플레이어를 다시 추적하도록 영역 탈출
        if (mainCamera != null)
            mainCamera.ExitFixedArea(this, moveDuration);
    }

    private void OnDisable()
    {
        if (mainCamera != null && playerColliders.Count > 0)
            mainCamera.ExitFixedArea(this, moveDuration);

        playerColliders.Clear();
    }

    //플레이어 태그 확인
    private bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return true;

        return other.attachedRigidbody != null &&
               other.attachedRigidbody.CompareTag("Player");
    }

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }
}