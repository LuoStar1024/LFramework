# 音频

## GameLogic 推荐用法

- GameLogic/业务代码中优先使用 `GameEntry.Audio` 扩展方法，不要直接拼接音频资源路径并调用 `IAudioManager.PlayAudio`。
- `GameEntry.Audio.PlayBgm(musicId, userData = null)`：停止当前 BGM，读取 `TbSound`，构建 BGM 资源路径，循环播放 2D 音乐，并记录 BGM 序列编号。
- `GameEntry.Audio.StopBgm()`：以淡出方式停止已记录的 BGM，并清空已存储的序列编号。
- `GameEntry.Audio.PlaySound(audioId, bindingTrans = null, userData = null)`：读取 `TbSound`，构建音效资源路径，并可选择将播放绑定到 Transform。
- `GameEntry.Audio.PlayUISound(uiSoundId, userData = null)`：读取 `TbSound`，构建 UI 音效资源路径，并播放非循环 2D UI 音效。
- `GameEntry.Audio.IsMuted(audioGroupName)`, `Mute(audioGroupName, mute)`：读取或更新音频组的静音标记；`Mute` 会持久化到 `GameEntry.Setting`。
- `GameEntry.Audio.GetVolume(audioGroupName)`, `SetVolume(audioGroupName, volume)`：读取或更新音频组音量；`SetVolume` 会持久化到 `GameEntry.Setting`。

## 注意事项

- `AudioExtension` 读取 `GameEntry.DataTable.TbSound`，通过 `AssetUtility` 构建资源路径，创建 `PlayAudioParams`，并调用 `IAudioManager.PlayAudio`。
- 停止 BGM 优先使用 `GameEntry.Audio.StopBgm()`，确保已存储的 BGM 序列状态保持正确。
- 手动使用底层播放 API 前，先查源码确认实际重载、`PlayAudioParams` 字段和音频组名称。

## IAudioManager API 速查

仅在框架集成代码或 `GameEntry.Audio` 扩展方法内部优先考虑直接使用 `IAudioManager`。

- 音频组：`HasAudioGroup`, `GetAudioGroup`, `GetAllAudioGroups`, `AddAudioGroup`。
- 播放：`PlayAudio(audioAssetName, audioGroupName, ...)` 返回序列编号。根据优先级、`PlayAudioParams`、绑定 Transform、世界坐标或 `userData` 需求选择匹配的重载。
- 停止/加载控制：`StopAudio(serialId[, fadeOutSeconds])`, `StopAllLoadedAudios([fadeOutSeconds])`, `StopAllLoadingAudios()`。
- 暂停/恢复：`PauseAudio(serialId[, fadeOutSeconds])`, `ResumeAudio(serialId[, fadeInSeconds])`。
- 音频组设置：`IAudioGroup.Mute`, `IAudioGroup.Volume`。
- 单次播放设置：通过 `PlayAudioParams.Create()` 创建 `PlayAudioParams`，然后设置 `Priority`, `Loop`, `VolumeInAudioGroup`, `FadeInSeconds`, `SpatialBlend` 等字段。

## 源码路径

- `Assets/GameScripts/GameLogic/Component/Audio/AudioExtension.cs`
- `Assets/LFramework/Runtime/Component/Audio/IAudioManager.cs`
- `Assets/LFramework/Runtime/Component/Audio/PlayAudioParams.cs`
- `Assets/LFramework/Runtime/Component/Audio/AudioComponent.cs`
