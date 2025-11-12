using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    // 存储所有摄像机的列表
    public Camera[] cameras;
    // 当前活跃摄像机的索引
    private int currentCameraIndex = 0;

    void Start()
    {
        // 游戏开始时，确保只有列表中的第一台摄像机是启用的
        if (cameras.Length > 0)
        {
            // 禁用所有摄像机
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null)
                {
                    cameras[i].enabled = false;
                    // 确保Audio Listener也被禁用
                    AudioListener audioListener = cameras[i].GetComponent<AudioListener>();
                    if (audioListener != null) audioListener.enabled = false;
                }
            }
            // 然后启用第一台
            EnableCamera(0);
        }
    }

    void Update()
    {
        // 键盘快捷键测试（按C键切换摄像机）
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchToNextCamera();
        }
    }

    // 切换到下一台摄像机（公共方法，可供UI按钮调用）
    public void SwitchToNextCamera()
    {
        int nextCameraIndex = (currentCameraIndex + 1) % cameras.Length;
        SwitchToCamera(nextCameraIndex);
    }

    // 核心方法：切换到指定索引的摄像机
    public void SwitchToCamera(int newCameraIndex)
    {
        // 安全检查
        if (newCameraIndex < 0 || newCameraIndex >= cameras.Length || cameras[newCameraIndex] == null)
        {
            Debug.LogError("无效的摄像机索引: " + newCameraIndex);
            return;
        }

        // 1. 禁用当前摄像机（包括Audio Listener）
        DisableCamera(currentCameraIndex);

        // 2. 启用新的摄像机
        EnableCamera(newCameraIndex);

        // 3. 更新索引
        currentCameraIndex = newCameraIndex;

        Debug.Log("切换到摄像机: " + cameras[currentCameraIndex].name);
    }

    // 启用指定索引的摄像机
    private void EnableCamera(int index)
    {
        if (cameras[index] != null)
        {
            cameras[index].enabled = true;
            // 启用Audio Listener
            AudioListener audioListener = cameras[index].GetComponent<AudioListener>();
            if (audioListener != null) audioListener.enabled = true;
        }
    }

    // 禁用指定索引的摄像机
    private void DisableCamera(int index)
    {
        if (cameras[index] != null)
        {
            cameras[index].enabled = false;
            // 禁用Audio Listener
            AudioListener audioListener = cameras[index].GetComponent<AudioListener>();
            if (audioListener != null) audioListener.enabled = false;
        }
    }
}
