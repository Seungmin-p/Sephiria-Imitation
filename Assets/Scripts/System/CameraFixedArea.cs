using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraFixedArea : MonoBehaviour
{
    [Header("카메라")]
    [SerializeField] private CinemachineCamera fixedCamera;

    private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        //플레이어가 아니라면 패스
        if (!IsPlayer(other))
            return;

        //플레이어 콜라이더가 새로 추가되고, 콜라이더 목록에 1개만 존재할 때만 로직 유지
        if (!playerColliders.Add(other) || playerColliders.Count != 1)
            return;

        //고정 카메라 활성화
        if (fixedCamera != null)
            fixedCamera.gameObject.SetActive(true);
    }

    //콜라이더 영역에서 빠져나가면
    private void OnTriggerExit2D(Collider2D other)
    {
        //콜라이더 목록에서 제거되면서 그로인해 목록에 아무것도 남지 않으면 로직 유지
        if (!playerColliders.Remove(other) || playerColliders.Count > 0)
            return;

        //고정 카메라 비활성화
        if (fixedCamera != null)
            fixedCamera.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (fixedCamera != null)
            fixedCamera.gameObject.SetActive(false);

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