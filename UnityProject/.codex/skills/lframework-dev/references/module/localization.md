# 本地化

## GameLogic 推荐用法

- GameLogic/业务代码中优先使用 `GameEntry.Localization` 访问本地化模块，不要在业务流程中直接获取 `LocalizationComponent`。
- `ProcedureGameLogicLaunch` 当前会在进入登录流程前调用 `GameEntry.Localization.LoadLanguageTotalAsset(LocalizationUtility.LocalizationAssetPath)`，加载完整语言总表。
- 语言总表加载完成后，再调用 `GameEntry.Localization.SetLanguage(GameEntry.Localization.Language)` 切换当前语言，并触发 I2 的界面刷新链路。
- 需要读取系统语言时，使用 `GameEntry.Localization.SystemLanguage`，它会把 Unity `Application.systemLanguage` 映射为框架 `Language` 枚举。
- 需要判断语言是否可用时，使用 `GameEntry.Localization.CheckLanguage(language)`，它基于已加载语言总表中收集到的语言名称判断。
- UI 文本优先在目标节点上挂 I2 `Localize` 组件：在 `Term` / `mTerm` 填写 CSV 中的词条 key，运行时 I2 会按当前语言取翻译并写回目标文本组件。
- 对 Unity UI `Text`，`LocalizeTarget_UnityUI_Text` 会把主词条翻译结果写入 `Text.text`；如果启用 TextMeshPro，`LocalizeTarget_TextMeshPro_UGUI` 会处理 `TextMeshProUGUI.text`。
- 代码中需要动态设置 UI 文本词条时，获取同节点 `Localize` 后调用 `localize.SetTerm(termKey)` 或设置 `localize.Term = termKey`，不要直接把翻译后的字符串写入 `Text.text`。
- 只有在确实需要脚本直接取得翻译字符串时，才考虑使用 `LFramework.Localization.LocalizationManager.GetTranslation(term, ...)`，并确认当前语言总表已经加载完成。

## 注意事项

- `LocalizationComponent.Awake()` 通过 `LFrameworkEntry.RegisterModule<ILocalizationManager>(this)` 注册模块；业务侧应等待 `GameEntry.InitBuiltinComponents()` 后再使用 `GameEntry.Localization`。
- `LocalizationComponent.Start()` 会缓存 `IResourceManager`，`LoadLanguageTotalAsset()` 内部也会在缺失时重新获取资源模块。
- `LoadLanguageTotalAsset(assetName)` 使用 `IResourceManager.LoadAsset<TextAsset>(assetName, 10)` 加载 CSV，并通过 I2 `LanguageSourceData.Import_CSV` 合并到本地化源。
- 编辑器模拟资源模式下，`LoadLanguageTotalAsset()` 会注册编辑器全局语言源、更新语言列表并初始化 `SourceData`，不会走运行时 CSV 加载。
- `Language` 属性不能设置为 `Language.Unspecified`；首次赋值只记录内部状态，后续赋值会转入 `SetLanguage(value)`。
- `SetLanguage(language, load = false)` 当前不会使用 `load` 参数加载分表，只会校验语言是否已存在、设置 I2 `LocalizationManager.CurrentLanguage`，并更新当前语言。
- `CheckLanguage(language)` 使用 `language.ToString()` 与 I2 语言名称匹配；新增语言枚举、CSV 表头、I2 语言名称必须保持一致。
- 本地化 CSV 资源路径集中在 `LocalizationUtility.LocalizationAssetPath`，当前值为 `Assets/GameResRaw/Localization/Localization.csv`。
- `ILocalizationManager` 当前不提供字符串取值接口；直接取词条属于 I2 集成细节，使用前先确认调用点是否真的需要绕过 UI `Localize` 组件。
- `Localize` 的 `Term` 是词条 key，不是最终显示文本；例如 CSV 中存在 `Login/StartButton` 词条时，组件 `Term` 填 `Login/StartButton`，不要填中文或英文显示值。
- `Localize.SetTerm(primary)` 内部只在 `primary` 非空时更新主词条，并会立即调用 `OnLocalize(true)` 刷新当前节点。

## ILocalizationManager API 速查

仅在框架集成代码或 `GameEntry.Localization` 已初始化后的 GameLogic 代码中使用 `ILocalizationManager`。

- 当前语言：`Language { get; set; }`，获取或设置当前框架语言；设置为 `Language.Unspecified` 会抛出异常。
- 系统语言：`SystemLanguage`，把 Unity 系统语言转换为框架 `Language` 枚举，无法识别时返回 `Language.Unspecified`。
- 语言总表加载：`LoadLanguageTotalAsset(string assetName)`，异步加载本地化 CSV 总表并更新 I2 语言源。
- 语言可用性：`CheckLanguage(Language language)`，判断指定语言是否存在于已加载语言列表。
- 语言切换：`SetLanguage(Language language, bool load = false)`，切换当前语言并触发 I2 本地化刷新；返回是否切换成功。

## 源码路径

- `Assets/LFramework/Runtime/Component/Localization/LocalizationUtility.cs`
- `Assets/LFramework/Runtime/Component/Localization/LocalizationComponent.cs`
- `Assets/LFramework/Runtime/Component/Localization/ILocalizationManager.cs`
- `Assets/LFramework/Runtime/Component/Localization/Language.cs`
- `Assets/LFramework/Runtime/Component/Localization/I2Localization/**`
