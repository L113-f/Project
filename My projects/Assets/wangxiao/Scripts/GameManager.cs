using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TrackConfig
{
    public RectTransform trackContainer;   // 该轨道的父容器（UI）
    public KeyCode hitKey;                 // 命中键（如 A/S/D/F/G）
    public List<float> hitTimes = new List<float>(); // 该轨道每个球的命中时间（秒）
}

public class GameManager : MonoBehaviour
{
    // 预制体与通用参数
    public GameObject notePrefab;
    public List<TrackConfig> tracks = new List<TrackConfig>();
    public float noteSpeed = 150f;     // 像素/秒（UI 下落速度）
    public float hitLineY = 0f;        // 判定线的 anchoredPosition.y
    public float hitTolerance = 50f;   // 命中容差（像素）
    public float spawnDistanceY = 400f; // 从生成位置到判定线的默认可视距离

    // UI 引用
    public Text hitText;
    public GameObject startShowButton;
    public RectTransform hitZone;

    // 音频
    public AudioSource musicSource;      // 主音乐（点击“演出开始”按钮后播放）
    public AudioSource hitBgMusicSource; // 敲击时的背景音乐（循环伴奏）
    public AudioSource hitSfxSource;     // 每次命中的敲击音效（短促）

    // 状态
    private int hitCount = 0;
    private bool musicStarted = false;        // 是否已开始主音乐
    private bool generationStopped = false;   // 是否停止生成新球

    // 运行时结构
    private Dictionary<RectTransform, List<NoteUI>> activeNotes = new Dictionary<RectTransform, List<NoteUI>>();
    private Dictionary<TrackConfig, int> nextIndex = new Dictionary<TrackConfig, int>(); // 每轨道待生成的下标

    void Start()
    {
        if (hitText != null) hitText.enabled = false;
        if (startShowButton != null) startShowButton.SetActive(false);

        foreach (var track in tracks)
        {
            activeNotes[track.trackContainer] = new List<NoteUI>();
            nextIndex[track] = 0;
            track.hitTimes.Sort(); // 确保时间升序（可选）
        }

        // 开启敲击背景音乐（循环），用于敲击阶段的氛围
        if (hitBgMusicSource != null)
        {
            hitBgMusicSource.loop = true;
            hitBgMusicSource.Play();
        }
    }

    void Update()
    {
        // 根据当前时间调度生成
        ScheduleNotes();

        // 命中键处理（每轨道独立）
        foreach (var track in tracks)
        {
            if (Input.GetKeyDown(track.hitKey))
            {
                TryHitNote(track.trackContainer);
            }
        }
    }

    // 当前“歌曲时间”：主音乐开始后与音频同步；开始前用场景时间进行预览
    float CurrentSongTime()
    {
        if (musicStarted && musicSource != null)
            return musicSource.time;
        else
            return Time.timeSinceLevelLoad;
    }

    // 按谱面时间生成：在需要出现的时机生成，使其以固定速度在 hitTime 准时到判定线
    void ScheduleNotes()
    {
        if (generationStopped) return;

        float t = CurrentSongTime();
        float fallDuration = spawnDistanceY / noteSpeed;

        foreach (var track in tracks)
        {
            int idx = nextIndex[track];
            var times = track.hitTimes;

            while (idx < times.Count && t >= (times[idx] - fallDuration))
            {
                SpawnScheduledNote(track, times[idx], t);
                idx++;
            }

            nextIndex[track] = idx;
        }
    }

    // 生成一个谱面 Note：初始位置确保在 hitTime 准时到判定线
    void SpawnScheduledNote(TrackConfig track, float hitTime, float currentTime)
    {
        GameObject go = Instantiate(notePrefab, track.trackContainer);
        var rect = go.GetComponent<RectTransform>();

        // 距离 = 速度 * 剩余时间
        float timeToHit = Mathf.Max(0f, hitTime - currentTime);
        float startY = hitLineY + noteSpeed * timeToHit;

        // 限制初始最大高度（避免过高）
        startY = Mathf.Min(hitLineY + spawnDistanceY, startY);

        rect.anchoredPosition = new Vector2(0f, startY);

        var note = go.GetComponent<NoteUI>();
        note.Initialize(noteSpeed, hitLineY, hitTolerance, this);

        activeNotes[track.trackContainer].Add(note);
    }

    // 命中选择：选该轨道上距离判定线最近且未命中/未错过的 Note
    void TryHitNote(RectTransform trackContainer)
    {
        var notes = activeNotes[trackContainer]
            .Where(n => !n.WasHit && !n.WasMissed)
            .OrderBy(n => Mathf.Abs(n.GetComponent<RectTransform>().anchoredPosition.y - hitLineY))
            .ToList();

        if (notes.Count > 0)
            notes[0].TryHit();
    }

    // 供 NoteUI 在命中或错过时调用，移除其引用
    public void RemoveNote(NoteUI note)
    {
        RectTransform parent = note.GetComponent<RectTransform>().parent as RectTransform;
        if (parent != null && activeNotes.ContainsKey(parent))
            activeNotes[parent].Remove(note);
    }

    // 命中反馈与阶段切换（命中阈值改为 30）
    public void RegisterHit()
    {
        hitCount++;

        // 每次命中播放短促敲击音效（可选）
        if (hitSfxSource != null)
            hitSfxSource.Play();

        ShowHitFeedback();

        if (hitCount == 30)
        {
            TriggerHitEffect(); // 命中30次时触发特效
        }

        // 达到 30 次命中：停止生成、清空球体、隐藏判定线、停止敲击背景音乐、显示按钮
        if (!musicStarted && hitCount >= 30)
        {
            generationStopped = true;

            if (hitZone != null)
                hitZone.gameObject.SetActive(false);

            foreach (var track in tracks)
            {
                foreach (Transform child in track.trackContainer)
                    Destroy(child.gameObject);
                activeNotes[track.trackContainer].Clear();
            }

            if (hitBgMusicSource != null && hitBgMusicSource.isPlaying)
                hitBgMusicSource.Stop();

            ShowStartButton();
        }


        // 如果已经开始主音乐，并且总命中达到 30，则关闭主音乐
        if (musicStarted && hitCount >= 30 && musicSource != null && musicSource.isPlaying)
            musicSource.Stop();
    }

    void ShowHitFeedback()
    {
        if (hitText == null) return;
        hitText.enabled = true;
        hitText.text = hitCount.ToString();

        CancelInvoke(nameof(HideHitFeedback));
        Invoke(nameof(HideHitFeedback), 0.5f);
    }

    void TriggerHitEffect()
    {
        // 让 hitText 放大一下
        if (hitText != null)
        {
            hitText.text = "30!";
            hitText.fontSize = 120;
            hitText.color = Color.yellow;
            hitText.transform.localScale = Vector3.one * 1.5f;
        }
    }

    void HideHitFeedback()
    {
        if (hitText == null) return;
        hitText.enabled = false;
    }

    void ShowStartButton()
    {
        if (startShowButton == null) return;
        startShowButton.SetActive(true);
    }

    public void OnStartShowClicked()
    {
        // 隐藏按钮
        if (startShowButton != null)
            startShowButton.SetActive(false);

        // 播放主音乐
        if (!musicStarted && musicSource != null)
        {
            musicStarted = true;
            musicSource.Play();
        }
    }
}
