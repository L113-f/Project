using System.Collections.Generic;
using UnityEngine;

public class PuzzlePieceGroup : MonoBehaviour
{
    private List<PuzzlePiece> puzzles = new List<PuzzlePiece>();
    private PuzzleGameManager gameManager;

    void Start()
    {
        gameManager = PuzzleGameManager.Instance;

        // 获取所有子对象的PuzzlePiece组件
        puzzles.AddRange(GetComponentsInChildren<PuzzlePiece>(true));

        // 初始时禁用所有拼图
        SetPuzzlesActive(false);
    }

    // 设置拼图组的激活状态
    public void SetPuzzlesActive(bool active)
    {
        gameObject.SetActive(active);

        // 重置拼图状态
        if (active)
        {
            ResetAllPuzzles();
        }
    }

    // 重置所有拼图
    void ResetAllPuzzles()
    {
        foreach (PuzzlePiece puzzle in puzzles)
        {
            if (puzzle != null)
            {
                // 保持你的原始重置逻辑
                puzzle.isComplete = false;
                // 这里可以添加位置重置等
            }
        }
    }

    // 检查关卡是否完成
    public bool IsLevelComplete()
    {
        foreach (PuzzlePiece puzzle in puzzles)
        {
            if (puzzle != null && !puzzle.isComplete)
            {
                return false;
            }
        }
        return true;
    }

    // 当拼图完成时调用（由PuzzlePiece调用）
    public void OnPuzzleComplete()
    {
        if (gameManager != null && IsLevelComplete())
        {
            gameManager.CheckLevelCompletion();
        }
    }
}