using UnityEngine;
using UnityEngine.EventSystems;

public class DragTracker : MonoBehaviour, IBeginDragHandler, IEndDragHandler {
    public bool isDragging { get; private set; }

    public void OnBeginDrag(PointerEventData e) {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData e) {
        isDragging = false;
    }
}
