using UnityEngine;

public class RotationController : MonoBehaviour
{
    public ObjectSwitcher objectSwitcher; // 物体切换器（在Inspector中赋值）
    public float rotationSpeed = 100f;    // 旋转速度

    private Transform currentTarget;      // 当前旋转目标
    private bool isRotating = false;      // 是否正在旋转
    private Vector3 lastMousePosition;    // 上一帧鼠标位置
    private Renderer[] targetRenderers;   // 目标物体的所有渲染器（用于计算边界）

    void Update()
    {
        // 自动获取当前激活的物体
        currentTarget = objectSwitcher?.GetCurrentObject();
        if (currentTarget == null) return;

        // 初始化渲染器数组（初次加载或切换目标时执行）
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = currentTarget.GetComponentsInChildren<Renderer>();
        }

        // 处理旋转开始
        if (Input.GetMouseButtonDown(0) && IsMouseOverRawImage())
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }

        // 执行绕中心点的旋转
        if (isRotating)
        {
            Vector3 deltaMouse = Input.mousePosition - lastMousePosition;
            float rotationX = deltaMouse.y * rotationSpeed * Time.deltaTime;
            float rotationY = deltaMouse.x * rotationSpeed * Time.deltaTime;

            // 获取自动计算的3D中心点
            Vector3 pivotPoint = GetAutoCalculatedPivot();

            // 绕中心点旋转的核心逻辑：平移、旋转、再平移
            RotateAroundPivot(currentTarget, pivotPoint, Vector3.right, -rotationX);
            RotateAroundPivot(currentTarget, pivotPoint, Vector3.up, rotationY);

            lastMousePosition = Input.mousePosition;
        }
    }

    /// <summary>
    /// 自动计算物体的3D中心点（基于所有渲染器的边界）
    /// </summary>
    private Vector3 GetAutoCalculatedPivot()
    {
        // 如果没有渲染器，使用物体自身位置作为 fallback
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            return currentTarget.position;
        }

        // 合并所有渲染器的边界来计算整体中心点
        Bounds totalBounds = targetRenderers[0].bounds;
        foreach (Renderer renderer in targetRenderers)
        {
            totalBounds.Encapsulate(renderer.bounds);
        }
        return totalBounds.center;
    }

    /// <summary>
    /// 绕指定中心点旋转物体
    /// </summary>
    private void RotateAroundPivot(Transform target, Vector3 pivot, Vector3 axis, float angle)
    {
        // 1. 平移物体使中心点位于原点
        Vector3 offset = target.position - pivot;
        target.position = pivot;

        // 2. 执行旋转（绕世界坐标系）
        target.Rotate(axis, angle, Space.World);

        // 3. 平移回原始偏移位置
        target.position += offset;
    }

    // 检查鼠标是否在RawImage上方
    private bool IsMouseOverRawImage()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;

        UnityEngine.EventSystems.PointerEventData pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        pointerData.position = Input.mousePosition;

        System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult> results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<UnityEngine.UI.RawImage>() != null)
            {
                return true;
            }
        }
        return false;
    }

    // 切换目标物体时重置渲染器数组（确保重新计算中心点）
    public void OnTargetSwitched()
    {
        targetRenderers = null;
    }
}