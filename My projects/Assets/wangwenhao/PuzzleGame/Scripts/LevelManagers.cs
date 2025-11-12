using UnityEngine;
using System.Collections.Generic; // 使用List需要引入这个命名空间

public class LevelManager : MonoBehaviour
{
    // 单例模式，方便其他脚本访问
    public static LevelManager Instance;

    // 存储所有关卡的父对象列表
    public List<GameObject> levelHolders = new List<GameObject>();
    // 当前关卡的索引
    private int currentLevelIndex = 0;

    void Awake()
    {
        // 简单的单例模式实现
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 游戏开始时，初始化并激活第一关
        InitializeLevels();
    }

    // 初始化所有关卡：禁用除第一个之外的所有关卡
    void InitializeLevels()
    {
        if (levelHolders.Count == 0) return;

        for (int i = 0; i < levelHolders.Count; i++)
        {
            // 如果关卡对象不为空，则设置其激活状态
            if (levelHolders[i] != null)
            {
                // 第一个关卡(i=0)激活，其他的都禁用
                levelHolders[i].SetActive(i == 0);
            }
        }
        currentLevelIndex = 0; // 设置当前为第一关
    }

    // 切换到下一关（公共方法，可供UI按钮调用）
    public void GoToNextLevel()
    {
        int nextLevelIndex = currentLevelIndex + 1;

        // 检查下一关索引是否有效
        if (nextLevelIndex < levelHolders.Count)
        {
            SwitchToLevel(nextLevelIndex);
        }
        else
        {
            Debug.Log("已经是最后一关了！");
            // 这里可以触发游戏胜利逻辑
        }
    }

    // 核心方法：切换到指定索引的关卡
    public void SwitchToLevel(int levelIndex)
    {
        // 安全检查：索引是否在有效范围内，关卡对象是否存在
        if (levelIndex < 0 || levelIndex >= levelHolders.Count || levelHolders[levelIndex] == null)
        {
            Debug.LogError("尝试切换的关卡索引无效: " + levelIndex);
            return;
        }

        // 1. 禁用当前关卡
        if (levelHolders[currentLevelIndex] != null)
        {
            levelHolders[currentLevelIndex].SetActive(false);
        }

        // 2. 启用目标关卡
        levelHolders[levelIndex].SetActive(true);

        // 3. 更新当前关卡索引
        currentLevelIndex = levelIndex;

        Debug.Log("切换到关卡: " + (currentLevelIndex + 1));
    }

    // 一个额外的方法：直接通过关卡名称切换（如果需要）
    public void SwitchToLevelByName(string levelName)
    {
        for (int i = 0; i < levelHolders.Count; i++)
        {
            if (levelHolders[i] != null && levelHolders[i].name == levelName)
            {
                SwitchToLevel(i);
                return;
            }
        }
        Debug.LogError("未找到名为 " + levelName + " 的关卡！");
    }
}
