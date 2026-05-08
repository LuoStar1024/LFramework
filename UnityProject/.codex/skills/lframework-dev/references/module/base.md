# Base

## GameLogic 推荐用法

- GameLogic/业务代码中优先使用 `GameEntry.Base` 访问基础运行时设置，不要直接查找 `BaseComponent` 或自行修改 Unity 全局状态。
- `GameEntry.Base.FrameRate`：获取或设置目标帧率；设置时会同步更新 `Application.targetFrameRate`。
- `GameEntry.Base.GameSpeed`：获取或设置游戏速度；设置时会同步更新 `Time.timeScale`，负数会被钳制为 `0f`。
- `GameEntry.Base.IsGamePaused`：判断当前游戏速度是否小于等于 `0f`。
- `GameEntry.Base.IsNormalGameSpeed`：判断当前游戏速度是否为 `1f`。
- `GameEntry.Base.RunInBackground`：获取或设置是否允许后台运行；设置时会同步更新 `Application.runInBackground`。
- `GameEntry.Base.NeverSleep`：获取或设置是否禁止休眠；设置时会同步更新 `Screen.sleepTimeout`。
- `GameEntry.Base.PauseGame()`：记录暂停前的游戏速度，并将 `GameSpeed` 设置为 `0f`。
- `GameEntry.Base.ResumeGame()`：在游戏暂停时恢复到 `PauseGame()` 记录的暂停前速度。
- `GameEntry.Base.ResetNormalGameSpeed()`：将游戏速度重置为 `1f`。

## 注意事项

- `BaseComponent` 是应用级运行时设置持有者，适合暴露帧率、时间缩放、后台运行和休眠策略，不应扩展为玩法服务或跨模块业务管理器。
- 暂停和恢复应优先使用 `PauseGame()` / `ResumeGame()`，避免业务代码直接把 `GameSpeed` 改为 `0f` 后丢失暂停前速度。

## IBaseManager API 速查

仅在框架集成代码或 `GameEntry` 尚未初始化时优先考虑直接使用 `IBaseManager`。

- 运行控制：`FrameRate`, `GameSpeed`, `RunInBackground`, `NeverSleep`。
- 状态查询：`IsGamePaused`, `IsNormalGameSpeed`。
- 速度控制：`PauseGame()`, `ResumeGame()`, `ResetNormalGameSpeed()`。

## 源码路径

- `Assets/LFramework/Runtime/Component/Base/BaseComponent.cs`
- `Assets/LFramework/Runtime/Component/Base/IBaseManager.cs`
