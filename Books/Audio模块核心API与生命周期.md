# Audio 模块核心 API 与生命周期

## 1. 文档目的

本文面向后续阅读和维护 Audio 模块的开发者，重点说明：

- 当前模块的核心类型与职责；
- 对外可用 API；
- 模块内部关键调用链；
- 如果需要接入或扩展时，必须理解的生命周期。

---

## 2. 模块核心结构

当前 Audio 模块核心关系如下：

```text
GameEntry.Audio / LFrameworkEntry.GetModule<IAudioManager>()
        ↓
AudioComponent
        ↓
AudioGroup
        ↓
AudioAgent
        ↓
Unity AudioSource
```

同时，业务层通过 `AudioExtension` 对常用场景做了二次封装。

---

## 3. 核心类型说明

## 3.1 AudioComponent

文件：

- `Assets/LFramework/Runtime/Component/Audio/AudioComponent.cs`

定义：

```csharp
public sealed partial class AudioComponent : MonoBehaviour, ILFrameworkModule, IAudioManager, IAudioRelease
```

职责：

- 作为整个 Audio 模块入口；
- 注册并暴露 `IAudioManager`；
- 管理声音组；
- 发起音频资源异步加载；
- 在加载完成后把播放请求路由给目标 `AudioGroup`；
- 对外提供停止、暂停、恢复、查询加载中的音频等能力；
- 负责释放音频资源。

说明：

- `sealed`，当前设计不以继承扩展为主；
- 业务层通常通过接口使用，而不是继承它。

---

## 3.2 AudioGroup

文件：

- `Assets/LFramework/Runtime/Component/Audio/AudioGroup.cs`

定义：

```csharp
public sealed class AudioGroup : MonoBehaviour, IAudioGroup
```

职责：

- 表示一个逻辑声音组；
- 管理多个 `AudioAgent`；
- 处理组级别静音、音量；
- 根据优先级和占用状态选择可用代理。

常见组：

- `Bgm`
- `Sound`
- `UISound`
- `Default`（启动器）

---

## 3.3 AudioAgent

文件：

- `Assets/LFramework/Runtime/Component/Audio/AudioAgent.cs`

定义：

```csharp
public sealed class AudioAgent : MonoBehaviour, IAudioAgent
```

职责：

- 真正承载播放行为；
- 内部封装一个 `AudioSource`；
- 处理单条音频的播放、停止、暂停、恢复；
- 处理淡入淡出；
- 跟随绑定对象或设置世界坐标；
- 播放结束后重置自身并释放资源。

说明：

- 也是 `sealed`；
- 业务方一般不会直接创建它，而是由 `AudioGroup.AddAudioAgentHelper()` 自动生成。

---

## 3.4 PlayAudioParams

文件：

- `Assets/LFramework/Runtime/Component/Audio/PlayAudioParams.cs`

职责：

- 承载播放参数；
- 支持通过 `ReferencePool` 复用；
- 用于替代大量离散参数。

常用字段：

- `Time`
- `MuteInAudioGroup`
- `Loop`
- `Priority`
- `VolumeInAudioGroup`
- `FadeInSeconds`
- `Pitch`
- `PanStereo`
- `SpatialBlend`
- `MaxDistance`
- `DopplerLevel`

---

## 3.5 AudioExtension

文件：

- `Assets/GameScripts/GameLogic/Component/Audio/AudioExtension.cs`

职责：

- 业务层便捷调用封装；
- 把数据表配置转换成 `PlayAudioParams`；
- 封装 BGM、普通音效、UI 音效的调用方式；
- 对组的静音和音量做设置存档。

适合业务方直接使用的入口：

- `PlayBgm`
- `StopBgm`
- `PlaySound`
- `PlayUISound`
- `Mute`
- `SetVolume`

---

## 4. 核心 API

## 4.1 IAudioManager

文件：

- `Assets/LFramework/Runtime/Component/Audio/IAudioManager.cs`

### 4.1.1 组管理相关

| API                                                                                                                                    | 说明           |
| -------------------------------------------------------------------------------------------------------------------------------------- | ------------ |
| `int AudioGroupCount`                                                                                                                  | 当前声音组数量      |
| `bool HasAudioGroup(string audioGroupName)`                                                                                            | 是否存在指定声音组    |
| `IAudioGroup GetAudioGroup(string audioGroupName)`                                                                                     | 获取指定声音组      |
| `IAudioGroup[] GetAllAudioGroups()`                                                                                                    | 获取全部声音组      |
| `void GetAllAudioGroups(List<IAudioGroup> results)`                                                                                    | 将全部声音组写入外部列表 |
| `bool AddAudioGroup(string audioGroupName, int audioAgentHelperCount)`                                                                 | 创建声音组并指定代理数量 |
| `bool AddAudioGroup(string audioGroupName, bool avoidBeingReplacedBySamePriority, bool mute, float volume, int audioAgentHelperCount)` | 创建声音组并指定完整参数 |

### 4.1.2 加载状态查询

| API                                                   | 说明             |
| ----------------------------------------------------- | -------------- |
| `int[] GetAllLoadingAudioSerialIds()`                 | 获取所有正在加载的序列号   |
| `void GetAllLoadingAudioSerialIds(List<int> results)` | 将加载中的序列号写入外部列表 |
| `bool IsLoadingAudio(int serialId)`                   | 查询指定序列号是否仍在加载  |

### 4.1.3 播放相关

`IAudioManager` 提供了多组 `PlayAudio(...)` 重载，核心输入本质上只有几类：

- `audioAssetName`：资源名；
- `audioGroupName`：目标组；
- `priority`：资源加载优先级；
- `PlayAudioParams`：播放参数；
- `bindingTrans`：绑定实体；
- `worldPosition`：世界坐标；
- `userData`：业务自定义数据。

最核心的内部收口实现为：

```csharp
private int PlayAudio(string audioAssetName, string audioGroupName, int priority,
    PlayAudioParams playAudioParams, Transform bindingTrans, Vector3 worldPosition, object userData)
```

### 4.1.4 停止 / 暂停 / 恢复

| API                                                   | 说明             |
| ----------------------------------------------------- | -------------- |
| `bool StopAudio(int serialId)`                        | 停止指定音频         |
| `bool StopAudio(int serialId, float fadeOutSeconds)`  | 带淡出时间停止        |
| `void StopAllLoadedAudios()`                          | 停止全部已加载音频      |
| `void StopAllLoadedAudios(float fadeOutSeconds)`      | 带淡出时间停止全部已加载音频 |
| `void StopAllLoadingAudios()`                         | 标记停止全部正在加载的音频  |
| `void PauseAudio(int serialId)`                       | 暂停指定音频         |
| `void PauseAudio(int serialId, float fadeOutSeconds)` | 带淡出暂停          |
| `void ResumeAudio(int serialId)`                      | 恢复指定音频         |
| `void ResumeAudio(int serialId, float fadeInSeconds)` | 带淡入恢复          |

---

## 4.2 IAudioGroup

文件：

- `Assets/LFramework/Runtime/Component/Audio/IAudioGroup.cs`

| API                                              | 说明             |
| ------------------------------------------------ | -------------- |
| `string AudioGroupName`                          | 声音组名称          |
| `int AudioAgentCount`                            | 当前代理数量         |
| `bool AvoidBeingReplacedBySamePriority`          | 是否避免同优先级抢占     |
| `bool Mute`                                      | 组静音开关          |
| `float Volume`                                   | 组音量            |
| `void StopAllLoadedAudios()`                     | 停止组内全部已加载音频    |
| `void StopAllLoadedAudios(float fadeOutSeconds)` | 带淡出停止组内全部已加载音频 |

---

## 4.3 IAudioAgent

文件：

- `Assets/LFramework/Runtime/Component/Audio/IAudioAgent.cs`

### 关键属性

| 属性                         | 说明         |
| -------------------------- | ---------- |
| `IAudioGroup AudioGroup`   | 所属组        |
| `int SerialId`             | 播放序列号      |
| `bool IsPlaying`           | 是否正在播放     |
| `float Length`             | 音频长度       |
| `float Time`               | 当前播放位置     |
| `bool MuteInAudioGroup`    | 组内静音       |
| `bool Loop`                | 是否循环       |
| `int Priority`             | 播放优先级      |
| `float VolumeInAudioGroup` | 组内音量系数     |
| `float Pitch`              | 音调         |
| `float PanStereo`          | 声像         |
| `float SpatialBlend`       | 2D / 3D 混合 |
| `float MaxDistance`        | 最大距离       |
| `float DopplerLevel`       | 多普勒等级      |

### 关键方法

| 方法                                         | 说明        |
| ------------------------------------------ | --------- |
| `Play()` / `Play(float fadeInSeconds)`     | 播放        |
| `Stop()` / `Stop(float fadeOutSeconds)`    | 停止        |
| `Pause()` / `Pause(float fadeOutSeconds)`  | 暂停        |
| `Resume()` / `Resume(float fadeInSeconds)` | 恢复        |
| `Reset()`                                  | 重置代理并归还资源 |

---

## 4.4 AudioExtension 业务侧 API

文件：

- `Assets/GameScripts/GameLogic/Component/Audio/AudioExtension.cs`

| API                                                                                                                | 说明                 |
| ------------------------------------------------------------------------------------------------------------------ | ------------------ |
| `PlayBgm(this IAudioManager audioComponent, int musicId, object userData = null)`                                  | 播放 BGM，内部会先停止旧 BGM |
| `StopBgm(this IAudioManager audioComponent)`                                                                       | 停止当前 BGM           |
| `PlaySound(this IAudioManager audioComponent, int audioId, Transform bindingTrans = null, object userData = null)` | 播放普通音效，可绑定实体       |
| `PlayUISound(this IAudioManager audioComponent, int uiAudioId, object userData = null)`                            | 播放 UI 音效           |
| `IsMuted(this IAudioManager audioComponent, string audioGroupName)`                                                | 查询组静音              |
| `Mute(this IAudioManager audioComponent, string audioGroupName, bool mute)`                                        | 设置组静音并存档           |
| `GetVolume(this IAudioManager audioComponent, string audioGroupName)`                                              | 获取组音量              |
| `SetVolume(this IAudioManager audioComponent, string audioGroupName, float volume)`                                | 设置组音量并存档           |

---

## 5. 核心调用链

## 5.1 播放调用链

```text
业务代码 / AudioExtension
    ↓
IAudioManager.PlayAudio(...)
    ↓
AudioComponent.PlayAudio(...)
    ↓
IResourceManager.LoadAsset(...)
    ↓
AudioComponent.LoadAssetSuccessCallback(...)
    ↓
AudioGroup.PlayAudio(...)
    ↓
AudioAgent.SetAudioAsset(...)
AudioAgent.Play(...)
```

简述：

1. 业务调用 `GameEntry.Audio` 或 `AudioExtension`；
2. `AudioComponent` 校验组并分配 `serialId`；
3. 向资源系统发起异步加载；
4. 加载完成后根据目标组挑选一个 `AudioAgent`；
5. 把参数写入 `AudioAgent`，最终调用 `AudioSource.Play()`。

---

## 5.2 停止调用链

```text
IAudioManager.StopAudio(serialId)
    ↓
AudioComponent.StopAudio(serialId)
    ↓
如果还在加载：标记 _audiosToReleaseOnLoad
否则遍历所有 AudioGroup
    ↓
AudioGroup.StopAudio(serialId, fadeOutSeconds)
    ↓
AudioAgent.Stop(...)
```

---

## 5.3 组初始化调用链

游戏逻辑流程：

```text
ProcedureGameLogicLaunch.OnEnter()
    ↓
InitAudio()
    ↓
foreach (Constant.Setting.AudioGroupDict)
    ↓
GameEntry.Audio.AddAudioGroup(...)
```

启动器流程：

```text
ProcedureLaunch.OnEnter()
    ↓
InitSound()
    ↓
LFrameworkEntry.GetModule<IAudioManager>().AddAudioGroup("Default", 3)
```

---

## 6. 生命周期说明

虽然当前 Audio 模块核心类都不是设计给业务层继承的，但要理解它们的生命周期。

## 6.1 AudioComponent 生命周期

### `Awake()`

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<IAudioManager>(this);
}
```

作用：

- 向框架注册自身；
- 让外部可以通过 `LFrameworkEntry.GetModule<IAudioManager>()` 获取模块。

### `OnInit()`

作用：

- 添加或获取 `AudioListener`；
- 初始化声音组字典；
- 初始化加载中列表和待释放集合；
- 构造资源加载回调；
- 重置序列号。

说明：

- 这是 `ILFrameworkModule` 的核心初始化入口；
- 比单纯 `Awake()` 更接近模块可用状态。

### `Start()`

作用：

- 获取 `IResourceManager`。

注意：

- 如果在资源管理器尚未准备好前调用 `PlayAudio`，会抛出异常；
- 因此调用方应确保模块初始化顺序正确。

### `OnUpdate(float elapseSeconds, float realElapseSeconds)`

当前为空实现。

说明：

- 表示 Audio 模块目前没有在管理器层做逐帧驱动；
- 逐帧逻辑主要在 `AudioAgent.Update()`。

### `Shutdown()`

作用：

- 停止所有已加载音频；
- 清空组集合；
- 清空加载状态集合。

说明：

- 属于模块销毁/关闭阶段的清理入口。

---

## 6.2 AudioGroup 生命周期

### `Awake()`

作用：

- 初始化 `_audioAgents` 列表。

### `AddAudioAgentHelper(...)`

作用：

- 动态创建 `AudioAgent` 子对象；
- 为每个代理绑定组信息、混音组和资源释放器；
- 调用 `Reset()` 完成初始状态归位。

说明：

- `AudioGroup` 的真正“构建完成”不是 `Awake()`，而是 `AddAudioAgentHelper()` 执行完毕后。

---

## 6.3 AudioAgent 生命周期

### `Awake()`

作用：

- 缓存 `Transform`；
- 创建或获取 `AudioSource`；
- 设置 `playOnAwake = false`；
- 设置 `rolloffMode = AudioRolloffMode.Custom`。

### `SetAudioAsset(object audioAsset)`

作用：

- 先 `Reset()` 清理旧状态；
- 绑定新 `AudioClip`；
- 记录设置时间，用于组内替换策略判断。

### `Play(float fadeInSeconds)`

作用：

- 调用 `AudioSource.Play()`；
- 如果配置了淡入时间，则从 0 音量渐入。

### `Update()`

作用：

- 检查播放是否结束，结束后执行 `Reset()`；
- 如果绑定了 `Transform`，则每帧跟随位置。

### `OnApplicationPause(bool pause)`

作用：

- 标记应用暂停状态；
- 避免在应用暂停时误判播放结束。

### `Reset()`

作用：

- 释放当前音频资源；
- 恢复所有参数默认值；
- 清空 clip、绑定、暂停状态；
- 回到可复用状态。

说明：

- 这是 `AudioAgent` 最关键的生命周期节点；
- 任何播放结束、切换、失败回收，最终都会回到这里。

---

## 6.4 PlayAudioParams / PlayAudioInfo 生命周期

### PlayAudioParams

创建方式：

```csharp
PlayAudioParams.Create()
```

释放方式：

```csharp
ReferencePool.Release(playAudioParams)
```

使用时机：

- 调用 `PlayAudio` 前构建参数；
- 在播放请求完成或失败后由模块释放。

### PlayAudioInfo

创建方式：

- 在 `AudioComponent.PlayAudio(...)` 内部通过 `Create(...)` 创建；
- 用于在异步加载完成后保留播放上下文。

释放时机：

- 加载成功并完成播放请求后；
- 加载失败后；
- 或者请求被取消后。

---

## 7. 是否存在需要继承的基类

## 7.1 Audio 模块自身

结论：当前模块核心类不建议继承。

原因：

- `AudioComponent`、`AudioGroup`、`AudioAgent` 都是 `sealed`；
- 当前扩展方式明显偏向“接口使用 + 业务扩展方法”，而不是“继承改写”。

因此：

- 业务侧应优先通过 `IAudioManager` 使用模块；
- 常用玩法建议继续通过 `AudioExtension` 封装。

## 7.2 如果要做业务接入，需要理解的基类生命周期

虽然 Audio 模块本身不通过继承扩展，但业务接入通常发生在流程类中。  
当前接入点是：

- `ProcedureGameLogicLaunch : ProcedureBase`
- `ProcedureLaunch : ProcedureBase`

最关键生命周期：

| 生命周期         | 作用              |
| ------------ | --------------- |
| `OnEnter()`  | 创建声音组、完成模块初始化接入 |
| `OnUpdate()` | 视流程状态推进后续逻辑     |

因此如果后续新增更多 Audio 初始化逻辑，通常会放在流程类的 `OnEnter()` 中，而不是修改底层代理类继承关系。

---

## 8. 典型使用方式

## 8.1 初始化声音组

```csharp
foreach (var groupInfo in Constant.Setting.AudioGroupDict)
{
    GameEntry.Audio.AddAudioGroup(groupInfo.Key, groupInfo.Value);
}
```

## 8.2 播放 BGM

```csharp
GameEntry.Audio.PlayBgm(musicId);
```

## 8.3 播放绑定角色的 3D 音效

```csharp
GameEntry.Audio.PlaySound(audioId, actorTransform);
```

## 8.4 设置组音量

```csharp
GameEntry.Audio.SetVolume(Constant.Setting.AudioGroupSound, 0.5f);
```

---

## 9. 使用注意事项

### 9.1 必须先创建声音组

如果组不存在，`PlayAudio` 会失败并输出错误日志。

### 9.2 资源管理器必须已初始化

`AudioComponent` 内部依赖 `IResourceManager`，如果过早调用播放，会触发异常。

### 9.3 `PlayAudioParams` 建议统一通过 `Create()` 获取

这样可以与当前引用池设计保持一致，避免后续释放语义混乱。

### 9.4 当前模块更适合通过封装使用

对于业务项目，建议继续用：

- `AudioExtension`
- 常量组名
- 数据表配置

而不是在外部频繁直连底层 `AudioAgent`。

---

## 10. 总结

当前 Audio 模块不是通过继承扩展的体系，而是通过：

- `IAudioManager` 作为统一入口；
- `AudioGroup` 和 `AudioAgent` 作为内部执行层；
- `AudioExtension` 作为业务侧便捷封装；
- `ProcedureBase.OnEnter()` 作为初始化接入时机。

如果后续要改造此模块，首先应该保护好现有生命周期顺序和资源释放链路，再考虑做接口收敛或行为优化。
