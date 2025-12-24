using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // 单例实例
    private AudioSource voiceSource; // 语音播放器
    private AudioSource bgmSource; // 背景音播放器

    [SerializeField] private AudioClip bgmClip; // 背景音乐
    [Range(0f, 1f)] public float normalBgmVolume = 1.0f; // 正常BGM音量
    [Range(0f, 1f)] public float reducedBgmVolume = 0.3f; // 降低后的BGM音量

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // 初始化音频源
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            voiceSource.loop = false;

            // 初始化BGM源
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = bgmClip;
            bgmSource.playOnAwake = true;
            bgmSource.loop = true;
            bgmSource.volume = normalBgmVolume;
            bgmSource.Play();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 播放随机音效
    private void PlaySound(AudioClip[] clips, Vector3 position, float volume = 1.0f)
    {
        int index = Random.Range(0, clips.Length);
        AudioSource.PlayClipAtPoint(clips[index], position, volume);
    }

    // 播放语音
    public void PlayVoice(AudioClip voiceClip)
    {
        if (voiceClip == null)
        {
            Debug.LogWarning("语音资源为空");
            return;
        }

        // 降低BGM音量
        bgmSource.volume = reducedBgmVolume;
        // 停止当前语音并播放新语音
        StopVoice();
        voiceSource.clip = voiceClip;
        voiceSource.Play();
        // 开始监听语音播放结束
        StartCoroutine(CheckVoiceFinished());
    }

    // 检查语音是否播放完毕
    private IEnumerator CheckVoiceFinished()
    {
        while (voiceSource.isPlaying)
        {
            yield return null;
        }
        // 语音播放完毕，恢复BGM音量
        bgmSource.volume = normalBgmVolume;
    }

    // 暂停语音
    public void PauseVoice()
    {
        if (voiceSource.isPlaying)
        {
            voiceSource.Pause();
            // 恢复BGM音量
            bgmSource.volume = normalBgmVolume;
        }
    }

    // 恢复语音播放
    public void ResumeVoice()
    {
        if (!voiceSource.isPlaying && voiceSource.clip != null)
        {
            voiceSource.UnPause();
            // 降低BGM音量
            bgmSource.volume = reducedBgmVolume;
        }
    }

    // 停止语音
    public void StopVoice()
    {
        if (voiceSource.clip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = null;
            // 恢复BGM音量
            bgmSource.volume = normalBgmVolume;
        }
    }

    // 检查是否正在播放语音
    public bool IsPlaying()
    {
        return voiceSource.isPlaying;
    }
}