// 创建一个新的文件：ExhibitItemSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewExhibitItem", menuName = "文物/文物数据")]
public class ExhibitItemSO : ScriptableObject
{
    public string itemName;
    [TextArea(3, 10)]
    public string itemDescription;
    public GameObject itemPrefab;  // 改为 Prefab 引用
    public AudioClip itemVoice;
}