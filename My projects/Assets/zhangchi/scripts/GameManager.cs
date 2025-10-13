// Assets/Scripts/GameManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("游戏配置")]
    public GameObject cardPrefab;
    public Transform cardContainer;
    public Button submitButton;
    public GameObject victoryPanel;

    [Header("章节列表")]
    public List<string> chapters = new List<string>()
    {
        "桃园三结义", "三英战吕布", "煮酒论英雄",
        "三顾茅庐", "草船借箭", "赤壁之战"
    };

    private List<GameObject> cards = new List<GameObject>();

    void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        submitButton.onClick.AddListener(CheckResult);
    }

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        // 清空现有卡片
        foreach (var card in cards)
            Destroy(card);
        cards.Clear();

        victoryPanel.SetActive(false);

        // 创建新卡片
        for (int i = 0; i < chapters.Count; i++)
        {
            GameObject card = Instantiate(cardPrefab, cardContainer);
            card.GetComponentInChildren<Text>().text = chapters[i];

            var draggable = card.AddComponent<CardDraggable>();
            draggable.cardIndex = i;

            cards.Add(card);
        }

        ShuffleCards();
    }

    void ShuffleCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int randomIndex = Random.Range(0, cards.Count);
            cards[i].transform.SetSiblingIndex(randomIndex);
        }
    }

    public void CheckResult()
    {
        bool isCorrect = true;
        for (int i = 0; i < cards.Count; i++)
        {
            // 检查卡片在容器中的位置是否等于其索引
            if (cards[i].transform.GetSiblingIndex() != i)
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            victoryPanel.SetActive(true);
            Debug.Log("排序正确！阁下学识渊博！");
        }
        else
        {
            Debug.Log("排序错误，请再试一次");
        }
    }

    public void ResetGame()
    {
        InitializeGame();
    }
}