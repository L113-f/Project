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
   
    public event EventHandler OnCraftSuccess;
    public event EventHandler OnCraftFailure;
    public GameObject wrong;
    public GameObject right;
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
               
                StopAllCoroutines();                    // 可选：防止多次触发叠加
                StartCoroutine(ShowThenHide(right, 1f));

                return;
            }
        }
        OnCraftFailure?.Invoke(this, EventArgs.Empty);
        StopAllCoroutines();                        // 可选
        StartCoroutine(ShowThenHide(wrong, 1f));
        ClearResultSlot();
        Debug.Log("�ϳ�ʧ��! �䷽��ƥ�䣨��λ���ֲ��䣩");
        
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

    IEnumerator ShowThenHide(GameObject go, float seconds)
    {   
        if (go == null) yield break;
        go.SetActive(true);
        yield return new WaitForSeconds(seconds);   // 受 Time.timeScale 影响
        go.SetActive(false);
    }

    
}
