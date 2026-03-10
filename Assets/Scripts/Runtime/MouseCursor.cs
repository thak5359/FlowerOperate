using UnityEngine;
using UnityEngine.UI;

public class MousePointer : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [Header("Cursor Images")]
    [SerializeField] private Sprite normalCursorSprite;
    [SerializeField] private Sprite clickCursorSprite;

    [Header("Cursor Components")]
    [SerializeField] private RectTransform cursor;
    [SerializeField] private Image cursorImage;

    private bool isCursorActive = false;

    public void Active()
    {
        isCursorActive = true;
        Cursor.visible = false;
        cursorImage.raycastTarget = false;

    }
    public void Deactive()
    {
        isCursorActive = false;
        Cursor.visible = true;

    }
    private void Update()
    {
        if (isCursorActive)
        {
            CursorMousePosition();
            CursorClickState();
        }
    }

    private void CursorMousePosition()
    {
        cursor.position = Input.mousePosition;
    }

    private void CursorClickState()
    {

        if (Input.GetMouseButtonDown(0))
        {
            cursorImage.sprite = clickCursorSprite;
        }

        if (Input.GetMouseButtonUp(0))
        {
            cursorImage.sprite = normalCursorSprite;
        }
    }
}