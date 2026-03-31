# Setting 模块核心 API 与生命周期

## 1. 文档目的

本文用于说明当前 `Setting` 模块的：

- 核心类型；
- 对外 API；
- 关键调用链；
- 是否存在需要继承的基类；
- 模块核心生命周期。

---

## 2. 模块定位

`Setting` 模块是 LFramework 的游戏配置管理模块，用于：

- 读写基础配置项；
- 读写对象配置项；
- 删除配置项；
- 在模块关闭时保存配置。

当前底层实现完全依赖：

```csharp
UnityEngine.PlayerPrefs
```

因此它在项目中的定位，是“框架级轻量持久化配置模块”。

---

## 3. 核心类型

## 3.1 SettingComponent

文件：

- `Assets/LFramework/Runtime/Component/Setting/SettingComponent.cs`

定义：

```csharp
public sealed class SettingComponent : MonoBehaviour, ILFrameworkModule, ISettingManager
```

职责：

- 作为 Setting 模块的 Unity 组件入口；
- 在 `Awake()` 中注册到 `LFrameworkEntry`；
- 实现全部 `ISettingManager` 接口；
- 把配置读写转发到 `PlayerPrefs`；
- 在 `Shutdown()` 时执行保存。

说明：

- `sealed`，当前模块不以继承 `SettingComponent` 扩展为主；
- 业务层通常通过 `GameEntry.Setting` 访问。

---

## 3.2 ISettingManager

文件：

- `Assets/LFramework/Runtime/Component/Setting/ISettingManager.cs`

职责：

- 定义游戏配置系统的统一对外接口；
- 覆盖：
  - 加载 / 保存；
  - 查询 / 删除；
  - 基础类型读写；
  - 对象读写；
  - 配置项计数与枚举。

---

## 3.3 SettingComponentInspector

文件：

- `Assets/LFramework/Editor/Inspector/SettingComponentInspector.cs`

职责：

- 在运行时展示：
  - Setting 数量；
  - 各配置项内容；
- 提供：
  - `Save Settings`
  - `Remove All Settings`

说明：

- 这是调试支持，不属于运行时核心逻辑；
- 但它体现了 `Count / GetAllSettingNames` 这些 API 的预期用途。

---

## 4. 核心 API

## 4.1 基础信息与管理 API

```csharp
int Count { get; }
bool Load()
bool Save()
string[] GetAllSettingNames()
void GetAllSettingNames(List<string> results)
bool HasSetting(string settingName)
bool RemoveSetting(string settingName)
void RemoveAllSettings()
```

### API 含义

| API | 说明 |
| --- | --- |
| `Count` | 配置项数量 |
| `Load()` | 加载配置 |
| `Save()` | 保存配置 |
| `GetAllSettingNames()` | 获取全部配置项名 |
| `HasSetting()` | 判断配置是否存在 |
| `RemoveSetting()` | 删除指定配置 |
| `RemoveAllSettings()` | 删除全部配置 |

---

## 4.2 Bool 配置 API

```csharp
bool GetBool(string settingName)
bool GetBool(string settingName, bool defaultValue)
void SetBool(string settingName, bool value)
```

说明：

- 当前实现通过 `PlayerPrefs.GetInt / SetInt` 间接存布尔值；
- `true` 对应 `1`，`false` 对应 `0`。

---

## 4.3 Int 配置 API

```csharp
int GetInt(string settingName)
int GetInt(string settingName, int defaultValue)
void SetInt(string settingName, int value)
```

---

## 4.4 Float 配置 API

```csharp
float GetFloat(string settingName)
float GetFloat(string settingName, float defaultValue)
void SetFloat(string settingName, float value)
```

---

## 4.5 String 配置 API

```csharp
string GetString(string settingName)
string GetString(string settingName, string defaultValue)
void SetString(string settingName, string value)
```

---

## 4.6 Object 配置 API

### 读取对象

```csharp
T GetObject<T>(string settingName)
object GetObject(Type objectType, string settingName)
T GetObject<T>(string settingName, T defaultObj)
object GetObject(Type objectType, string settingName, object defaultObj)
```

### 写入对象

```csharp
void SetObject<T>(string settingName, T obj)
void SetObject(string settingName, object obj)
```

说明：

- 当前对象读写基于：

```csharp
Utility.Json.ToJson(obj)
Utility.Json.ToObject(...)
```

- 也就是说对象最终是以字符串 JSON 形式存入 `PlayerPrefs`。

---

## 5. 关键调用链

## 5.1 模块注册调用链

```text
SettingComponent.Awake()
    ↓
LFrameworkEntry.RegisterModule<ISettingManager>(this)
    ↓
SettingComponent.OnInit()
```

---

## 5.2 基础配置写入调用链

以 `SetInt(...)` 为例：

```text
GameEntry.Setting.SetInt(...)
    ↓
SettingComponent.SetInt(...)
    ↓
PlayerPrefs.SetInt(...)
```

其它 `Bool / Float / String` 也是同样模式。

---

## 5.3 对象配置写入调用链

```text
GameEntry.Setting.SetObject(...)
    ↓
SettingComponent.SetObject(...)
    ↓
Utility.Json.ToJson(obj)
    ↓
PlayerPrefs.SetString(...)
```

---

## 5.4 对象配置读取调用链

```text
GameEntry.Setting.GetObject<T>(...)
    ↓
SettingComponent.GetObject<T>(...)
    ↓
PlayerPrefs.GetString(...)
    ↓
Utility.Json.ToObject<T>(json)
```

---

## 5.5 模块关闭调用链

```text
框架关闭模块
    ↓
SettingComponent.Shutdown()
    ↓
Save()
    ↓
PlayerPrefs.Save()
```

---

## 6. 模块生命周期

## 6.1 SettingComponent 生命周期

### `Awake()`

作用：

- 把当前组件注册为 `ISettingManager` 模块。

```csharp
private void Awake()
{
    LFrameworkEntry.RegisterModule<ISettingManager>(this);
}
```

### `OnInit()`

作用：

- 当前实现为空；
- 由于底层是 `PlayerPrefs`，没有额外初始化容器。

### `OnUpdate(float elapseSeconds, float realElapseSeconds)`

作用：

- 当前实现为空；
- Setting 模块不依赖逐帧轮询。

### `Shutdown()`

作用：

- 在模块关闭时执行 `Save()`。

```csharp
public void Shutdown()
{
    Save();
}
```

### `Load()`

作用：

- 当前实现直接返回 `true`；
- 在 PlayerPrefs 后端下没有显式加载动作。

### `Save()`

作用：

- 调用 `PlayerPrefs.Save()` 把当前配置落盘。

---

## 7. 是否存在需要继承的基类

结论：当前 Setting 模块没有需要业务继承的基类。

原因：

- `SettingComponent` 是 `sealed`；
- `ISettingManager` 是标准模块接口；
- 业务层通过接口直接读写配置即可。

因此：

- 不建议通过继承 `SettingComponent` 扩展配置逻辑；
- 若后续要扩展存储后端，更合理的方向是：
  - 新增另一种管理器实现；
  - 或在当前组件内部替换存储策略。

---

## 8. 典型使用方式

## 8.1 读写布尔值

```csharp
GameEntry.Setting.SetBool("Setting.BgmMuted", true);
bool muted = GameEntry.Setting.GetBool("Setting.BgmMuted", false);
```

---

## 8.2 读写整数

```csharp
GameEntry.Setting.SetInt("Setting.Level", 3);
int level = GameEntry.Setting.GetInt("Setting.Level", 1);
```

---

## 8.3 读写浮点

```csharp
GameEntry.Setting.SetFloat("Setting.BgmVolume", 0.8f);
float volume = GameEntry.Setting.GetFloat("Setting.BgmVolume", 1f);
```

---

## 8.4 读写字符串

```csharp
GameEntry.Setting.SetString("Setting.Language", "English");
string language = GameEntry.Setting.GetString("Setting.Language", "English");
```

---

## 8.5 读写对象

```csharp
GameEntry.Setting.SetObject("Setting.SomeData", someObject);
var data = GameEntry.Setting.GetObject<MyData>("Setting.SomeData");
```

---

## 8.6 删除与保存

```csharp
GameEntry.Setting.RemoveSetting("Setting.SomeData");
GameEntry.Setting.Save();
```

---

## 9. 使用注意事项

### 9.1 当前底层是 PlayerPrefs

这意味着它更适合保存：

- 小型配置；
- 基础类型；
- 轻量 JSON 对象；

不适合保存大体积复杂数据。

---

### 9.2 `SetXxx(...)` 不等于立刻落盘

写入后只是进入 `PlayerPrefs` 缓冲，真正持久化依赖：

- 显式调用 `Save()`；
- 或应用退出时 Unity 的行为。

因此关键数据建议主动 `Save()`。

---

### 9.3 对象配置最终是字符串

对象配置底层走 JSON，因此：

- 类型结构变化可能影响反序列化；
- 坏数据会影响读取稳定性。

---

### 9.4 接口里存在“枚举全部配置项”的能力

按接口设计，`Count / GetAllSettingNames` 是对外能力的一部分。  
因此在理解模块时，需要特别关注这组能力是否真的被后端支持。

---

## 10. 总结

当前 `Setting` 模块可以概括为：

- 一个 `sealed` 的配置管理组件 `SettingComponent`；
- 一个统一接口 `ISettingManager`；
- 一套围绕 `PlayerPrefs` 的基础类型与对象配置读写 API；
- 一个在关闭时自动保存的轻量配置模块。

如果后续你要继续阅读源码或开始修复，最重要的是先把以下三点吃透：

1. `ISettingManager` 对外承诺了哪些能力；
2. `SettingComponent` 当前哪些能力是真实现，哪些只是占位；
3. 业务层关键配置（语言、资源版本、音频）目前是如何依赖这个模块的。
