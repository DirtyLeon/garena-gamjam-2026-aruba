using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 快捷鍵測試場景切換，驗證用，上線前移除。
/// 按 N 循環切換 Scene_1 → Scene_2 → Scene_3 → Scene_4 → Scene_1 ...
/// </summary>
public class LoadSceneTest : MonoBehaviour
{
    private int currentIndex = 0;
    private const int SCENE_COUNT = 4;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
        {
            currentIndex = (currentIndex + 1) % SCENE_COUNT;
            GameScene targetScene = (GameScene)currentIndex;

            Debug.Log($"[LoadSceneTest] 切換到: {targetScene}");
            LoadSceneSystem.Instance.LoadScene(targetScene);
        }
    }
}
