using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public sealed class MouseCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Texture2D uiCursorTexture;

    [Header("UI 클릭 애니메이션")]
    [SerializeField] private Texture2D[] uiClickFrames;
    [SerializeField, Min(0.01f)] private float clickFrameDuration = 0.05f;

    private readonly HashSet<MonoBehaviour> openWindows = new();
    private Coroutine clickAnimation;
    
    public bool IsUIOpen => openWindows.Count > 0;

    private void OnEnable()
    {
        Cursor.visible = true;
        ApplyCursor();
    }

    private void Update()
    {
        //UI가 열려있을 때만 클릭 애니메이션 실행
        if (openWindows.Count == 0 || Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (uiClickFrames == null || uiClickFrames.Length == 0) return;

        //재생 중 다시 클릭하면 처음부터 재생
        StopClickAnimation();
        clickAnimation = StartCoroutine(PlayClickAnimation());
    }

    //UI 창 열림 여부를 반영
    public void SetUIWindowOpen(MonoBehaviour window, bool isOpen)
    {
        if (window == null) return;

        if (isOpen) openWindows.Add(window);
        else openWindows.Remove(window);

        StopClickAnimation();
        ApplyCursor();
    }

    //UI 커서 클릭 애니메이션
    private IEnumerator PlayClickAnimation()
    {
        foreach (Texture2D frame in uiClickFrames)
        {
            if (frame != null)
                Cursor.SetCursor(frame, Vector2.zero, CursorMode.ForceSoftware);

            yield return new WaitForSecondsRealtime(clickFrameDuration);
        }

        clickAnimation = null;
        ApplyCursor();
    }

    //클릭 애니메이션 중단
    private void StopClickAnimation()
    {
        if (clickAnimation == null) return;

        StopCoroutine(clickAnimation);
        clickAnimation = null;
    }

    //열려있는 UI 창에 맞게 커서 변경
    private void ApplyCursor()
    {
        //UI가 1개라도 열려있으면 UI 커서
        if (openWindows.Count > 0)
        {
            Cursor.SetCursor(uiCursorTexture, Vector2.zero, CursorMode.ForceSoftware);
            return;
        }

        //UI가 안열려있으면 십자커서
        //마우스 포인터의 중앙을 클릭 기준점으로 지정
        Vector2 hotspot = new(cursorTexture.width * 0.5f, cursorTexture.height * 0.5f);
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.ForceSoftware);
    }

    private void OnDisable()
    {
        StopClickAnimation();
        openWindows.Clear();
        Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
    }
}
