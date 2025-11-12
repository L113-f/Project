using UnityEngine;
using UnityEngine.UI;

public class CloseCanvas : MonoBehaviour
{
    [SerializeField] private GameObject canvasToClose;

    void Start()
    {
        // 获取按钮组件并添加点击事件监听
        Button closeButton = GetComponent<Button>();
        closeButton.onClick.AddListener(CloseCanvasObject);
    }

    public void CloseCanvasObject()
    {
        // 禁用Canvas GameObject
        if (canvasToClose != null)
        {
            canvasToClose.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Canvas to close is not assigned!");
        }
    }
}