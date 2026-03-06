using UnityEngine;

public class UIToggleHelper : MonoBehaviour
{
    public void ToggleGameObject(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(!target.activeSelf);
        }
    }

    public void ToggleCanvasGroup(CanvasGroup cg)
    {
        if (cg == null) return;

        bool isVisible = cg.alpha > 0.5f;
        cg.alpha = isVisible ? 0f : 1f;
        cg.blocksRaycasts = !isVisible;
        cg.interactable = !isVisible;
    }
}