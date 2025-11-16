using UnityEngine;
using TMPro;

public class ObjectSwitcher : MonoBehaviour
{
    public ItemData[] items; // 物品数据数组（在Inspector中设置）
    public TextMeshProUGUI nameText; // 物品名称文本组件引用
    public TextMeshProUGUI descText; // 物品介绍文本组件引用
    private int currentIndex = 0; // 当前显示的物品索引

    void Start()
    {
        // 初始化只显示第一个物品，隐藏其他物品
        UpdateActiveObject();
        UpdateItemInfo(); // 初始化显示物品信息
    }

    // 切换到上一个物品（绑定到 "<" 按钮）
    public void PreviousObject()
    {
        currentIndex = (currentIndex - 1 + items.Length) % items.Length;
        UpdateActiveObject();
        UpdateItemInfo();
    }

    // 切换到下一个物品（绑定到 ">" 按钮）
    public void NextObject()
    {
        currentIndex = (currentIndex + 1) % items.Length;
        UpdateActiveObject();
        UpdateItemInfo();
    }

    // 更新当前显示的物品（在场景中）
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

    // 更新物品信息显示（名称和介绍）
    private void UpdateItemInfo()
    {
        if (items == null || items.Length == 0) return;

        ItemData currentItem = items[currentIndex];

        if (nameText != null)
            nameText.text = currentItem.itemName;

        if (descText != null)
            descText.text = currentItem.itemDescription;
    }

    // 供RotationController获取当前激活的物品
    public Transform GetCurrentObject()
    {
        if (items == null || items.Length == 0) return null;
        return items[currentIndex].itemObject;
    }
}