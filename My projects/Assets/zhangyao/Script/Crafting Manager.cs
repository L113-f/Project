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
    public slot[] slots; // 3���ϳɲ�
    public slot[] foodslots;
    public List<Item> items; // ��slots��Ӧ���洢�������Ʒ
    public string[] recipes; // �䷽��ʽ��ÿ��Ԫ��Ϊ"��Ʒ1,��Ʒ2,��Ʒ3"
    public Item[] recipeResults; // �ϳɽ������recipes������Ӧ
    public slot ResultSlot; // �ϳɽ����

    public List<Item> availableItems; // �ɹ�ѡ�����Ʒ�б�������ά˳�����У�
    public List<Image> itemImages; // ��Ʒ��Ӧ��UIͼƬ������availableItems˳��һ�£�
    public int gridRows = 2; // ��Ʒ��������
    public int gridCols = 5; // ��Ʒ��������
    private int currentRow = 0; // ��ǰѡ����
    private int currentCol = 0; // ��ǰѡ����
    private int nextSlotIndex = 0; // ��һ��Ҫ����Ĳ�λ����

    public float highlightScale = 1.1f; // ѡ��ʱ�����ű���
    public float normalScale = 1.0f; // ����״̬�����ű���
    public float highlightBorderWidth = 3f; // ѡ��ʱ�ı߿����
    public float normalBorderWidth = 1f; // ����״̬�ı߿����
    public Color borderColor = Color.yellow; // �߿���ɫ
    private Sprite[] originalSlotSprites; // slots初始图片
    private Sprite[] originalFoodSlotSprites; // foodSlots初始图片
    public event EventHandler OnCraftSuccess;
    public event EventHandler OnCraftFailure;
    public GameObject wrong;
    public GameObject right;

    [SerializeField]private PotVisual potVisual;
    private HashSet<Item> craftedItems = new HashSet<Item>();
    [SerializeField] private GameObject craftingUI; // 需要隐藏的UI界面
    private bool allItemsCrafted = false;
    // private int currentSelectedIndex = 0;
    [System.Serializable]
    public struct CheckmarkMapping
    {
        public Item targetItem; // 对应的菜品
        public Image checkmarkImage; // 对号图片UI组件
    }

    // 在CraftingManager类中添加以下成员变量
    [Header("对号UI配置")]
    [SerializeField] private CheckmarkMapping[] checkmarkMappings; // 菜品-对号映射数组
    private Dictionary<Item, Image> itemToCheckmark = new Dictionary<Item, Image>();

    private void Awake()
    {
        Instance = this;
        foreach (var mapping in checkmarkMappings)
        {
            // 确保每个对号初始状态为隐藏
            if (mapping.checkmarkImage != null)
            {
                mapping.checkmarkImage.gameObject.SetActive(false);
            }

            // 添加到字典（避免重复添加）
            if (!itemToCheckmark.ContainsKey(mapping.targetItem))
            {
                itemToCheckmark.Add(mapping.targetItem, mapping.checkmarkImage);
            }
            else
            {
                Debug.LogWarning($"菜品 {mapping.targetItem.itemName} 存在重复的对号UI配置");
            }
        }
    }
    private void Start()
    {
        CacheOriginalSprites();

        EnsureOutlineComponents();
        foreach (var item in availableItems)
        {
            item.Init(selectedMaterial, unselectedMaterial);
        }

       // availableItems[currentSelectedIndex].SetSelected(true);
        UpdateItemHighlight();
    }
    private void CacheOriginalSprites()
    {
        // 缓存slots初始图片
        if (slots != null)
        {
            originalSlotSprites = new Sprite[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    originalSlotSprites[i] = slots[i].GetComponent<Image>().sprite;
                }
            }
        }
        if (foodslots != null)
        {
            originalFoodSlotSprites = new Sprite[foodslots.Length];
            for (int i = 0; i <foodslots.Length; i++)
            {
                
                
                    originalFoodSlotSprites[i] = foodslots[i].GetComponent<Image>().sprite;
                
            }
        }
    }
    private void Update()
    {
        HandleKeyboardInput();
        HandleMouseInput(); //���
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

    // ��WASDѡ��Enterȷ�ϣ�
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
            //����
            itemImages[selectedIndex].rectTransform.localScale = Vector3.one * highlightScale;

            Outline selectedOutline = itemImages[selectedIndex].GetComponent<Outline>();
            if (selectedOutline != null)
            {
                selectedOutline.effectDistance = new Vector2(highlightBorderWidth, highlightBorderWidth);
            }
        }
    }

    // ����Ʒ�����λ
    private void PlaceItemInSlot(Item selectedItem)
    {
        if (slots.Length == 0) return;

      
        slot targetSlot = slots[nextSlotIndex];
        slot foodSlotmax=foodslots[nextSlotIndex];
        targetSlot.item = selectedItem;
        if (targetSlot.TryGetComponent<Image>(out Image targetImage) && selectedItem.itemImage != null)
        {
            targetImage.sprite = selectedItem.itemImage.sprite;
        }
        //targetSlot.GetComponent<Image>().sprite = selectedItem.GetComponent<Image>().sprite;
        if (foodSlotmax.TryGetComponent<Image>(out Image foodImage) && selectedItem.foodImage != null)
        {
            foodImage.sprite = selectedItem.foodImage.sprite;
        }

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

    private void ClearAllSlots()
    {
        // 清空每个slot的物品和显示
        foreach (var slot in slots)
        {
            slot.item = null;
            slot.GetComponent<Image>().sprite = null;
        }

        // 清空物品列表
        for (int i = 0; i < items.Count; i++)
        {
            items[i] = null;
        }
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && i < originalSlotSprites.Length)
            {
                slots[i].GetComponent<Image>().sprite = originalSlotSprites[i];
            }
        }
        // 重置下一个slot索引
        nextSlotIndex = 0;
    }
    private bool IsAllSlotsFilled()
    {
        foreach (var slot in slots)
        {
            if (slot.item == null) return false;
        }
        return true;
    }

    // ԭ������봦���߼�
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
        Debug.Log($"��ʼ��ק��Ʒ: {item.itemName}");

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
            potVisual.PlayFoodMaking();//动画播放
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
                Debug.Log($"�ϳɳɹ�! ���: {recipeResults[i].itemName}");
                GenerateCraftResult(recipeResults[i]);
                if (!craftedItems.Contains(recipeResults[i]))
                {
                    craftedItems.Add(recipeResults[i]);
                    
                    ShowCheckmarkForItem(recipeResults[i]);
                    CheckAllItemsCrafted();
                }
                StopAllCoroutines();                    // 可选：防止多次触发叠加
                ClearAllSlots();
                ClearAllFoodSlots();
                StartCoroutine(ShowThenHide(right, 1f));

                return;
            }
        }
        OnCraftFailure?.Invoke(this, EventArgs.Empty);
        StopAllCoroutines();                        // 可选
        StartCoroutine(ShowThenHide(wrong, 1f));
        
        Debug.Log("合成失败! 无匹配配方");// ClearResultSlot();
        ClearAllSlots(); ClearAllFoodSlots();

    }
    private void ClearAllFoodSlots()
    {
        // 若foodSlots未定义则跳过
        if (foodslots == null) return;

        // 清空foodSlots显示
        foreach (var slot in foodslots)
        {
            if (slot != null)
            {
                slot.item = null;
                slot.GetComponent<Image>().sprite = null;
            }
        }
        for (int i = 0; i < foodslots.Length; i++)
        {
            if (foodslots[i] != null && i < originalFoodSlotSprites.Length)
            {
                foodslots[i].GetComponent<Image>().sprite = originalFoodSlotSprites[i];
            }
        }

    }
    private void GenerateCraftResult(Item resultItem)
    {
        if (ResultSlot != null)
        {
            ResultSlot.item = resultItem;
            ResultSlot.GetComponent<Image>().sprite = resultItem.GetComponent<Image>().sprite;
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

    IEnumerator ShowThenHide(GameObject go, float seconds)
    {   
        if (go == null) yield break;
        go.SetActive(true);
        yield return new WaitForSeconds(seconds);   // 受 Time.timeScale 影响
        go.SetActive(false);
    }
    // 1. 添加隐藏UI的方法
    private void HideCraftingUI()
    {
        if (craftingUI != null)
        {
            craftingUI.SetActive(false);
            Debug.Log("所有菜品制作完成，5秒后自动隐藏合成界面");
        }
        else
        {
            Debug.LogError("craftingUI未赋值，无法隐藏！");
        }
    }

    // 2. 修改 CheckAllItemsCrafted 方法，替换原协程逻辑
    private void CheckAllItemsCrafted()
    {
        bool allCrafted = true;
        foreach (var recipeResult in recipeResults)
        {
            if (!craftedItems.Contains(recipeResult))
            {
                allCrafted = false;
                Debug.LogWarning($"未完成的菜品：{recipeResult.itemName}");
                break;
            }
        }

        if (allCrafted && !allItemsCrafted)
        {
            allItemsCrafted = true;
            Debug.Log("所有菜品已制作完成，准备5秒后隐藏UI");
            if (craftingUI != null)
            {
                // 取消可能存在的重复调用（避免多次触发时时间错乱）
                CancelInvoke("HideCraftingUI");
                // 5秒后调用 HideCraftingUI 方法
                Invoke("HideCraftingUI", 5f);
                Debug.Log("已设置5秒后隐藏UI");
            }
            else
            {
                Debug.LogError("craftingUI未赋值，无法设置延迟隐藏！");
            }
        }
    }


    private void ShowCheckmarkForItem(Item item)
    {
        if (itemToCheckmark.TryGetValue(item, out Image checkmark))
        {
            if (checkmark != null)
            {
                checkmark.gameObject.SetActive(true); // 显示对号，且不会再隐藏
                Debug.Log($"显示菜品 {item.itemName} 的对号UI");
            }
            else
            {
                Debug.LogWarning($"菜品 {item.itemName} 的对号UI未配置");
            }
        }
        else
        {
            Debug.LogWarning($"未找到菜品 {item.itemName} 对应的对号UI配置");
        }
    }
}
