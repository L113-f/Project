using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{

    public string itemName;
    //public Sprite itemSprite;
    [SerializeField]public Image itemImage;
    [SerializeField]public Image foodImage;
    [SerializeField]private Material selectedMaterial;   
    [SerializeField]private Material unselectedMaterial;
    private bool isSelected;            

   
    public void Init(Material selectedMat, Material unselectedMat)
    {
        selectedMaterial = selectedMat;
        unselectedMaterial = unselectedMat;
        isSelected = false; // 初始未选中
        UpdateMaterial();
    }

   
    private void UpdateMaterial()
    {
        if (itemImage == null)
        {
            Debug.LogWarning($"Item {itemName} 未绑定Image组件！");
            return;
        }
        itemImage.material = isSelected ? selectedMaterial : unselectedMaterial;
    }
}

