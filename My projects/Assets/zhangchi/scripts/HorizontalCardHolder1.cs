using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

// 新增：定义一个可序列化的类来存储每个关卡的数据
[System.Serializable]
public class LevelData
{
    public string levelName; // 关卡名称，如“三国演义”、“水浒传”
    public List<string> chapters; // 该关卡的事件列表
    public List<Sprite> chapterImages; // 该关卡的图片列表
}

public class HorizontalCardHolder1 : MonoBehaviour
{
    // 修改：将原来的两个列表替换为一个关卡数据列表
    [Header("所有关卡数据")]
    public List<LevelData> levels = new List<LevelData>();

    [Header("当前关卡索引（0是第一个关卡）")]
    [SerializeField] private int currentLevelIndex = 0; // 默认从第一个关卡开始

    [Header("UI组件")]
    public GameObject slotPrefab;
    public Button checkButton;
    public Button nextLevelButton; // 新增：下一关按钮
    public Button previousLevelButton; // 新增：上一关按钮
    public Text levelNameText; // 新增：显示关卡名称的Text组件
    public GameObject victoryPanel;
    public GameObject MistakePanel;

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
        rect = GetComponent<RectTransform>();

        // 绑定按钮事件
        if (checkButton != null)
        {
            checkButton.onClick.AddListener(CheckOrder);
        }
        // 新增：绑定下一关按钮事件
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(NextLevel);
        }
        // 新增：绑定上一关按钮事件
        if (previousLevelButton != null)
        {
            previousLevelButton.onClick.AddListener(PreviousLevel);
        }

        // 初始化加载当前关卡
        LoadLevel(currentLevelIndex);
    }

    // 新增：加载指定关卡的核心方法
    private void LoadLevel(int levelIndex)
    {
        // 安全检查，防止索引超出范围
        if (levelIndex < 0 || levelIndex >= levels.Count)
            return;

        currentLevelIndex = levelIndex;

        // 更新UI上显示的关卡名称
        if (levelNameText != null)
        {
            levelNameText.text = levels[levelIndex].levelName;
        }

        // 保存该关卡的正确答案顺序
        correctOrder = new List<string>(levels[levelIndex].chapters);

        // 创建索引列表用于同步打乱事件和图片
        List<int> indices = Enumerable.Range(0, levels[levelIndex].chapters.Count).ToList();
        indices = indices.OrderBy(x => Random.Range(0, 100)).ToList();

        // 打乱章节和图片顺序，确保文字和图片对应关系正确
        List<string> shuffledChapters = new List<string>();
        shuffledImages = new List<Sprite>();

        for (int i = 0; i < indices.Count; i++)
        {
            int index = indices[i];
            shuffledChapters.Add(levels[levelIndex].chapters[index]);
            if (index < levels[levelIndex].chapterImages.Count)
            {
                shuffledImages.Add(levels[levelIndex].chapterImages[index]);
            }
        }

        // 清空当前场景中的旧卡片
        ClearExistingCards();

        // 生成新的、打乱顺序的卡片
        for (int i = 0; i < shuffledChapters.Count; i++)
        {
            GameObject slot = Instantiate(slotPrefab, transform);
            Card card = slot.GetComponentInChildren<Card>();

            if (card != null)
            {
                Text textComponent = card.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    textComponent.text = shuffledChapters[i];
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


        // 确保胜利面板是关闭状态
        victoryPanel.SetActive(false);
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


    // 新增：切换到下一关
    public void NextLevel()
    {
        LoadLevel((currentLevelIndex + 1) % levels.Count);
    }

    // 新增：切换到上一关
    public void PreviousLevel()
    {
        int newIndex = currentLevelIndex - 1;
        if (newIndex < 0) newIndex = levels.Count - 1; // 如果已经是第一关，就跳到最后一关
        LoadLevel(newIndex);
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

        // 恢复原来的交换逻辑
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
        // 按照层级顺序获取所有卡片的文本
        List<string> currentOrder = new List<string>();
        // 按transform的sibling index顺序遍历
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

        // 比较顺序
        bool isCorrect = currentOrder.SequenceEqual(correctOrder);

        if (isCorrect)
        {
            victoryPanel.SetActive(true);
            Debug.Log("顺序正确！恭喜！");
            StartCoroutine(HideVictoryPanelAfterDelay());

        }
        else
        {
            MistakePanel.SetActive(true);
            Debug.Log("顺序错误，请继续尝试。");

            StartCoroutine(HideMistakePanelAfterDelay());
        }
    }
    private IEnumerator HideVictoryPanelAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        victoryPanel.SetActive(false);
    }
    private IEnumerator HideMistakePanelAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        MistakePanel.SetActive(false);
    }
}