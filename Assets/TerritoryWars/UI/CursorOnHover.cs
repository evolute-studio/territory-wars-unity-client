using UnityEngine;
using UnityEngine.EventSystems;

public class CursorOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    public string cursorType = "pointer";

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetCursor(cursorType);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetCursor("default");
        }
    }
}