using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PuzzleMG : MonoBehaviour
{
    public static PuzzleMG instance;
    public List<PuzzlePiece> puzzles = new List<PuzzlePiece>();
    public GameObject gameOverPanel;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        puzzles.Clear();
        puzzles.AddRange(GetComponentsInChildren<PuzzlePiece>());
    }

    public void Check()
    {
        foreach (PuzzlePiece puzzle in puzzles)
        {
            if (!puzzle.isComplete)
            {
                return;
            }
        }

        // 过关逻辑
        Debug.Log("恭喜过关！");
        gameOverPanel.SetActive(true);
    }
}
