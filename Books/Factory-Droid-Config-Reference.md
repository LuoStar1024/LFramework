# Factory Droid Configuration Reference

This document summarizes the Factory Droid configuration fields that are relevant to `settings.json` and legacy `config.json`, based on the current official documentation plus the legacy keys that are present in the local `config.json` on this machine.

## Sources

- Official Settings page: `https://docs.factory.ai/cli/configuration/settings`
- Official BYOK page: `https://docs.factory.ai/cli/byok/overview`
- Official Mixed Models page: `https://docs.factory.ai/cli/configuration/mixed-models`
- Official CLI Reference page: `https://docs.factory.ai/reference/cli-reference`

## File Locations and Precedence

| File                                     | Scope                     | Purpose                                                                 |
| ---------------------------------------- | ------------------------- | ----------------------------------------------------------------------- |
| `~/.factory/settings.json`               | Primary user-level config | Main Droid configuration file documented by Factory.                    |
| `~/.factory/settings.local.json`         | User-level override       | Optional local override file that merges on top of `settings.json`.     |
| `<project>/.factory/settings.local.json` | Project-level override    | Optional project-specific override that merges at the project level.    |
| `~/.factory/config.json`                 | Legacy compatibility file | Legacy file still supported mainly for custom model/BYOK compatibility. |

## Merge Rules

- Factory officially documents `settings.json` as the main configuration file.
- `settings.local.json` overrides `settings.json` at the same level.
- Legacy `config.json` is still loaded for backwards compatibility.
- When both legacy `config.json` and `settings.json` define overlapping custom model data, `settings.json` takes priority.

## Officially Documented `settings.json` Top-Level Fields

| Key                        | Typical Type / Values                                    | Purpose                                                                           |
| -------------------------- | -------------------------------------------------------- | --------------------------------------------------------------------------------- |
| `model`                    | string                                                   | Sets the default model used by Droid for normal sessions.                         |
| `reasoningEffort`          | string                                                   | Controls how much structured reasoning the selected model uses before replying.   |
| `autonomyMode`             | `normal`, `spec`, `auto-low`, `auto-medium`, `auto-high` | Sets the default autonomy level when a new Droid session starts.                  |
| `cloudSessionSync`         | boolean                                                  | Controls whether local CLI sessions are mirrored to Factory web.                  |
| `diffMode`                 | `github`, `unified`                                      | Chooses how code diffs are rendered in the UI.                                    |
| `completionSound`          | string                                                   | Chooses the sound played when a response finishes.                                |
| `awaitingInputSound`       | string                                                   | Chooses the sound played when Droid is waiting for user input.                    |
| `soundFocusMode`           | `always`, `focused`, `unfocused`                         | Controls when notification sounds should play based on window focus.              |
| `commandAllowlist`         | string array                                             | Defines commands that Droid may run without extra confirmation.                   |
| `commandDenylist`          | string array                                             | Defines commands that should always require confirmation or be blocked.           |
| `includeCoAuthoredByDroid` | boolean                                                  | Automatically appends the Droid co-author trailer to git commits.                 |
| `enableDroidShield`        | boolean                                                  | Enables secret scanning and related git safety guardrails.                        |
| `hooksDisabled`            | boolean                                                  | Globally disables configured hooks without deleting hook definitions.             |
| `ideAutoConnect`           | boolean                                                  | Controls whether Droid automatically connects to the IDE from external terminals. |
| `todoDisplayMode`          | `inline`, `pinned`                                       | Chooses how the Todo panel is displayed in the UI.                                |
| `showThinkingInMainView`   | boolean                                                  | Controls whether reasoning/thinking blocks are shown in the main chat view.       |
| `customModels`             | array                                                    | Defines custom BYOK model entries in the modern schema.                           |

## Notes on `reasoningEffort`

- The Settings page documents general values such as `off`, `none`, `low`, `medium`, and `high`.
- The CLI reference also documents model-specific higher tiers for some models. For example, GPT-5.4 supports an Extra High tier; in current local CLI behavior this appears as `xhigh`.
- Supported values depend on the selected model.

## `customModels` Entry Schema in `settings.json`

Each item inside `customModels` is a custom model definition.

| Key               | Required | Purpose                                                                                                                                  |
| ----------------- | -------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| `model`           | Yes      | The model identifier sent to the upstream provider.                                                                                      |
| `displayName`     | No       | A human-friendly label shown in the model selector.                                                                                      |
| `baseUrl`         | Yes      | The provider API base endpoint used by Droid.                                                                                            |
| `apiKey`          | Yes      | The authentication key for the provider. In `settings.json`, this supports environment-variable expansion such as `${PROVIDER_API_KEY}`. |
| `provider`        | Yes      | Selects the API format Droid should use. Official values are `anthropic`, `openai`, or `generic-chat-completion-api`.                    |
| `maxOutputTokens` | No       | Caps the maximum number of output tokens for this custom model.                                                                          |
| `noImageSupport`  | No       | Disables image input support for this model entry.                                                                                       |
| `extraArgs`       | No       | Adds provider-specific request arguments such as temperature or top-p.                                                                   |
| `extraHeaders`    | No       | Adds custom HTTP headers to requests sent to the model provider.                                                                         |

## Legacy `config.json`

The official BYOK documentation states that legacy custom models in `~/.factory/config.json` are still supported when they use snake_case field names such as `custom_models`, `base_url`, and similar fields.

The current official docs do not publish a complete standalone legacy schema table. The table below therefore combines:

1. the official compatibility statement for legacy snake_case fields, and
2. the actual legacy keys that exist in the local `config.json` on this machine.

## Legacy `config.json` Top-Level Field

| Legacy Key      | Purpose                                                                                                            |
| --------------- | ------------------------------------------------------------------------------------------------------------------ |
| `custom_models` | Legacy array of BYOK custom model definitions. This is the legacy equivalent of `customModels` in `settings.json`. |

## Legacy `custom_models[]` Entry Fields Observed Locally

| Legacy Key           | Modern Equivalent                    | Purpose                                                                |
| -------------------- | ------------------------------------ | ---------------------------------------------------------------------- |
| `model`              | `model`                              | The provider-facing model identifier.                                  |
| `model_display_name` | `displayName`                        | Human-readable label shown to the user in the model list.              |
| `base_url`           | `baseUrl`                            | Base API endpoint for the upstream provider.                           |
| `api_key`            | `apiKey`                             | Authentication key used for requests to the upstream provider.         |
| `provider`           | `provider`                           | API compatibility mode, such as `openai` or `anthropic`.               |
| `supports_vision`    | roughly opposite of `noImageSupport` | Indicates whether the legacy custom model entry supports image inputs. |
| `max_tokens`         | roughly `maxOutputTokens`            | Sets the maximum output token limit for the legacy custom model entry. |

## Important Legacy Caveats

- Factory officially recommends `settings.json` for modern configuration.
- `config.json` is kept for backwards compatibility.
- `settings.json` has priority over overlapping legacy definitions.
- Environment-variable expansion for `apiKey` is officially documented for `settings.json` and `settings.local.json`, but not for legacy `config.json`.

## Locally Observed `settings.json` Keys That Are Not Currently Documented on the Main Settings Page

The current local `settings.json` also contains the following keys. They are observable in the local file, but they are not described on the current official Settings page.

| Key                      | Purpose                                                                                                 |
| ------------------------ | ------------------------------------------------------------------------------------------------------- |
| `logoAnimation`          | Appears to control whether the Droid startup logo animation is shown.                                   |
| `ideExtensionPromptedAt` | Appears to store timestamps of when Droid last prompted the user about IDE extension/integration setup. |

Because these two fields are not currently described on the official Settings page, they should be treated as internal or less-stable implementation details rather than primary user-facing settings.

## Mixed Models and Spec Mode

Factory officially documents that Specification Mode can use a different model from the normal default model. This is configured from `/model` inside the CLI. The current Mixed Models page explains the feature and compatibility rules, but it does not publish a clear JSON schema table for the persisted config keys on the Settings page.

In practice, treat mixed-model configuration as a supported feature, but rely on the CLI UI for editing it unless Factory later publishes a dedicated JSON schema for those persisted fields.

## Recommended Usage

- Prefer `settings.json` for all new configuration.
- Prefer `customModels` over legacy `custom_models`.
- Prefer environment variables for API keys instead of storing plaintext secrets directly in JSON.
- Keep legacy `config.json` only if you still need older compatibility behavior.

## Short Reference Summary

- `settings.json` = main Droid settings file.
- `settings.local.json` = override layer.
- `config.json` = legacy compatibility file, mainly for old BYOK custom model definitions.
- `customModels` / `custom_models` = where custom provider-backed model entries live.
- `model`, `reasoningEffort`, and `autonomyMode` = the most important day-to-day behavior controls.

---

# Factory Droid 配置参考（中文对照）

本文基于当前官方文档，以及本机本地 `config.json` 中实际存在的旧版字段，汇总了与 `settings.json` 和旧版 `config.json` 相关的 Factory Droid 配置项，方便与英文版逐项对照。

## 资料来源

- 官方 Settings 页面：`https://docs.factory.ai/cli/configuration/settings`
- 官方 BYOK 页面：`https://docs.factory.ai/cli/byok/overview`
- 官方 Mixed Models 页面：`https://docs.factory.ai/cli/configuration/mixed-models`
- 官方 CLI Reference 页面：`https://docs.factory.ai/reference/cli-reference`

## 文件位置与优先级

| 文件                                       | 作用范围     | 用途                                   |
| ---------------------------------------- | -------- | ------------------------------------ |
| `~/.factory/settings.json`               | 主要的用户级配置 | Factory 官方文档中说明的 Droid 主配置文件。        |
| `~/.factory/settings.local.json`         | 用户级覆盖配置  | 可选的本地覆盖文件，会在同级 `settings.json` 之上合并。 |
| `<project>/.factory/settings.local.json` | 项目级覆盖配置  | 可选的项目级覆盖文件，在项目层面进行合并。                |
| `~/.factory/config.json`                 | 旧版兼容文件   | 旧版配置文件，目前主要为自定义模型 / BYOK 兼容而保留。      |

## 合并规则

- Factory 官方将 `settings.json` 作为主配置文件。
- `settings.local.json` 会覆盖同级的 `settings.json`。
- 旧版 `config.json` 仍会被加载，以兼容旧配置。
- 如果旧版 `config.json` 与 `settings.json` 中存在重叠的自定义模型定义，则 `settings.json` 优先。

## 官方文档中说明的 `settings.json` 顶层字段

| 字段                         | 常见类型 / 取值                                                | 用途                                     |
| -------------------------- | -------------------------------------------------------- | -------------------------------------- |
| `model`                    | string                                                   | 设置 Droid 普通会话默认使用的模型。                  |
| `reasoningEffort`          | string                                                   | 控制所选模型在回复前进行多少结构化推理。                   |
| `autonomyMode`             | `normal`, `spec`, `auto-low`, `auto-medium`, `auto-high` | 设置新建 Droid 会话时默认的自动执行级别。               |
| `cloudSessionSync`         | boolean                                                  | 控制本地 CLI 会话是否同步到 Factory Web。          |
| `diffMode`                 | `github`, `unified`                                      | 选择代码 diff 的显示方式。                       |
| `completionSound`          | string                                                   | 设置回复完成时播放的提示音。                         |
| `awaitingInputSound`       | string                                                   | 设置 Droid 等待用户输入时播放的提示音。                |
| `soundFocusMode`           | `always`, `focused`, `unfocused`                         | 控制提示音在不同窗口焦点状态下的播放行为。                  |
| `commandAllowlist`         | string array                                             | 定义哪些命令可在无需额外确认的情况下执行。                  |
| `commandDenylist`          | string array                                             | 定义哪些命令应始终要求确认或被阻止。                     |
| `includeCoAuthoredByDroid` | boolean                                                  | 是否在 git commit 中自动附加 Droid 的协作者尾注。     |
| `enableDroidShield`        | boolean                                                  | 是否启用密钥扫描及相关 git 安全防护。                  |
| `hooksDisabled`            | boolean                                                  | 全局关闭 hooks，而无需删除 hook 配置。              |
| `ideAutoConnect`           | boolean                                                  | 控制 Droid 是否会从外部终端自动连接到 IDE。            |
| `todoDisplayMode`          | `inline`, `pinned`                                       | 控制 Todo 面板在 UI 中的显示方式。                 |
| `showThinkingInMainView`   | boolean                                                  | 控制是否在主聊天界面显示 reasoning / thinking 内容块。 |
| `customModels`             | array                                                    | 定义新版 BYOK 自定义模型列表。                     |

## 关于 `reasoningEffort` 的说明

- Settings 页面中给出了通用取值，例如 `off`、`none`、`low`、`medium`、`high`。
- CLI Reference 还说明某些模型支持更高等级。例如 GPT-5.4 支持 Extra High；在当前本机 CLI 行为中，这一值表现为 `xhigh`。
- 实际支持的取值取决于当前所选模型。

## `settings.json` 中 `customModels` 的子字段结构

`customModels` 数组中的每一项都表示一个自定义模型定义。

| 字段                | 是否必填 | 用途                                                                               |
| ----------------- | ---- | -------------------------------------------------------------------------------- |
| `model`           | 是    | 发送给上游提供商的模型标识。                                                                   |
| `displayName`     | 否    | 在模型选择器中显示的人类可读名称。                                                                |
| `baseUrl`         | 是    | Droid 请求该模型时使用的提供商 API 基础地址。                                                     |
| `apiKey`          | 是    | 访问该提供商所用的认证密钥。在 `settings.json` 中支持 `${PROVIDER_API_KEY}` 这类环境变量展开。              |
| `provider`        | 是    | 指定 Droid 应使用的 API 协议格式。官方值包括 `anthropic`、`openai`、`generic-chat-completion-api`。 |
| `maxOutputTokens` | 否    | 限制该自定义模型允许输出的最大 token 数。                                                         |
| `noImageSupport`  | 否    | 用于关闭该模型的图像输入支持。                                                                  |
| `extraArgs`       | 否    | 添加如 temperature、top-p 等提供商专用请求参数。                                                |
| `extraHeaders`    | 否    | 为发往模型提供商的请求添加自定义 HTTP 头。                                                         |

## 旧版 `config.json`

官方 BYOK 文档说明，`~/.factory/config.json` 中使用 snake_case 字段名（如 `custom_models`、`base_url` 等）的旧版自定义模型配置，当前仍然受支持。

但目前官方文档并未单独给出一份完整的旧版 schema 表。因此，下面的内容结合了：

1. 官方关于旧版 snake_case 兼容的说明；
2. 本机当前 `config.json` 中实际出现的旧版字段。

## 旧版 `config.json` 顶层字段

| 旧字段             | 用途                                                      |
| --------------- | ------------------------------------------------------- |
| `custom_models` | 旧版 BYOK 自定义模型数组，对应新版 `settings.json` 中的 `customModels`。 |

## 本机实际观察到的 `custom_models[]` 子字段

| 旧字段                  | 新版对应字段                      | 用途                                  |
| -------------------- | --------------------------- | ----------------------------------- |
| `model`              | `model`                     | 传给提供商的模型标识。                         |
| `model_display_name` | `displayName`               | 在模型列表中展示给用户的可读名称。                   |
| `base_url`           | `baseUrl`                   | 上游提供商的 API 基础地址。                    |
| `api_key`            | `apiKey`                    | 请求上游提供商所使用的认证密钥。                    |
| `provider`           | `provider`                  | API 兼容模式，例如 `openai` 或 `anthropic`。 |
| `supports_vision`    | 大致对应 `noImageSupport` 的反向语义 | 表示该旧版模型项是否支持图像输入。                   |
| `max_tokens`         | 大致对应 `maxOutputTokens`      | 设置该旧版模型项允许的最大输出 token 数。            |

## 关于旧版配置的重要说明

- Factory 官方推荐使用 `settings.json` 作为现代配置方式。
- `config.json` 仅作为向后兼容保留。
- 当旧版与新版定义冲突时，`settings.json` 优先。
- 官方仅说明 `settings.json` 和 `settings.local.json` 支持 `apiKey` 的环境变量展开；旧版 `config.json` 没有这一说明。

## 当前本机 `settings.json` 中存在、但官方 Settings 页面未明确说明的字段

当前本机的 `settings.json` 中还出现了以下字段，但它们没有在当前官方 Settings 页面中被正式说明。

| 字段                       | 用途                                         |
| ------------------------ | ------------------------------------------ |
| `logoAnimation`          | 看起来用于控制 Droid 启动时的 Logo 动画是否显示。            |
| `ideExtensionPromptedAt` | 看起来用于记录 Droid 上次提示用户安装或配置 IDE 扩展 / 集成的时间戳。 |

由于这两个字段没有出现在当前官方 Settings 页面中，因此更适合将其视为内部字段或稳定性较低的实现细节，而不是主要的用户配置项。

## Mixed Models 与 Spec Mode

Factory 官方文档说明，Specification Mode 可以使用与普通默认模型不同的模型。这一功能通常通过 CLI 中的 `/model` 进行配置。当前 Mixed Models 页面解释了该特性及其兼容性规则，但并未在 Settings 页面中给出明确的 JSON 配置字段表。

因此，实践上可以把 mixed-model 配置视为受支持功能，但如果 Factory 未来没有发布更明确的持久化字段 schema，建议优先通过 CLI 界面进行配置，而不是手工编辑 JSON。

## 推荐用法

- 新配置优先使用 `settings.json`。
- 自定义模型优先使用 `customModels`，而不是旧版 `custom_models`。
- API key 优先通过环境变量注入，不要直接以明文写入 JSON。
- 如果不再需要旧版兼容行为，可逐步减少对 `config.json` 的依赖。

## 简短总结

- `settings.json` = Droid 主配置文件。
- `settings.local.json` = 覆盖层。
- `config.json` = 旧版兼容文件，主要用于老的 BYOK 自定义模型定义。
- `customModels` / `custom_models` = 放置自定义提供商模型的地方。
- `model`、`reasoningEffort`、`autonomyMode` = 日常最重要的三个行为控制项。
