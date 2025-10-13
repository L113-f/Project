using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMananger : MonoBehaviour
{
    public static CraftingMananger Instance { get; private set; }
    public Material selectedMaterial;
    public Material unselectedMaterial;
    private Item currentItem;
    public Image customCursor;
    public slot[] slots; // 3个合成槽
    public List<Item> items; // 与slots对应，存储放入的物品
    public string[] recipes; // 配方格式，每个元素为"物品1,物品2,物品3"
    public Item[] recipeResults; // 合成结果，与recipes索引对应
    public slot ResultSlot; // 合成结果槽

    public List<Item> availableItems; // 可供选择的物品列表（按二维顺序排列）
    public List<Image> itemImages; // 物品对应的UI图片（需与availableItems顺序一致）
    public int gridRows = 2; // 物品网格行数
    public int gridCols = 4; // 物品网格列数
    private int currentRow = 0; // 当前选中行
    private int currentCol = 0; // 当前选中列
    private int nextSlotIndex = 0; // 下一个要放入的槽位索引

    public float highlightScale = 1.1f; // 选中时的缩放比例
    public float normalScale = 1.0f; // 正常状态的缩放比例
    public float highlightBorderWidth = 3f; // 选中时的边框宽度
    public float normalBorderWidth = 1f; // 正常状态的边框宽度
    public Color borderColor = Color.yellow; // 边框颜色
   
    public event EventHandler OnCraftSuccess;
    public event EventHandler OnCraftFailure;
    // private int currentSelectedIndex = 0;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
    
        EnsureOutlineComponents();
        foreach (var item in availableItems)
        {
            item.Init(selectedMaterial, unselectedMaterial);
        }

       // availableItems[currentSelectedIndex].SetSelected(true);
        UpdateItemHighlight();
    }

    private void Update()
    {
        HandleKeyboardInput();
        HandleMouseInput(); //鼠标
    }

    private void EnsureOutlineComponents()
    {
        foreach (var image in itemImages)
        {
            if (image != null && image.GetComponent<Outline>() == null)
            {
                Outline outline = image.gameObject.AddComponent<Outline>();
                outline.effectColor = borderColor;
                outline.effectDistance = new Vector2(normalBorderWidth, normalBorderWidth);
            }
        }
    }

    // （WASD选择，Enter确认）
    private void HandleKeyboardInput()
    {
        if (availableItems == null || availableItems.Count == 0) return;

        int maxIndex = availableItems.Count - 1;
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentRow = (currentRow - 1 + gridRows) % gridRows;
            UpdateItemHighlight();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            currentRow = (currentRow + 1) % gridRows;
            UpdateItemHighlight();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            currentCol = (currentCol - 1 + gridCols) % gridCols;
            UpdateItemHighlight();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            currentCol = (currentCol + 1) % gridCols;
            UpdateItemHighlight();
        }

     
        if (Input.GetKeyDown(KeyCode.Return))
        {
            int selectedIndex = currentRow * gridCols + currentCol;
            if (selectedIndex < availableItems.Count)
            {
                PlaceItemInSlot(availableItems[selectedIndex]);
            }
        }
    }

    private void UpdateItemHighlight()
    {
        
        int selectedIndex = currentRow * gridCols + currentCol;
        //Debug.Log(selectedIndex);
        //availableItems[selectedIndex]
        for (int i = 0; i < itemImages.Count; i++)
        {
            if (itemImages[i] != null)
            {
                itemImages[i].rectTransform.localScale = Vector3.one * normalScale;

                Outline outline = itemImages[i].GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectDistance = new Vector2(normalBorderWidth, normalBorderWidth);
                }
            }
        }

        if (selectedIndex < itemImages.Count && itemImages[selectedIndex] != null)
        {
            //缩放
            itemImages[selectedIndex].rectTransform.localScale = Vector3.one * highlightScale;

            Outline selectedOutline = itemImages[selectedIndex].GetComponent<Outline>();
            if (selectedOutline != null)
            {
                selectedOutline.effectDistance = new Vector2(highlightBorderWidth, highlightBorderWidth);
            }
        }
    }

    // 将物品放入槽位
    private void PlaceItemInSlot(Item selectedItem)
    {
        if (slots.Length == 0) return;

      
        slot targetSlot = slots[nextSlotIndex];
        targetSlot.item = selectedItem;
        targetSlot.GetComponent<Image>().sprite = selectedItem.GetComponent<Image>().sprite;

    
        if (nextSlotIndex < items.Count)
        {
            items[nextSlotIndex] = selectedItem;
        }

        if (IsAllSlotsFilled())
        {
            CheckCraftingCondition();
        }

        nextSlotIndex = (nextSlotIndex + 1) % slots.Length;
    }

    
    private bool IsAllSlotsFilled()
    {
        foreach (var slot in slots)
        {
            if (slot.item == null) return false;
        }
        return true;
    }

    // 原鼠标输入处理逻辑
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (currentItem != null)
            {
                customCursor.gameObject.SetActive(false);
                slot nearestSlot = FindNearestSlot();

                if (nearestSlot != null)
                {
                    nearestSlot.item = currentItem;
                    nearestSlot.GetComponent<Image>().sprite = currentItem.GetComponent<Image>().sprite;
                    if (nearestSlot.index >= 0 && nearestSlot.index < items.Count)
                    {
                        items[nearestSlot.index] = currentItem;
                    }
                    CheckCraftingCondition();
                }
            }
        }
    }

    private slot FindNearestSlot()
    {
        slot nearestSlot = null;
        float shortestDistance = float.MaxValue;

        foreach (slot slot in slots)
        {
            float dist = Vector2.Distance(Input.mousePosition, slot.transform.position);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                nearestSlot = slot;
            }
        }
        return nearestSlot;
    }

    public void OnMouseDownItem(Item item)
    {
        Debug.Log($"开始拖拽物品: {item.itemName}");

        if (currentItem == null || currentItem != item)
        {
          
            currentItem = item;
            customCursor.gameObject.SetActive(true);
            customCursor.sprite = currentItem.GetComponent<Image>().sprite;
        }
        else
        {
            currentItem = null;
            customCursor.gameObject.SetActive(false);
        }
    }

    private void CheckCraftingCondition()
    {
        List<string> currentIngredients = new List<string>();
        foreach (var slot in slots)
        {
            if (slot.item != null)
            {
                currentIngredients.Add(slot.item.itemName);
                //Debug.Log(slot.item.itemName);
            }
        }

        if (currentIngredients.Count == 3)
        {
            TryMatchRecipe(currentIngredients);
        }
    }

    private void TryMatchRecipe(List<string> currentIngredients)
    {
        currentIngredients.Sort();
        string sortedCurrent = string.Join(",", currentIngredients);

        for (int i = 0; i < recipes.Length; i++)
        {
            if (string.IsNullOrEmpty(recipes[i])) continue;

            List<string> recipeIngredients = new List<string>(recipes[i].Split(','));
            recipeIngredients.Sort();
            string sortedRecipe = string.Join(",", recipeIngredients);

            if (sortedCurrent == sortedRecipe)
            {
                OnCraftSuccess?.Invoke(this, EventArgs.Empty);
                Debug.Log($"合成成功! 获得: {recipeResults[i].itemName}");
                GenerateCraftResult(recipeResults[i]);
                return;
            }
        }
        OnCraftFailure?.Invoke(this, EventArgs.Empty);
        ClearResultSlot();
        Debug.Log("合成失败! 配方不匹配（槽位保持不变）");
        
    }

    private void GenerateCraftResult(Item resultItem)
    {
        if (ResultSlot != null)
        {
            ResultSlot.item = resultItem;
            ResultSlot.GetComponent<Image>().sprite = resultItem.GetComponent<Image>().sprite;
        }

        foreach (var slot in slots)
        {
            slot.item = null;
            slot.GetComponent<Image>().sprite = null;
        }

        for (int i = 0; i < items.Count; i++)
        {
            items[i] = null;
        }

        nextSlotIndex = 0;
        StartCoroutine(ClearResultAfterDelay(3f));
    }
    private IEnumerator ClearResultAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearResultSlot();
    }
    public void ClearResultSlot()
    {
        if (ResultSlot != null)
        {
            ResultSlot.item = null;
            ResultSlot.GetComponent<Image>().sprite = null;
        }
    }
}
