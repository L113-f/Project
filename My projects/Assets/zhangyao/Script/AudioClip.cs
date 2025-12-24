using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AudioData : ScriptableObject // 重命名类以避免与Unity的AudioClip冲突
{
    public UnityEngine.AudioClip[] itemDescriptions; // 文物介绍音频
    public UnityEngine.AudioClip[] sound1;
    public UnityEngine.AudioClip[] sound2;
    public UnityEngine.AudioClip[] sound3;
}