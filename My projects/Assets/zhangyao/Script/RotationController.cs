using UnityEngine;

public class RotationController : MonoBehaviour
{
    public ObjectSwitcher objectSwitcher; // 物体切换器（在Inspector中赋值）
    public float rotationSpeed = 30f;    // 旋转速度（灵敏度）
    public Transform pivotObject;         // 旋转中心（必须在Inspector中指定）

    private Transform currentTarget;      // 当前旋转目标
    private bool isRotating = false;      // 是否正在旋转
    private Vector3 lastMousePosition;    // 上一帧鼠标位置

    private void Update()
    {
        // 刷新当前目标（始终与切换器保持同步）
        UpdateCurrentTarget();

        // 处理鼠标输入
        HandleMouseInput();

        // 如果正在旋转且目标有效，执行旋转逻辑
        if (isRotating && currentTarget != null && pivotObject != null)
        {
            RotateTarget();
        }
    }

    // 更新当前需要旋转的目标物体
    private void UpdateCurrentTarget()
    {
        if (objectSwitcher != null)
        {
            currentTarget = objectSwitcher.GetCurrentObject();
        }
    }

    // 处理鼠标输入（开始/结束旋转）
    private void HandleMouseInput()
    {
        // 鼠标左键按下：开始旋转
        if (Input.GetMouseButtonDown(0))
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }
        // 鼠标左键释放：结束旋转
        else if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }
    }

    // 执行绕 pivotObject 的旋转逻辑
    private void RotateTarget()
    {
        // 获取当前鼠标位置
        Vector3 currentMousePosition = Input.mousePosition;
        // 计算鼠标移动差值
        Vector3 mouseDelta = currentMousePosition - lastMousePosition;

        // 只有鼠标有移动时才执行旋转（避免无效计算）
        if (mouseDelta.sqrMagnitude > 0.01f)
        {
            // 水平方向鼠标移动 → 绕 pivot 的 Y 轴旋转（左右旋转）
            float yRotation = mouseDelta.x * rotationSpeed * Time.deltaTime;
            // 垂直方向鼠标移动 → 绕 pivot 的 X 轴旋转（上下旋转）
            float xRotation = -mouseDelta.y * rotationSpeed * Time.deltaTime; // 负号是为了让旋转方向符合直觉

            // 绕 pivot 的 Y 轴旋转（世界空间）
            currentTarget.RotateAround(pivotObject.position, Vector3.up, yRotation);
            // 绕 pivot 的 X 轴旋转（世界空间）
            currentTarget.RotateAround(pivotObject.position, Vector3.right, xRotation);
        }

        // 更新上一帧鼠标位置
        lastMousePosition = currentMousePosition;
    }

    // 绘制辅助线（在Scene视图中显示旋转中心与目标的连线，便于调试）
    private void OnDrawGizmos()
    {
        if (pivotObject != null && currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pivotObject.position, currentTarget.position); // 连接线
            Gizmos.DrawWireSphere(pivotObject.position, 0.1f); // 旋转中心标记
        }
    }
}