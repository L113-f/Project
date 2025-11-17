using UnityEngine;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;
    public GameObject PuzzelUI;

    [Header("level root")]
    public List<GameObject> levelRoots = new List<GameObject>();

    [Header("level win panel")]
    public List<GameObject> winPanels = new List<GameObject>();


    public int currentLevelIndex = 0;

    private PuzzlePieces[] currentPieces;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitLevel(currentLevelIndex);
    }


    public void InitLevel(int index)
    {
        if (levelRoots.Count == 0)
        {

            return;
        }

        if (index < 0 || index >= levelRoots.Count)
        {

            return;
        }

        currentLevelIndex = index;


        for (int i = 0; i < levelRoots.Count; i++)
        {
            levelRoots[i].SetActive(i == currentLevelIndex);
        }


        for (int i = 0; i < winPanels.Count; i++)
        {
            if (winPanels[i] != null)
                winPanels[i].SetActive(false);
        }


        GameObject levelRoot = levelRoots[currentLevelIndex];
        currentPieces = levelRoot.GetComponentsInChildren<PuzzlePieces>(true);


    }


    public void CheckWin()
    {
        if (currentPieces == null || currentPieces.Length == 0)
            return;

        foreach (var p in currentPieces)
        {
            if (!p.IsInRightPlace)
                return;
        }




        if (currentLevelIndex < winPanels.Count && winPanels[currentLevelIndex] != null)
        {
            winPanels[currentLevelIndex].SetActive(true);
        }
    }


    public void GoToNextLevel()
    {
        int nextIndex = currentLevelIndex + 1;

        if (nextIndex >= levelRoots.Count)
        {


            return;
        }

        InitLevel(nextIndex);
    }

    public void ExitGame()
    {
        PuzzelUI.SetActive(false);
    }



}