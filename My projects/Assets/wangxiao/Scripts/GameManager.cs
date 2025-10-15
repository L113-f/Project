using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject notePrefab;
    public RectTransform noteContainer;
    public float noteSpeed = 150f;
    public float spawnInterval = 1.5f;
    public float hitLineY = 0f;
    public float hitTolerance = 50f;

    public Text hitText;
    public GameObject startShowButton;
    public AudioSource musicSource;
    public RectTransform hitZone;

    private int hitCount = 0;
    private bool musicStarted = false;
    private List<NoteUI> activeNotes = new List<NoteUI>();

    void Start()
    {
        InvokeRepeating(nameof(SpawnNote), 1f, spawnInterval);
        hitText.enabled = false;
        startShowButton.SetActive(false);
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
        GameObject go = Instantiate(notePrefab, noteContainer);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 400); // 从上方开始
        var note = go.GetComponent<NoteUI>();
        note.Initialize(noteSpeed, hitLineY, hitTolerance, this);
        activeNotes.Add(note);
    }

    public void RemoveNote(NoteUI note)
    {
        activeNotes.Remove(note);
    }

    void TryHitNote()
    {
        var candidates = activeNotes
            .Where(n => !n.WasHit && !n.WasMissed)
            .OrderBy(n => Mathf.Abs(n.GetComponent<RectTransform>().anchoredPosition.y - hitLineY))
            .ToList();

        if (candidates.Count > 0)
        {
            candidates[0].TryHit();
        }
    }

    public void RegisterHit()
    {
        hitCount++;
        ShowHitFeedback();

        if (!musicStarted && hitCount >= 6)
        {
            // 隐藏判定线
            if (hitZone != null)
                hitZone.gameObject.SetActive(false);

            // 清除所有 Note
            foreach (Transform child in noteContainer)
                Destroy(child.gameObject);

            // 停止生成新 Note
            CancelInvoke(nameof(SpawnNote));

            ShowStartButton(); // 等待点击
        }
    }

    void ShowHitFeedback()
    {
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
        startShowButton.SetActive(true);
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
        }
    }
}
