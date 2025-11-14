using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class HorizontalCardHolder : MonoBehaviour
{
    [Header("章节列表")]
    public List<string> chapters = new List<string>()
    {
        "桃园三结义", "三英战吕布", "煮酒论英雄",
        "三顾茅庐", "草船借箭", "赤壁之战"
    };

    [Header("章节图片列表")]
    public List<Sprite> chapterImages = new List<Sprite>();

    [SerializeField] private GameObject slotPrefab;
    public Button checkButton;
    public GameObject victoryPanel;

    private List<Card> cards = new List<Card>();
    private List<string> correctOrder;
    private List<Sprite> shuffledImages;
    private Card selectedCard;
    private Card hoveredCard;
    private bool isCrossing = false;
    private RectTransform rect;

    void Start()
    {
        victoryPanel.SetActive(false);

        // 保存正确顺序
        correctOrder = new List<string>(chapters);

        // 创建索引列表用于同步打乱
        List<int> indices = Enumerable.Range(0, chapters.Count).ToList();
        indices = indices.OrderBy(x => Random.Range(0, 100)).ToList();
        // 打乱章节和图片顺序
        List<string> shuffledChapters = new List<string>();
        shuffledImages = new List<Sprite>();

        for (int i = 0; i < indices.Count; i++)
        {
            int index = indices[i];
            shuffledChapters.Add(chapters[index]);
            if (index < chapterImages.Count)
            {
                shuffledImages.Add(chapterImages[index]);
            }
        }

        chapters = shuffledChapters;

        ClearExistingCards();

        for (int i = 0; i < chapters.Count; i++)
        {
            GameObject slot = Instantiate(slotPrefab, transform);
            Card card = slot.GetComponentInChildren<Card>();

            if (card != null)
            {
                Text textComponent = card.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    textComponent.text = chapters[i];
                }

                Image imageComponent = card.GetComponentInChildren<Image>();
                if (imageComponent != null && i < shuffledImages.Count)
                {


                    imageComponent.sprite = shuffledImages[i];
                }

                cards.Add(card);

                card.PointerEnterEvent.AddListener(CardPointerEnter);
                card.PointerExitEvent.AddListener(CardPointerExit);
                card.BeginDragEvent.AddListener(BeginDrag);
                card.EndDragEvent.AddListener(EndDrag);
            }
        }

        rect = GetComponent<RectTransform>();

        if (checkButton != null)
        {
            checkButton.onClick.AddListener(CheckOrder);
        }
    }

    private void ClearExistingCards()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        cards.Clear();
    }

    private void BeginDrag(Card card)
    {
        selectedCard = card;
    }

    private void EndDrag(Card card)
    {
        if (selectedCard == null) return;
        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;
        selectedCard = null;
    }

    private void CardPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    private void CardPointerExit(Card card)
    {
        hoveredCard = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                cards.Remove(hoveredCard);
            }
        }

        if (selectedCard == null) return;
        if (isCrossing) return;

        for (int i = 0; i < cards.Count; i++)
        {
            if (selectedCard.transform.position.x > cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() < cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
            if (selectedCard.transform.position.x < cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() > cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    void Swap(int index)
    {
        isCrossing = true;
        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;
        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ?
            new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);
        selectedCard.transform.localPosition = Vector3.zero;
        isCrossing = false;
    }

    public void CheckOrder()
    {
        List<string> currentOrder = new List<string>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform slot = transform.GetChild(i);
            Card card = slot.GetComponentInChildren<Card>();
            if (card != null)
            {
                Text textComponent = card.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    currentOrder.Add(textComponent.text);
                }
            }
        }

        bool isCorrect = currentOrder.SequenceEqual(correctOrder);
        if (isCorrect)
        {
            victoryPanel.SetActive(true);
            Debug.Log("顺序正确！恭喜！");
        }
        else
        {
            Debug.Log("顺序错误，请继续尝试。");
        }
    }
}