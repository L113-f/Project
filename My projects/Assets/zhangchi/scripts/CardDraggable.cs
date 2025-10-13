// Assets/Scripts/CardDraggable.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int cardIndex;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;
    private Transform startParent;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        startParent = transform.parent;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(transform.root); // 移动到顶层Canvas
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 如果没有放入有效区域，返回原位
        if (!eventData.pointerEnter || eventData.pointerEnter.transform != transform.parent)
        {
            transform.SetParent(startParent);
            transform.position = startPosition;
        }
    }
}