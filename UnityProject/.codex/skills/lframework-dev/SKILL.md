---
name: lframework-dev
description: LFramework Unity 游戏框架开发指南，供 Codex CLI 使用。处理本项目中涉及 LFramework、GameEntry、LFrameworkEntry、ILFrameworkModule、Runtime Component 模块、GameLogic UI、DataTable、Singleton、ResourceContainer、EventContainer、ReferencePool、YooAsset 资源加载、场景加载、音频、FSM、Procedure、ObjectPool、Luban Tables、HybridCLR 配置、UniTask 工作流、模块扩展、生命周期清理、排障或代码审查的任务时使用。
---

# LFramework 开发

在本仓库处理 LFramework 专属的 Unity 工作时使用此 skill。精炼知识源位于 `references/`。

## 核心规则

1. GameLogic/业务代码优先使用 `GameEntry.Xxx`。仅在框架集成代码中，或 `GameEntry` 尚未初始化时，使用 `LFrameworkEntry.GetModule<I...>()`。
2. `LFrameworkEntry.GetModule<T>()` 和 `RegisterModule<T>()` 要求 `T` 为接口类型。不要把具体组件类作为 `T`。
3. 通过 `ReferencePool.Acquire<T>()` 创建的对象必须实现 `IReference`，在 `Clear()` 中清空所有保留字段，并通过 `ReferencePool.Release()` 释放。
4. 资源所有权必须明确。带所有者生命周期的加载优先使用 `ResourceContainer`；直接调用 `GameEntry.Resource.LoadAsset<T>()` 时，需要匹配调用 `UnloadAsset()`。
5. 事件所有权必须明确。生命周期绑定的订阅优先使用 `EventContainer`；释放它以确保 `UnsubscribeAll()` 执行。
6. UI 逻辑必须遵守 `UIFormLogic` / `UguiForm` 生命周期。在 `OnOpen()` 中绑定数据，在 `OnClose()` 或 `OnRecycle()` 中释放订阅/资源，不要绕过 `UIComponent` 去销毁池化 UI 实例。
7. `DataTableComponent` 会延迟加载 Luban `Tables`；访问 `GameEntry.DataTable` 前，DataTable `TextAsset` 必须已经加载进资源池。
8. 日志打印遵守分层边界：GameLogic 和 Launcher 统一使用 `Log` 静态类；LFramework 框架内部使用 Unity 自带的 `Debug`。
9. 当 reference 与源码不一致时，通过源码搜索核实实际 C# 签名，并以实现为准。

## 读取策略

1. 先按任务等级判断需要读取的主题。L2 只读直接相关模块；L3 读取完整相关调用链；L4 读取架构和多个相关模块。
2. 默认只读取相关 reference 的 `GameLogic 推荐用法` 和 `注意事项`。架构类任务读取 `references/architecture.md` 的相关小节。
3. 只有在代码编译失败，或任务依赖精确重载、枚举值、序列化字段、回调参数、资源路径、Unity 生命周期顺序时，才继续读取同一 reference 的 `API 速查` 和 `源码路径`，并打开对应源码确认。
4. 如果 reference 中没有覆盖目标主题，先读最接近模块的推荐用法和注意事项，再通过当前源码确认实现。

## Reference 路由

| 任务                              | 读取                                                                    |
| ------------------------------- | --------------------------------------------------------------------- |
| 项目架构、启动流程、分层边界                  | `references/architecture.md`                                         |
| GameEntry、Runtime/Core、Component 边界 | `references/architecture.md`                                  |
| 新增或修改框架模块                       | `references/architecture.md` 加上最接近的 `references/module/*.md`       |
| 基础应用设置和运行时标记                    | `references/module/base.md`                                          |
| 数据树/状态值                         | `references/module/datanode.md`                                      |
| 事件发布/订阅或生命周期订阅                  | `references/module/event.md`                                         |
| FSM 状态或 FSM 数据                  | `references/module/fsm.md`                                           |
| 本地化/I2 集成                       | `references/module/localization.md`                                  |
| 对象池                             | `references/module/objectpool.md`, `references/module/reference-pool.md` |
| Procedure/游戏流程状态                | `references/module/procedure.md`, `references/module/fsm.md`          |
| ReferencePool 或 `IReference` 对象 | `references/module/reference-pool.md`                                |
| YooAsset 资源包、资源加载、释放            | `references/module/resource.md`                                      |
| 场景加载/卸载                         | `references/module/scene.md`, `references/module/resource.md`         |
| 玩家设置持久化                         | `references/module/setting.md`                                       |
| 定时器/延迟回调                        | `references/module/timer.md`                                         |
| 音频播放、BGM/SFX 辅助接口               | `references/module/audio.md`                                         |
| 协程包装器或 AOT 保留桥接                 | `references/module/unitywrapper.md`                                  |
| Luban 表/配置访问                    | `references/module/datatable.md`, `references/module/resource.md`     |
| Singleton 管理器                   | `references/module/singleton.md`                                     |
| UI 窗口、组、组件、UI 资源                | `references/module/ui.md`, `references/module/resource.md`, `references/module/event.md` |
| HybridCLR、Launcher、资源更新流程        | `references/architecture.md`, `references/module/resource.md`         |
| Debug 浮层、运行时检查、失败分析             | 受影响模块的 `references/module/*.md`；必要时按 `源码路径` 打开源码确认             |
| 端到端运行链路                         | `references/architecture.md` 加上受影响模块 reference                    |

## 源码验证

References 有意保持精炼。编辑代码前，如遇以下情况，需要检查受影响的 `.cs` 文件：

- 任务依赖精确的重载、枚举值、序列化字段或回调参数；
- reference 与当前源码看起来存在冲突；
- 涉及 Unity 序列化、生成的 Luban 代码、YooAsset 或 HybridCLR 行为。

将改动限制在用户请求的模块内。除非用户明确要求修改生成结果，否则避免修改生成的 DataTable 代码。
