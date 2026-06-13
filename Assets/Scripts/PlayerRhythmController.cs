using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRhythmController : MonoBehaviour
{
    private void Update()
    {
        // 優先檢查新版 Input System 的鍵盤輸入 (如果鍵盤存在)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.wasPressedThisFrame) TriggerInput(RhythmDirection.W);
            else if (Keyboard.current.aKey.wasPressedThisFrame) TriggerInput(RhythmDirection.A);
            else if (Keyboard.current.dKey.wasPressedThisFrame) TriggerInput(RhythmDirection.D);
            else if (Keyboard.current.qKey.wasPressedThisFrame) TriggerInput(RhythmDirection.Q);
            else if (Keyboard.current.eKey.wasPressedThisFrame) TriggerInput(RhythmDirection.E);
            else if (Keyboard.current.zKey.wasPressedThisFrame) TriggerInput(RhythmDirection.Z);
            else if (Keyboard.current.xKey.wasPressedThisFrame) TriggerInput(RhythmDirection.X);
            else if (Keyboard.current.cKey.wasPressedThisFrame) TriggerInput(RhythmDirection.C);
        }
        else
        {
            // 備用的舊版 Input Manager 鍵盤輸入
            if (Input.GetKeyDown(KeyCode.W)) TriggerInput(RhythmDirection.W);
            else if (Input.GetKeyDown(KeyCode.A)) TriggerInput(RhythmDirection.A);
            else if (Input.GetKeyDown(KeyCode.D)) TriggerInput(RhythmDirection.D);
            else if (Input.GetKeyDown(KeyCode.Q)) TriggerInput(RhythmDirection.Q);
            else if (Input.GetKeyDown(KeyCode.E)) TriggerInput(RhythmDirection.E);
            else if (Input.GetKeyDown(KeyCode.Z)) TriggerInput(RhythmDirection.Z);
            else if (Input.GetKeyDown(KeyCode.X)) TriggerInput(RhythmDirection.X);
            else if (Input.GetKeyDown(KeyCode.C)) TriggerInput(RhythmDirection.C);
        }
    }

    /// <summary>
    /// 供畫面上的 UI 方向按鈕（Button 組件的 OnClick() 事件）調用
    /// </summary>
    /// <param name="direction">要輸入的方向</param>
    public void PressDirectionFromUI(RhythmDirection direction)
    {
        TriggerInput(direction);
    }

    /// <summary>
    /// 供畫面上的 UI 按鈕透過字串形式調用 (例如在 Unity 編輯器 Inspector 傳入 "W", "A", "Q" 等)
    /// </summary>
    /// <param name="directionStr">方向的字串名稱</param>
    public void PressDirectionFromUIString(string directionStr)
    {
        if (System.Enum.TryParse(directionStr, true, out RhythmDirection dir))
        {
            TriggerInput(dir);
        }
        else
        {
            Debug.LogWarning($"PlayerRhythmController: 無法解析的 UI 方向輸入字串: {directionStr}");
        }
    }

    private void TriggerInput(RhythmDirection direction)
    {
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.RegisterPlayerInput(direction);
        }
    }
}
