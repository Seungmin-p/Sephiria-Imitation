using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOcclusionVisual : MonoBehaviour
{
    //원본 플레이어 이미지와 반투명 표시용 이미지
    [Serializable]
    private class RendererPair
    {
        public SpriteRenderer sourceRenderer; //원본 스프라이트(플레이어 본체, 무기 방패)
        public SpriteRenderer occlusionRenderer; //반투명 출력용 스프라이트(플레이어 본체, 무기 방패)
    }

    [Header("가림 표시")]
    [SerializeField, Range(0f, 1f)] private float occlusionAlpha = 0.4f;
    [SerializeField] private RendererPair[] rendererPairs;
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField] private Transform maskRoot; //오브젝트의 마스크를 연결해 둘 내부 오브젝트

    //현재 플레이어가 들어가 있는 가림 영역
    private readonly HashSet<OcclusionArea> activeAreas = new();

    private bool isOccluded;
    private bool shadowWasEnabled;

    //OcclusionArea에서 마스크를 연결할 Transform
    public Transform MaskRoot => maskRoot;

    private void Awake()
    {
        //본체, 무기, 방패 스프라이트를 돌면서 
        foreach (RendererPair rendererPair in rendererPairs)
        {
            if (rendererPair.occlusionRenderer == null) continue;

            //반투명 이미지를 마스크 영역 안쪽에서만 표시하겠다는 의미를 지님(VisibleInsideMask)
            rendererPair.occlusionRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            rendererPair.occlusionRenderer.enabled = false; //반투명용 스프라이트 렌더러는 일단 비활성화
        }
    }

    private void LateUpdate()
    {
        if (!isOccluded) return;

        //가림 상태라면 계속해서 반투명용 이미지를 원본 이미지와 동기화
        SyncRenderers();
    }

    //영역에서 호출해주는 영역 진입 메소드
    public void EnterArea(OcclusionArea occlusionArea)
    {
        //유효하지 않거나 이미 등록된 영역이라면 패스
        if (occlusionArea == null || !activeAreas.Add(occlusionArea)) return;

        //첫 번째 영역에 진입했을 때만 가림 표시 시작
        if (activeAreas.Count == 1)
            StartOcclusion();
    }

    //영역에서 호출해주는 영역 이탈 메소드
    public void ExitArea(OcclusionArea occlusionArea)
    {
        //유효하지 않거나 등록되지 않은 영역이라면 패스
        if (occlusionArea == null || !activeAreas.Remove(occlusionArea)) return;

        //모든 영역에서 이탈했을 때 가림 표시 종료
        if (activeAreas.Count == 0)
            StopOcclusion();
    }

    //가림 표시 시작
    private void StartOcclusion()
    {
        //가림 상태 활성화
        isOccluded = true;

        //그림자의 기존 활성화 상태 저장 및 비활성화
        if (shadowRenderer != null)
        {
            shadowWasEnabled = shadowRenderer.enabled;
            shadowRenderer.enabled = false;
        }

        //반투명용 이미지를 원본 이미지와 동기화
        SyncRenderers();
    }

    //가림 표시 종료
    private void StopOcclusion()
    {
        //가려진 상태, 반투명 이미지 비활성화
        isOccluded = false;
        SetOcclusionRenderersActive(false);

        //기존 그림자 상태 복구
        if (shadowRenderer != null)
            shadowRenderer.enabled = shadowWasEnabled;
    }

    //원본 렌더러 상태를 가림 표시용 렌더러에 적용
    private void SyncRenderers()
    {
        foreach (RendererPair rendererPair in rendererPairs)
        {
            SpriteRenderer sourceRenderer = rendererPair.sourceRenderer; //원본 스프라이트
            SpriteRenderer occlusionRenderer = rendererPair.occlusionRenderer; //반투명용 스프라이트

            if (sourceRenderer == null || occlusionRenderer == null) continue;

            Transform sourceTransform = sourceRenderer.transform; //원본 스프라이트 트랜스폼
            Transform occlusionTransform = occlusionRenderer.transform; //반투명용 스프라이트 트랜스폼

            //원본 이미지의 위치, 방향, 스케일 그대로 적용
            occlusionTransform.SetPositionAndRotation(sourceTransform.position, sourceTransform.rotation);
            occlusionTransform.localScale = sourceTransform.localScale;

            //원본 이미지의 이미지, 플립 상태 또한 그대로 적용
            occlusionRenderer.sprite = sourceRenderer.sprite;
            occlusionRenderer.flipX = sourceRenderer.flipX;
            occlusionRenderer.flipY = sourceRenderer.flipY;

            //원본 색상에서 알파값만 설정값에 따라 감소
            Color sourceColor = sourceRenderer.color;
            sourceColor.a *= occlusionAlpha;
            occlusionRenderer.color = sourceColor;
            
            //반투명 이미지 활성화
            occlusionRenderer.enabled = sourceRenderer.enabled;
        }
    }

    //반투명 이미지 활성화 상태 조절
    private void SetOcclusionRenderersActive(bool isActive)
    {
        foreach (RendererPair rendererPair in rendererPairs)
        {
            if (rendererPair.occlusionRenderer != null)
                rendererPair.occlusionRenderer.enabled = isActive;
        }
    }

    private void OnDisable()
    {
        //현재 활성화 된 모든 영역에 비활성화 상태 전달
        foreach (OcclusionArea occlusionArea in activeAreas)
        {
            if (occlusionArea != null)
                occlusionArea.ReleasePlayer(this);
        }

        activeAreas.Clear();

        if (isOccluded)
            StopOcclusion();
    }
}