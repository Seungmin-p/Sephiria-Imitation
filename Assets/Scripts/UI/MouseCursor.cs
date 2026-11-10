using UnityEngine;

public sealed class MouseCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;

    private void Start()
    {
        //마우스 포인터의 중앙을 클릭 기준점으로 지정
        Vector2 hotspot = new(cursorTexture.width * 0.5f, cursorTexture.height * 0.5f);

        //게임 마우스 커서 변경
        Cursor.visible = true;
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.ForceSoftware);
    }

    private void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
    }
}