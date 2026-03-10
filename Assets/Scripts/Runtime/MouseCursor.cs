using UnityEngine;
using UnityEngine.UI;

public class MousePointer : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject frame;

    [Header("Cursor Images")]
    [SerializeField] private Sprite normalCursorSprite;
    [SerializeField] private Sprite clickCursorSprite;

    [Header("Cursor Components")]
    [SerializeField] private RectTransform cursor;
    [SerializeField] private Image cursorImage;

    private bool isCursorActive = false;

    public void SetFrameState(bool state)
    {
        frame.SetActive(state);
    }
    public void Active()
    {
        isCursorActive = true;
        Cursor.visible = false;
        cursorImage.raycastTarget = false;

        SetFrameState(true);
    }
    public void Deactive()
    {
        isCursorActive = false;
        Cursor.visible = true;

        SetFrameState(false);
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