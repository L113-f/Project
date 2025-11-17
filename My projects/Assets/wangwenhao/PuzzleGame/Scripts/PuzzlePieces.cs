using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class PuzzlePieces : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler

{

    public RectTransform targetSlot;


    public float snapDistance = 50f;

    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 startPos;

    public bool IsInRightPlace { get; private set; }

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsInRightPlace) return;

        startPos = rect.anchoredPosition;


        rect.SetAsLastSibling();


        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsInRightPlace) return;


        Vector2 localPos;
        RectTransform canvasRect = canvas.transform as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPos))
        {
            rect.anchoredPosition = localPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsInRightPlace) return;

        canvasGroup.blocksRaycasts = true;


        float dist = Vector2.Distance(rect.anchoredPosition, targetSlot.anchoredPosition);

        if (dist <= snapDistance)
        {

            rect.anchoredPosition = targetSlot.anchoredPosition;
            IsInRightPlace = true;


            canvasGroup.blocksRaycasts = false;
        }
        else
        {

            rect.anchoredPosition = startPos;
        }


        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.CheckWin();
        }
    }
}
