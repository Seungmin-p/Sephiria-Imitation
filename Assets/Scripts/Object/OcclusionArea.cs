using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OcclusionArea : MonoBehaviour
{
    //마스크 데이터
    [Serializable]
    private class MaskData
    {
        public SpriteRenderer sourceRenderer;
        public SpriteMask spriteMask;

        [NonSerialized] public Transform originalParent;
    }

    [Header("가림 마스크")]
    [SerializeField] private MaskData[] maskData;

    private PlayerOcclusionVisual currentPlayer;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        //모든 마스크별로, 부모 오브젝트 저장
        foreach (MaskData data in maskData)
        {
            if (data.spriteMask != null)
                data.originalParent = data.spriteMask.transform.parent;
        }

        //일단은 마스크 비활성화
        SetMaskActive(false);
    }

    private void LateUpdate()
    {
        if (currentPlayer == null) return;

        //대상 플레이어가 확인되어 있는 상태면 마스크 적용
        SyncMasks();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //이미 확인중인 플레이어가 있다면 패스
        if (currentPlayer != null) return;
        
        //트리거 대상에게 PlayerOcclusionVisual 컴포넌트가 존재하는지 확인
        if (!other.TryGetComponent(out PlayerOcclusionVisual playerOcclusion)) return;
        
        //가져온 PlayerOcclusionVisual에 출력용 마스크가 존재하는지 확인
        if (playerOcclusion.MaskRoot == null)
        {
            Debug.LogError("PlayerOcclusionVisual의 Mask Root가 연결되지 않았습니다.", playerOcclusion);
            return;
        }

        //현재 플레이어로 지정
        currentPlayer = playerOcclusion;

        //마스크 연결, 동기화, 마스크 활성화 진행
        AttachMasks(currentPlayer.MaskRoot);
        SyncMasks();
        SetMaskActive(true);

        //가려진 부분에 대한 반투명 표시 시작
        currentPlayer.EnterArea(this);
    }

    //마스크 연결
    private void AttachMasks(Transform maskRoot)
    {
        foreach (MaskData data in maskData)
        {
            if (data.spriteMask == null) continue;

            //오브젝트의 마스크를 플레이어에게 연결
            Transform maskTransform = data.spriteMask.transform;
            maskTransform.SetParent(maskRoot, true);
        }
    }

    //현재 스프라이트와 위치를 가림 마스크에 적용
    private void SyncMasks()
    {
        foreach (MaskData data in maskData)
        {
            if (data.sourceRenderer == null || data.spriteMask == null) continue;

            SpriteRenderer sourceRenderer = data.sourceRenderer; //오브젝트 메인 이미지
            Transform sourceTransform = sourceRenderer.transform; //오브젝트 위치
            Transform maskTransform = data.spriteMask.transform; //오브젝트의 스프라이트 마스크 위치

            data.spriteMask.sprite = sourceRenderer.sprite; //스프라이트 마스크의 이미지를 메인 이미지로 등록

            maskTransform.SetPositionAndRotation(sourceTransform.position, sourceTransform.rotation); //마스크의 위치 및 회전을 메인 오브젝트랑 맞춰줌

            Vector3 sourceScale = sourceTransform.lossyScale; //오브젝트 스케일 고려
            Vector3 parentScale = maskTransform.parent != null ? maskTransform.parent.lossyScale : Vector3.one; //부모(플레이어)의 스케일도 고려

            //원본 스프라이트가 플립되어 있을 가능성도 고려
            float flipX = sourceRenderer.flipX ? -1f : 1f;
            float flipY = sourceRenderer.flipY ? -1f : 1f;

            //마스크의 스케일 조정, 플레이어의 스케일이 다른 경우에도 오브젝트의 마스크는 항상 오브젝트와 동일한 스케일을 갖게 함
            maskTransform.localScale = new Vector3(
                DivideScale(sourceScale.x, parentScale.x) * flipX,
                DivideScale(sourceScale.y, parentScale.y) * flipY,
                DivideScale(sourceScale.z, parentScale.z)
            );
        }
    }
    
    //스케일 조정용 메소드
    private static float DivideScale(float value, float parentValue)
    {
        return Mathf.Abs(parentValue) > Mathf.Epsilon ? value / parentValue : value;
    }
    
    //spriteMask 활성, 비활성화용 메소드
    public void SetMaskActive(bool isActive)
    {
        foreach (MaskData data in maskData)
        {
            if (data.spriteMask != null)
                data.spriteMask.enabled = isActive;
        }
    }
    
    //플레이어가 빠져나가면 마스크 해제 진행
    private void OnTriggerExit2D(Collider2D other)
    {
        if (currentPlayer == null) return;
        if (!other.TryGetComponent(out PlayerOcclusionVisual playerOcclusion)) return;
        if (playerOcclusion != currentPlayer) return;

        StopOcclusion();
    }
    
    //마스크 복구 및 플레이어의 영역 이탈 진행
    private void StopOcclusion()
    {
        PlayerOcclusionVisual playerOcclusion = currentPlayer;

        RestoreMasks();
        currentPlayer = null;

        playerOcclusion.ExitArea(this);
    }

    //오브젝트의 마스크를 플레이어에서 다시 오브젝트로 이동
    private void RestoreMasks()
    {
        SetMaskActive(false);

        foreach (MaskData data in maskData)
        {
            if (data.spriteMask == null) continue;

            data.spriteMask.transform.SetParent(data.originalParent, true);
        }
    }

    //플레이어 비활성화 시 마스크 복구
    public void ReleasePlayer(PlayerOcclusionVisual playerOcclusion)
    {
        if (currentPlayer != playerOcclusion) return;

        RestoreMasks();
        currentPlayer = null;
    }

    private void OnDisable()
    {
        if (currentPlayer == null)
        {
            RestoreMasks();
            return;
        }

        StopOcclusion();
    }
}