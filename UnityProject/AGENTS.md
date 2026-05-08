# AGENTS.md

请使用中文写提案和回答。

这个文件为当前 LFramework Unity 项目提供指导，用于处理此项目中的代码。当前项目基于 LFramework + HybridCLR + YooAsset + UniTask + Luban 构建。

---

## 强制工作流（所有任务必须遵守）

所有任务先判断等级，再按等级获取项目规范，再实现或输出方案，最后在验证。

### 第零步：判断任务等级

| 等级 | 判断标准 | 知识查询策略 |
|------|----------|--------------|
| L1 简单 | typo 修正、注释修改、日志文本调整、局部变量改名，且不涉及框架 API、生命周期、资源路径、UI 节点、事件定义或生成代码 | 可跳过查询，直接处理 |
| L2 调用 | 调用已知 API、单一模块局部修改、单个 UI 或资源调用点调整 | 使用 `lframework-dev` skill，只查直接相关主题的推荐用法和注意事项 |
| L3 功能 | 新功能开发、跨文件修改、新增 UI/资源/事件/流程/配置表逻辑 | 使用 `lframework-dev` skill，查询完整相关调用链的推荐用法和注意事项 |
| L4 架构 | 新模块设计、系统重构、多模块协作、启动流程或架构决策 | 使用 `lframework-dev` skill，并行查询架构和多个相关模块 |

判断原则：宁可高估等级，不可低估。不确定时上调一级。

### 第一步：按等级获取规范

L1 任务可直接进入第二步。L2-L4 必须使用 `lframework-dev` skill。

知识源：`.codex/skills/lframework-dev/references/`。这是当前项目给 Codex 使用的精炼文档，唯一权威来源。

阅读粒度：

- 一般情况下，只需要阅读相关模块 reference 的 `GameLogic 推荐用法` 和 `注意事项`。
- 架构、启动链路、分层边界相关任务读取 `architecture.md` 的相关小节。
- 只有当代码编译失败，或任务依赖精确的重载、枚举值、序列化字段、回调参数、资源路径、Unity 生命周期顺序时，才继续阅读同一 reference 的 `API 速查` 和 `源码路径`，并打开源码确认。
- 如果 reference 未覆盖目标主题，先读取最接近模块的推荐用法和注意事项，再以当前源码为准确认实现。

会话内缓存：

- 同一会话中已查询过的主题无需重复读取。
- 直接引用本次会话已获取的规范摘要。
- 仅当任务涉及未覆盖的新主题时，再读取新的 reference。

常见主题路由：

| 场景 | 必读 reference |
|------|----------------|
| 项目架构、启动流程、分层边界 | `architecture.md` |
| 新增或修改框架模块 | `architecture.md` 加最接近的 `module/*.md` |
| GameEntry、Runtime/Core、Component 边界 | `architecture.md` |
| YooAsset 资源加载、卸载、资源包 | `module/resource.md` |
| UI 窗口、UIForm、UguiForm、UIWidget | `module/ui.md`, `module/resource.md`, `module/event.md` |
| 事件发布订阅、生命周期订阅 | `module/event.md` |
| ReferencePool 或 IReference 对象 | `module/reference-pool.md` |
| ObjectPool 对象池 | `module/objectpool.md`, `module/reference-pool.md` |
| Procedure 流程 | `module/procedure.md`, `module/fsm.md` |
| FSM 状态机 | `module/fsm.md` |
| Luban 配置表 | `module/datatable.md`, `module/resource.md` |
| 场景加载卸载 | `module/scene.md`, `module/resource.md` |
| 音频播放 | `module/audio.md` |
| 定时器 | `module/timer.md` |
| 本地化 | `module/localization.md` |
| Singleton 管理器 | `module/singleton.md` |
| DataNode 状态树 | `module/datanode.md` |
| Base/Setting 配置 | `module/base.md`, `module/setting.md` |
| UnityWrapper 协程包装 | `module/unitywrapper.md` |
| HybridCLR、Launcher、资源更新流程 | `architecture.md`, `module/resource.md` |
| Debugger 或排障 | 受影响模块的 `module/*.md`；必要时按 `源码路径` 打开源码确认 |
| 完整运行链路 | `architecture.md` 加受影响模块 reference |

### 第二步：验证实际代码并实现

基于 `lframework-dev` skill 返回的规范编写实现。

当 reference 与代码实际 API 冲突时：

1. 以当前源码为准。
2. 在回答中标注冲突点和采用的实际 API。
3. 记录到 `.codex/memory/problem_YYYY-MM-DD.md`，如果目录不存在则先创建。

### 第三步：按任务等级验证

所有实现类任务必须按等级执行对应的最低验证。无法在当前环境完成的验证项，必须在最终回答中明确说明原因、已完成的替代验证，以及需要用户在 Unity Editor 中确认的内容。

| 等级 | 最低验证方式 |
|------|--------------|
| L1 简单 | 复查目标文件或搜索结果，确认 typo、注释、日志文本或局部变量修改范围正确，且未触碰无关代码。 |
| L2 调用 | 搜索并阅读受影响调用点，确认 API 签名、参数、生命周期和释放逻辑匹配当前源码；如可行，运行相关编译、静态检查或最小范围测试。 |
| L3 功能 | 除 L2 验证外，检查跨文件调用链、资源持有与释放、事件订阅与取消订阅、UI 生命周期或配置表加载前置条件；如可行，运行相关程序集编译、EditMode/PlayMode 测试或功能入口验证。 |
| L4 架构 | 除 L3 验证外，对照启动链路、模块依赖、热更边界、资源包/配置表流程和 OpenSpec 任务验收项进行检查；如可行，运行完整编译、关键流程测试或构建相关校验。 |

验证汇报必须区分“已执行并通过”的项目和“未能执行”的项目，禁止把静态检查等同于 Unity Editor 运行验证。

---

## 项目结构

主要代码路径：

| 路径 | 说明 |
|------|------|
| `Assets/LFramework/Runtime` | LFramework 运行时核心、组件模块、接口和基础设施 |
| `Assets/LFramework/Editor` | LFramework 编辑器工具、Inspector、HybridCLR/YooAsset 配置工具 |
| `Assets/GameScripts/GameLogic` | 热更业务逻辑、GameEntry、UI、流程、事件、游戏示例逻辑 |
| `Assets/GameScripts/GameDataTable` | Luban 生成代码和配置表访问层 |
| `Assets/Launcher/Scripts` | 启动器、资源初始化、下载、加载热更程序集 |
| `Assets/HybridCLRGenerate` | HybridCLR 生成物 |
| `openspec` | OpenSpec 需求、变更和任务 |

程序集：

- `LFramework.Runtime`
- `LFramework.Editor`
- `Launcher`
- `GameLogic`
- `GameDataTable`

不要手工改 Unity 自动生成的 `.csproj`，也不要直接修改 Luban 生成代码，除非用户明确要求处理生成结果。

---

## 核心编码红线

1. 业务代码优先通过 `GameEntry.Xxx` 访问模块，例如 `GameEntry.Resource`、`GameEntry.UI`、`GameEntry.Event`、`GameEntry.DataTable`。
2. 框架集成代码或 `GameEntry` 尚未初始化的阶段，才直接使用 `LFrameworkEntry.GetModule<I...>()`。
3. `LFrameworkEntry.GetModule<T>()` 和 `RegisterModule<T>()` 的 `T` 必须是接口类型，禁止用具体 Component 类作为泛型参数。
4. 新模块必须实现 `ILFrameworkModule`，并明确 `Priority`、`OnInit()`、`OnUpdate()`、`Shutdown()` 的职责。
5. `OnInit()` 只做自身状态初始化；跨模块依赖尽量放到 `Start()` 或更晚阶段，避免 Unity `Awake()` 顺序问题。
6. 资源所有权必须明确。GameLogic 中优先使用 `ResourceContainer`；直接 `GameEntry.Resource.LoadAsset<T>()` 时必须匹配 `UnloadAsset()`。
7. `ResourceContainer` 通过 `ReferencePool.Acquire` 创建，结束时必须 `ReferencePool.Release(container)`，让 `Clear()` 释放资源和取消异步加载。
8. 事件订阅必须有生命周期所有者。优先使用 `EventContainer`，释放时确保 `UnsubscribeAll()` 执行。
9. 通过 `ReferencePool.Acquire<T>()` 创建的对象必须实现 `IReference`，`Clear()` 必须清空全部保留字段，并通过 `ReferencePool.Release()` 回收。
10. UI 逻辑遵守 `UIFormLogic` / `UguiForm` 生命周期：在 `OnOpen()` 绑定数据和订阅，在 `OnClose()` 或 `OnRecycle()` 释放事件与资源。
11. 不要绕过 `UIComponent` 直接 `Destroy` 已纳入 UI 管理的窗口实例。
12. `GameEntry.DataTable` 访问 Luban `Tables` 前，配置表 `TextAsset` 必须已经加载进资源池。
13. Runtime 框架层不要依赖 GameLogic；GameLogic 可以依赖 Runtime 暴露的接口和 `GameEntry` facade。
14. 日志打印遵守分层边界：GameLogic 和 Launcher 统一使用 `Log` 静态类；LFramework 框架内部使用 Unity 自带的 `Debug`。
15. 涉及 HybridCLR、Launcher、资源更新流程时，必须先查启动链路和实际代码，避免破坏热更边界。

---

## 启动与运行链路

核心启动顺序：

```text
RootComponent.Awake()
  -> Runtime/GameLogic Component Awake()
  -> LFrameworkEntry.RegisterModule<I...>(this)
  -> module.OnInit()
RootComponent.Update()
  -> LFrameworkEntry.OnUpdate(deltaTime, unscaledDeltaTime)
GameEntry.Start()
  -> 重置 Procedure FSM
  -> 缓存内置模块和自定义模块
  -> EventHelper.OnInit()
  -> StartProcedure<ProcedureGameLogicLaunch>()
```

`GameEntry` 缓存的内置模块包括 `Audio`、`Base`、`Config`、`DataNode`、`Debugger`、`Event`、`Fsm`、`Localization`、`ObjectPool`、`Procedure`、`ReferencePool`、`Resource`、`Scene`、`Setting`、`Timer`、`UnityWrapper`。

`GameEntry` 缓存的自定义模块包括 `DataTable`、`UI`、`Singleton`。

---

## OpenSpec 约定

如果用户要求提案、设计变更、实现 OpenSpec 任务或归档变更，使用对应技能：

- 提案或新 change：`openspec-propose`
- 探索和澄清需求：`openspec-explore`
- 实现 change tasks：`openspec-apply-change`
- 完成后归档：`openspec-archive-change`

普通代码修复不需要强制走 OpenSpec，除非用户明确要求或改动达到 L4 架构等级。

---

## 自我优化记录

触发条件：

1. reference 文档描述与实际源码 API 不符。
2. 生成代码编译或运行失败，根因是 reference 描述有误。
3. 用户明确指出某个 reference 描述错误。

记录位置：`.codex/memory/problem_YYYY-MM-DD.md`。

记录字段：

- 问题现象：错误表现或报错信息。
- 文档位置：哪篇 reference 文档哪一节。
- 正确 API：经代码验证后的正确用法。
- 建议修正：文档应该改成什么表诉。
