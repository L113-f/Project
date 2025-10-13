using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject notePrefab;
    public Transform spawnPoint;
    public float noteSpeed = 5f;
    public float spawnInterval = 1f;
    public float hitLineY = 0f;
    public float hitTolerance = 0.5f;
    public AudioSource musicSource;
    public Text hitText;
    public GameObject startShowButton;

    private int hitCount = 0;
    private bool musicStarted = false;

    void Start()
    {
        InvokeRepeating(nameof(SpawnNote), 1f, spawnInterval);
        if (hitText != null) hitText.enabled = false;
        if (startShowButton != null) startShowButton.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryHitNote();
        }
    }

    void SpawnNote()
    {
        GameObject note = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity);
        note.GetComponent<Note>().Initialize(noteSpeed, hitLineY, hitTolerance, this);
    }

    public void RegisterHit()
    {
        hitCount++;
        ShowHitFeedback();

        if (!musicStarted && hitCount >= 6)
        {
            Debug.Log("达到6次命中，等待玩家点击开始演出");
            ShowStartButton(); // 只弹出按钮，不播放音乐
        }
    }

    void ShowHitFeedback()
    {
        if (hitText == null) return;
        hitText.enabled = true;
        CancelInvoke(nameof(HideHitFeedback));
        Invoke(nameof(HideHitFeedback), 0.5f);
    }
    void HideHitFeedback()
    {
        hitText.enabled = false;
    }
    void ShowStartButton()
    {
        if (startShowButton != null)
        {
            startShowButton.SetActive(true);
        }
    }
    public void OnStartShowClicked()
    {
        if (startShowButton != null)
        {
            startShowButton.SetActive(false);
        }

        if (!musicStarted)
        {
            musicStarted = true;
            musicSource.Play();
            Debug.Log("演出开始，音乐播放！");
        }
    }
    void TryHitNote()
    {
        // 找到所有还在场景里、且未命中的 Note
        var candidates = FindObjectsOfType<Note>()
            .Where(n => n != null && !n.WasHit && !n.WasMissed) // 用公共属性判断状态
            .OrderBy(n => Mathf.Abs(n.transform.position.y - hitLineY)) // 距离击打线最近优先
            .ToList();

        if (candidates.Count == 0)
        {
            Debug.Log("没有可命中的 Note");
            return;
        }

        // 只尝试命中“最近的那个”
        var best = candidates[0];
        best.TryHit();
    }

}
