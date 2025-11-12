using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleGameManager : MonoBehaviour
{
    public static PuzzleGameManager Instance;

    [Header("摄像机设置")]
    public Camera[] levelCameras; // 每个关卡的摄像机
    private int currentLevelIndex = 0; // 当前关卡索引
    private bool isSwitchingLevel = false; // 防止重复切换

    [Header("UI设置")]
    public GameObject levelCompletePanel; // 关卡完成面板
    public Button nextLevelButton; // 下一关按钮
    public Button exitButton; // 退出按钮
    public Text levelText; // 关卡显示文本

    [Header("关卡拼图管理")]
    public List<PuzzlePieceGroup> levelPuzzleGroups = new List<PuzzlePieceGroup>();

    void Awake()
    {
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
        Debug.Log("=== 运行时按钮检查 ===");

        // 强制重新绑定
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            Debug.Log($"下一关按钮强制绑定到: {nameof(OnNextLevelClicked)}");
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitClicked);
            Debug.Log($"退出按钮强制绑定到: {nameof(OnExitClicked)}");
        }

        InitializeGame();
    }
    void InitializeGame()
    {
        Debug.Log("初始化游戏...");

        // 安全检查
        if (levelCameras == null || levelCameras.Length == 0)
        {
            Debug.LogError("levelCameras 数组未设置或为空！");
            return;
        }

        if (levelPuzzleGroups == null || levelPuzzleGroups.Count == 0)
        {
            Debug.LogError("levelPuzzleGroups 列表未设置或为空！");
            return;
        }

        // 移除所有旧的监听器（重要！）
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            Debug.Log("下一关按钮绑定: OnNextLevelClicked");
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitClicked);
            Debug.Log("退出按钮绑定: OnExitClicked");
        }

        // 初始化UI
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        // 开始第一关
        StartCoroutine(StartLevelCoroutine(0, 0.1f));
    }

    // 开始指定关卡
    public void StartLevel(int levelIndex)
    {
        if (isSwitchingLevel) return;

        isSwitchingLevel = true;
        StartCoroutine(StartLevelCoroutine(levelIndex, 0.1f));
    }

    // 协程：延迟开始关卡
    IEnumerator StartLevelCoroutine(int levelIndex, float delay)
    {
        yield return new WaitForSeconds(delay);

        currentLevelIndex = levelIndex;

        Debug.Log($"=== 开始加载关卡 {levelIndex + 1} ===");

        // 先禁用所有拼图组
        DeactivateAllPuzzleGroups();

        // 切换摄像机
        SwitchToCamera(levelIndex);

        // 等待一帧确保摄像机切换完成
        yield return null;

        // 激活当前关卡的拼图组
        ActivateCurrentLevelPuzzles();

        // 更新关卡显示
        if (levelText != null)
        {
            levelText.text = "关卡 " + (levelIndex + 1);
        }

        // 隐藏完成面板
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        isSwitchingLevel = false;
        Debug.Log($"关卡 {levelIndex + 1} 加载完成");
    }

    // 禁用所有拼图组
    void DeactivateAllPuzzleGroups()
    {
        if (levelPuzzleGroups == null) return;

        for (int i = 0; i < levelPuzzleGroups.Count; i++)
        {
            if (levelPuzzleGroups[i] != null)
            {
                levelPuzzleGroups[i].SetPuzzlesActive(false);
                Debug.Log($"禁用拼图组 {i}: {levelPuzzleGroups[i].name}");
            }
        }
    }

    // 切换摄像机
    void SwitchToCamera(int cameraIndex)
    {
        Debug.Log($"切换到摄像机 {cameraIndex}");

        if (levelCameras == null || levelCameras.Length == 0)
        {
            Debug.LogError("levelCameras 数组未设置或为空！");
            return;
        }

        if (cameraIndex < 0 || cameraIndex >= levelCameras.Length)
        {
            Debug.LogError($"摄像机索引 {cameraIndex} 超出范围");
            return;
        }

        if (levelCameras[cameraIndex] == null)
        {
            Debug.LogError($"摄像机 {cameraIndex} 为 null!");
            return;
        }

        // 禁用所有摄像机
        for (int i = 0; i < levelCameras.Length; i++)
        {
            if (levelCameras[i] != null)
            {
                levelCameras[i].gameObject.SetActive(false);
                AudioListener listener = levelCameras[i].GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
            }
        }

        // 启用当前摄像机
        levelCameras[cameraIndex].gameObject.SetActive(true);

        // 处理Audio Listener
        AudioListener currentListener = levelCameras[cameraIndex].GetComponent<AudioListener>();
        if (currentListener != null) currentListener.enabled = true;

        // 安全地设置主摄像机标签
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam != levelCameras[cameraIndex])
        {
            mainCam.tag = "Untagged";
        }
        levelCameras[cameraIndex].tag = "MainCamera";

        Debug.Log($"摄像机切换完成: {levelCameras[cameraIndex].name}");
    }

    void ActivateCurrentLevelPuzzles()
    {
        if (levelPuzzleGroups == null || levelPuzzleGroups.Count == 0)
        {
            Debug.LogError("levelPuzzleGroups 列表未设置或为空！");
            return;
        }

        if (currentLevelIndex < 0 || currentLevelIndex >= levelPuzzleGroups.Count)
        {
            Debug.LogError($"关卡索引 {currentLevelIndex} 超出拼图组范围");
            return;
        }

        if (levelPuzzleGroups[currentLevelIndex] != null)
        {
            levelPuzzleGroups[currentLevelIndex].SetPuzzlesActive(true);
            Debug.Log($"激活拼图组 {currentLevelIndex}: {levelPuzzleGroups[currentLevelIndex].name}");
        }
        else
        {
            Debug.LogError($"拼图组 {currentLevelIndex} 为 null!");
        }
    }

    // 检查拼图完成状态
    public void CheckLevelCompletion()
    {
        if (currentLevelIndex >= levelPuzzleGroups.Count) return;

        if (levelPuzzleGroups[currentLevelIndex] != null &&
            levelPuzzleGroups[currentLevelIndex].IsLevelComplete())
        {
            OnLevelComplete();
        }
    }

    void OnLevelComplete()
    {
        Debug.Log("=== 关卡完成 ===");
        Debug.Log($"当前关卡索引: {currentLevelIndex}");

        // 显示完成面板
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            Debug.Log("完成面板已显示");
        }
        else
        {
            Debug.LogError("levelCompletePanel 为 null!");
        }

        // 检查是否是最后一关
        bool isLastLevel = (currentLevelIndex >= levelCameras.Length - 1) ||
                          (currentLevelIndex >= levelPuzzleGroups.Count - 1);

        Debug.Log($"是否是最后一关: {isLastLevel}");
        Debug.Log($"摄像机总数: {levelCameras?.Length}, 拼图组总数: {levelPuzzleGroups?.Count}");

        // 更新UI状态
        if (nextLevelButton != null)
        {
            bool showNextButton = !isLastLevel;
            nextLevelButton.gameObject.SetActive(showNextButton);
            Debug.Log(showNextButton ? "显示下一关按钮" : "隐藏下一关按钮");
        }

        if (exitButton != null)
        {
            bool showExitButton = isLastLevel;
            exitButton.gameObject.SetActive(showExitButton);

            if (showExitButton)
            {
                exitButton.interactable = true;
                exitButton.image.raycastTarget = true;
                Debug.Log("显示退出按钮并设置为可点击");
            }
            else
            {
                Debug.Log("隐藏退出按钮");
            }
        }

        // 更新提示文字
        if (levelText != null)
        {
            levelText.text = isLastLevel ?
                $"恭喜完成所有关卡！" :
                $"恭喜你通过关卡 {currentLevelIndex + 1} ！！！";
        }

        Debug.Log("=== 界面更新完成 ===");
    }

    // 下一关按钮点击
    public void OnNextLevelClicked()
    {
        Debug.Log("=== 下一关按钮被点击 ===");
        Debug.Log($"当前关卡索引: {currentLevelIndex}");

        if (isSwitchingLevel)
        {
            Debug.Log("正在切换关卡，忽略重复点击");
            return;
        }

        Debug.Log($"点击下一关按钮，当前关卡: {currentLevelIndex}");

        // 隐藏完成面板
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
            Debug.Log("隐藏完成面板");
        }

        int nextLevel = currentLevelIndex + 1;
        Debug.Log($"计算下一关索引: {nextLevel}");

        // 检查是否还有下一关
        bool hasNextLevel = nextLevel < levelCameras.Length &&
                           nextLevel < levelPuzzleGroups.Count;

        Debug.Log($"是否有下一关: {hasNextLevel}");
        Debug.Log($"摄像机数量: {levelCameras?.Length}, 需要: {nextLevel + 1}");
        Debug.Log($"拼图组数量: {levelPuzzleGroups?.Count}, 需要: {nextLevel + 1}");

        if (hasNextLevel)
        {
            Debug.Log($"开始切换到下一关: {nextLevel}");
            StartLevel(nextLevel);
        }
        else
        {
            Debug.Log("所有关卡已完成！显示完成提示");
            // 显示完成提示，但不退出游戏
            if (levelText != null)
            {
                levelText.text = "恭喜！游戏通关！";
            }
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(false);
            }

            // 确保不会执行退出逻辑
            Debug.Log("退出应用程序");
            Application.Quit();
        }
    }

    // 退出按钮点击
    public void OnExitClicked()
    {
        Debug.Log("=== 退出按钮被点击 ===");

#if UNITY_EDITOR
        Debug.Log("在编辑器中停止运行");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Debug.Log("退出应用程序");
        Application.Quit();
#endif
    }

    void Update()
    {
        // 监控意外退出
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC键按下，当前状态:");
            Debug.Log($"isSwitchingLevel: {isSwitchingLevel}");
            Debug.Log($"currentLevelIndex: {currentLevelIndex}");
        }
    }
}