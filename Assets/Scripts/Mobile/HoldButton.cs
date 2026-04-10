using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IDragHandler
{
    public bool IsHolding { get; private set; }
    public Vector2 StartPosition { get; private set; }
    public Vector2 CurrentPosition { get; private set; }

    public float DeltaY => CurrentPosition.y - StartPosition.y;

    public void OnPointerDown(PointerEventData eventData)
    {
        IsHolding = true;
        StartPosition = eventData.position;
        CurrentPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsHolding)
        {
            CurrentPosition = eventData.position;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsHolding = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHolding = false;
    }
}
