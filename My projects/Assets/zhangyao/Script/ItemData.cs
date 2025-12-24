using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName; // 物品名称
    public string itemDescription; // 物品描述
    public Transform itemObject; // 物品对象
    public UnityEngine.AudioClip descriptionAudio; // 物品介绍音频
}