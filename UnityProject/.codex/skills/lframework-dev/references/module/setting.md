# Setting

## GameLogic 推荐用法

- GameLogic/业务代码中优先使用 `GameEntry.Setting` 读写玩家本地设置，不要直接访问 `PlayerPrefs`。
- `GameEntry.Setting.GetBool(settingName, defaultValue)`、`GetInt(settingName, defaultValue)`、`GetFloat(settingName, defaultValue)`、`GetString(settingName, defaultValue)`：读取基础类型设置；设置可能不存在时优先使用带默认值的重载。
- `GameEntry.Setting.SetBool(settingName, value)`、`SetInt(settingName, value)`、`SetFloat(settingName, value)`、`SetString(settingName, value)`：写入基础类型设置；关键用户设置写入后调用 `GameEntry.Setting.Save()`。
- `GameEntry.Setting.HasSetting(settingName)`：判断设置项是否存在，适合处理首启默认值、版本迁移或 Launcher 阶段的资源版本记录。
- `GameEntry.Setting.RemoveSetting(settingName)`：移除单个设置项；仅在明确需要清理旧 key 或重置某项设置时使用。
- 对象设置可使用 `SetObject(...)` 和 `GetObject(...)`，底层通过 `Utility.Json` 序列化到字符串；使用前确认类型可被项目 JSON 工具稳定序列化。
- Launcher、Debugger 等框架集成代码，或 `GameEntry` 尚未初始化的阶段，可通过 `LFrameworkEntry.GetModule<ISettingManager>()` 获取接口。

## 注意事项

- `Load()` 当前直接返回 `true`；`Save()` 调用 `PlayerPrefs.Save()` 并返回 `true`；`Shutdown()` 会调用 `Save()`。
- `SetBool` 使用 `PlayerPrefs.SetInt(settingName, value ? 1 : 0)` 存储布尔值。
- 所有读写和移除接口都会校验 `settingName`，空字符串或 `null` 会抛出 `LFrameworkException`。
- 设置 key 必须保持稳定。重命名 key 会丢失旧用户偏好，除非额外编写迁移逻辑。
- 重要用户操作后应立即 `Save()`，例如音量、静音、语言、资源版本等；不要只依赖模块关闭时的 `Shutdown()` 保存。
- `Count`、`GetAllSettingNames()` 和 `GetAllSettingNames(List<string>)` 当前不支持枚举 `PlayerPrefs`：`Count` 返回 `0`，数组重载返回 `null`，列表重载会清空传入列表。
- 不要把大型对象、资源数据或频繁变化的运行时状态写入 Setting；它适合保存小型、本地、持久化的用户设置。
- `RemoveAllSettings()` 会调用 `PlayerPrefs.DeleteAll()` 清空全部 PlayerPrefs 数据，影响范围大，业务代码中谨慎使用。

## ISettingManager API 速查

仅在框架集成代码或 `GameEntry.Setting` facade 内部优先考虑直接使用 `ISettingManager`。

- 生命周期：`Load()`、`Save()`。
- 查询：`HasSetting(settingName)`。
- 移除：`RemoveSetting(settingName)`、`RemoveAllSettings()`。
- 布尔值：`GetBool(settingName)`、`GetBool(settingName, defaultValue)`、`SetBool(settingName, value)`。
- 整数值：`GetInt(settingName)`、`GetInt(settingName, defaultValue)`、`SetInt(settingName, value)`。
- 浮点值：`GetFloat(settingName)`、`GetFloat(settingName, defaultValue)`、`SetFloat(settingName, value)`。
- 字符串：`GetString(settingName)`、`GetString(settingName, defaultValue)`、`SetString(settingName, value)`。
- 对象：`GetObject<T>(settingName)`、`GetObject<T>(settingName, defaultObj)`、`GetObject(objectType, settingName)`、`GetObject(objectType, settingName, defaultObj)`、`SetObject<T>(settingName, obj)`、`SetObject(settingName, obj)`。
- 当前不推荐依赖：`Count`、`GetAllSettingNames()`、`GetAllSettingNames(List<string>)`，因为 PlayerPrefs 实现不支持枚举。

## 源码路径

- `Assets/LFramework/Runtime/Component/Setting/ISettingManager.cs`
- `Assets/LFramework/Runtime/Component/Setting/SettingComponent.cs`
