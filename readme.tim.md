# garena-2026-aruba

Garena Game Jam 2026 參賽作品。目前處於早期開發階段。

---

## 技術堆疊

- **引擎**: Unity 6000.3.16f1
- **渲染管線**: Universal Render Pipeline (URP) 17.3.0
- **輸入系統**: Unity New Input System 1.19.0
- **目標平台**: WebGL
- **團隊**: dirty-fonne-coop

---

## 專案結構

```
Assets/
├── InputSystem_Actions.inputactions  ← 玩家輸入設定
├── Scenes/SampleScene.unity          ← 預設場景
├── TempLevel/Test_Level.unity        ← 測試場景（地板 + 3 個 Cube）
├── Settings/                         ← PC 與 Mobile URP 渲染設定
└── TutorialInfo/                     ← URP 模板樣板（可刪除）
```

---

## 遊戲類型

3D 動作 / 冒險遊戲（根據 Input Map 推測）

---

## 輸入對照表

| 動作 | 鍵盤 | 手把 |
|------|------|------|
| 移動 | WASD / 方向鍵 | 左搖桿 |
| 視角 | 滑鼠移動 | 右搖桿 |
| 攻擊 | 左鍵 / Enter | X 鍵 |
| 跳躍 | Space | A 鍵 |
| 衝刺 | Left Shift | 左搖桿按下 |
| 互動 | E（長按） | Y 鍵（長按） |
| 蹲下 | C | B 鍵 |
| 切換上一個 | 1 | D-Pad 左 |
| 切換下一個 | 2 | D-Pad 右 |

---

## 重要套件

| 套件 | 版本 | 用途 |
|------|------|------|
| com.unity.render-pipelines.universal | 17.3.0 | URP 渲染 |
| com.unity.inputsystem | 1.19.0 | 新版輸入系統 |
| com.unity.ai.navigation | 2.0.12 | NavMesh AI 尋路 |
| com.unity.timeline | 1.8.12 | 過場動畫 / 劇情演出 |
| com.unity.multiplayer.center | 1.0.1 | 多人遊戲工具 |
| com.unity.visualscripting | 1.9.11 | 視覺化腳本 |

---

## 開發進度

### 已完成
- Unity 6 URP 專案初始化
- WebGL 發布設定
- 完整輸入系統配置（鍵盤、手把、觸控、XR）
- PC / Mobile 雙品質渲染設定
- 基礎測試場景

### 待辦
- [ ] 玩家控制器腳本
- [ ] 角色模型與動畫
- [ ] 遊戲核心機制
- [ ] 敵人 AI（NavMesh）
- [ ] UI 系統
- [ ] 遊戲內容與關卡設計

---

## Git 歷史摘要

```
37ed612  Merge pull request #1 from DirtyLeon/HankBranch
729bf94  test level test
348d716  Project init
b700935  Initial commit
```
