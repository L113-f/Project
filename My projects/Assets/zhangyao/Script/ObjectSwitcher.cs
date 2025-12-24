using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSwitcher : MonoBehaviour
{
    [Header("物品数据")]
    public ItemData[] items; // 物品数据数组
    public TextMeshProUGUI nameText; // 物品名称文本
    public TextMeshProUGUI descText; // 物品介绍文本
    private int currentIndex = 0; // 当前显示的物品索引

    [Header("按钮配置")]
    public Button prevButton; // 原上一个物品按钮
    public Button nextButton; // 原下一个物品按钮
    public Button audioButton; // 新增的音频播放/暂停按钮
    public TextMeshProUGUI audioButtonText; // 音频按钮的文本组件（与切换按钮文本样式一致）

    [Header("按钮文本配置")]
    public string playText = "播放介绍"; // 未播放时的按钮文本
    public string pauseText = "暂停介绍"; // 播放中时的按钮文本

    void Start()
    {
        // 初始化按钮样式（强制复用切换按钮的样式）
        SyncAudioButtonStyle();

        // 初始化物品显示和按钮文本
        UpdateActiveObject();
        UpdateItemInfo();
        UpdateAudioButtonText();

        // 绑定按钮点击事件（与切换按钮绑定方式一致）
        //prevButton.onClick.AddListener(PreviousObject);
        //nextButton.onClick.AddListener(NextObject);
        //audioButton.onClick.AddListener(ToggleDescriptionAudio);
    }

    // 同步音频按钮样式到切换按钮（确保视觉统一）
    private void SyncAudioButtonStyle()
    {
        if (audioButton == null || prevButton == null) return;

        // 复用按钮的基础样式（颜色、大小、字体、布局）
        Image audioBtnImage = audioButton.GetComponent<Image>();
        Image prevBtnImage = prevButton.GetComponent<Image>();
        if (audioBtnImage != null && prevBtnImage != null)
        {
            audioBtnImage.sprite = prevBtnImage.sprite;
            audioBtnImage.color = prevBtnImage.color;
            audioBtnImage.type = prevBtnImage.type;
        }

        // 复用按钮的大小和布局
        RectTransform audioRect = audioButton.GetComponent<RectTransform>();
        RectTransform prevRect = prevButton.GetComponent<RectTransform>();
        if (audioRect != null && prevRect != null)
        {
            audioRect.sizeDelta = prevRect.sizeDelta; // 统一按钮大小
            audioRect.anchorMin = prevRect.anchorMin; // 统一锚点
            audioRect.anchorMax = prevRect.anchorMax;
        }

        // 复用文本样式
        if (audioButtonText != null)
        {
            TextMeshProUGUI prevText = prevButton.GetComponentInChildren<TextMeshProUGUI>();
            if (prevText != null)
            {
                audioButtonText.font = prevText.font;
                audioButtonText.fontSize = prevText.fontSize;
                audioButtonText.color = prevText.color;
                audioButtonText.alignment = prevText.alignment;
            }
        }
    }

    // 切换到上一个物品（保持原逻辑，新增按钮状态重置）
    public void PreviousObject()
    {
        // 停止当前音频并重置按钮文本
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopVoice();
        }
        UpdateAudioButtonText();

        currentIndex = (currentIndex - 1 + items.Length) % items.Length;
        UpdateActiveObject();
        UpdateItemInfo();
    }

    // 切换到下一个物品（保持原逻辑，新增按钮状态重置）
    public void NextObject()
    {
        // 停止当前音频并重置按钮文本
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopVoice();
        }
        UpdateAudioButtonText();

        currentIndex = (currentIndex + 1) % items.Length;
        UpdateActiveObject();
        UpdateItemInfo();
    }

    // 更新当前显示的物品（原逻辑不变）
    private void UpdateActiveObject()
    {
        if (items == null || items.Length == 0) return;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemObject != null)
            {
                items[i].itemObject.gameObject.SetActive(i == currentIndex);
            }
        }
    }

    // 更新物品信息显示（原逻辑不变）
    private void UpdateItemInfo()
    {
        if (items == null || items.Length == 0) return;

        ItemData currentItem = items[currentIndex];
        if (nameText != null) nameText.text = currentItem.itemName;
        if (descText != null) descText.text = currentItem.itemDescription;
    }

    // 播放/暂停当前文物介绍音频（匹配切换按钮的交互逻辑）
    public void ToggleDescriptionAudio()
    {
        if (items == null || items.Length == 0 || SoundManager.Instance == null || audioButtonText == null)
            return;

        ItemData currentItem = items[currentIndex];

        if (SoundManager.Instance.IsPlaying())
        {
            // 暂停音频，更新按钮文本
            SoundManager.Instance.PauseVoice();
        }
        else
        {
            // 播放音频（无音频时提示）
            if (currentItem.descriptionAudio != null)
            {
                SoundManager.Instance.PlayVoice(currentItem.descriptionAudio);
            }
            else
            {
                Debug.LogWarning($"【{currentItem.itemName}】暂无介绍音频");
                return; // 无音频时不修改按钮文本
            }
        }

        // 同步按钮文本状态
        UpdateAudioButtonText();
    }

    // 更新音频按钮的文本（核心：匹配切换按钮的文本交互）
    private void UpdateAudioButtonText()
    {
        if (audioButtonText == null || SoundManager.Instance == null) return;

        // 根据播放状态切换文本，与切换按钮的文本逻辑一致
        audioButtonText.text = SoundManager.Instance.IsPlaying() ? pauseText : playText;
    }

    // 获取当前激活的物品（原方法不变）
    public Transform GetCurrentObject()
    {
        if (items == null || items.Length == 0) return null;
        return items[currentIndex].itemObject;
    }

    // 可选：防止按钮状态异常，每帧同步（如需严格对齐）
    void Update()
    {
        if (audioButtonText != null && SoundManager.Instance != null)
        {
            UpdateAudioButtonText();
        }
    }
}