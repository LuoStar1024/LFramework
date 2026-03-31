# LFramework - Unity 游戏开发框架

## 项目概述

LFramework 是一个功能完整、模块化的 Unity 游戏开发框架，专为商业级游戏项目设计。该框架整合了现代化的资源管理、异步操作和热更新技术，提供了完善的开发工具和最佳实践。

### 核心特性

- **模块化架构** - 基于 ECS 设计思想的组件系统，各模块独立且可插拔
- **热更新支持** - 集成 HybridCLR，支持全平台原生 C# 热更新
- **资源管理** - 使用 YooAsset 实现高效的资源加载和更新
- **异步操作** - 基于 UniTask 的高性能异步任务系统
- **状态机管理** - 灵活的 FSM（有限状态机）和 Procedure（流程）系统
- **事件驱动** - 高性能的事件池系统，支持事件的订阅、发布和管理
- **对象池** - 智能的对象池和引用池，减少 GC 压力
- **UI 框架** - 基于 UGUI 的 UI 管理系统
- **多语言支持** - 集成 I2 Localization 插件
- **调试工具** - 内置调试器，便于开发阶段问题排查

---

## 技术栈

### 核心依赖

| 依赖库                 | 版本         | 用途                            |
| ------------------- | ---------- | ----------------------------- |
| **YooAsset**        | Latest     | 资源管理系统，支持资源热更新、分包下载、内存管理      |
| **UniTask**         | Latest     | 高性能异步/多线程库，替代 Unity Coroutine |
| **HybridCLR**       | Latest     | 全平台原生 C# 热更新解决方案              |
| **I2 Localization** | Integrated | 多语言本地化支持                      |

### 技术亮点

- **约 329 个框架源文件** - 完善的架构设计
- **68+ 游戏逻辑示例** - 展示框架使用方法
- **26+ 接口定义** - 清晰的模块抽象
- **支持 Unity 2019.4+** - 兼容多个 LTS 版本
- **支持所有 il2cpp 平台** - iOS、Android、WebGL、Consoles 等

---

## 项目结构

```
LFramework/
├── UnityProject/                    # Unity 项目根目录
│   ├── Assets/
│   │   ├── LFramework/             # 框架核心代码
│   │   │   ├── Runtime/           # 运行时组件 (329+ 文件)
│   │   │   │   ├── Component/     # 各功能模块
│   │   │   │   │   ├── Audio/     # 音频系统
│   │   │   │   │   ├── Config/    # 配置管理
│   │   │   │   │   ├── DataNode/  # 数据节点树
│   │   │   │   │   ├── Debugger/  # 调试器
│   │   │   │   │   ├── Event/     # 事件系统
│   │   │   │   │   ├── Fsm/       # 有限状态机
│   │   │   │   │   ├── Localization/ # 多语言
│   │   │   │   │   ├── ObjectPool/ # 对象池
│   │   │   │   │   ├── Procedure/ # 流程管理
│   │   │   │   │   ├── ReferencePool/ # 引用池
│   │   │   │   │   ├── Resource/  # 资源管理（基于 YooAsset）
│   │   │   │   │   ├── Scene/     # 场景管理
│   │   │   │   │   ├── Setting/   # 设置系统
│   │   │   │   │   └── Timer/     # 定时器
│   │   │   │   └── Core/          # 核心基础设施
│   │   │   │       ├── DataStruct/ # 数据结构
│   │   │   │       ├── EventPool/  # 事件池实现
│   │   │   │       └── Extension/  # 扩展方法
│   │   │   └── Editor/            # 编辑器工具
│   │   │       ├── HybridCLR/     # 热更新构建脚本
│   │   │       ├── I2Localization/ # 本地化编辑器
│   │   │       └── ConfigsProvider/ # 配置工具
│   │   ├── GameScripts/          # 游戏逻辑代码
│   │   │   ├── GameLogic/        # 游戏主逻辑 (68+ 文件)
│   │   │   │   ├── Base/         # 基础类和入口
│   │   │   │   ├── Component/    # 自定义组件
│   │   │   │   ├── Definition/   # 常量和定义
│   │   │   │   ├── Event/        # 游戏事件
│   │   │   │   ├── Game/         # 游戏管理器
│   │   │   │   ├── Procedure/    # 游戏流程
│   │   │   │   ├── UI/           # UI 界面
│   │   │   │   └── Utility/      # 工具类
│   │   │   └── GameDataTable/    # 数据表
│   │   ├── Launcher/             # 启动器（AOT 部分）
│   │   ├── Test/                 # 测试代码
│   │   ├── Resources/            # 资源文件
│   │   ├── GameResRaw/           # 原始资源
│   │   └── GameResArt/           # 美术资源
│   └── Packages/                 # Unity Package
│       ├── YooAsset/             # 资源管理插件
│       ├── UniTask/              # 异步任务库
│       └── HybridCLR/            # 热更新插件
├── Tools/                        # 外部工具
│   └── Luban/                    # 配置表工具
└── Books/                        # 文档和教程
    └── README.md                 # 本文件
```

---

## 核心模块详解

### 1. 模块系统 (Module System)

框架采用模块化设计，所有功能通过接口形式暴露，通过 `LFrameworkEntry` 统一管理：

```csharp
// 获取模块
var eventManager = LFrameworkEntry.GetModule<IEventManager>();
var resourceManager = LFrameworkEntry.GetModule<IResourceManager>();

// 模块按优先级自动排序，每帧调用 Update
```

**主要模块接口：**

- `IEventManager` - 事件管理
- `IResourceManager` - 资源管理（基于 YooAsset）
- `IObjectPoolManager` - 对象池管理
- `IProcedureManager` - 流程管理
- `IFsmManager` - 状态机管理
- `ILocalizationManager` - 多语言管理
- `IAudioManager` - 音频管理
- `IDataNodeManager` - 数据节点管理
- `ISettingManager` - 设置管理
- `IDebuggerManager` - 调试器管理

### 2. 流程系统 (Procedure System)

流程系统用于管理游戏的全局状态切换，例如：

- **ProcedureGameLogicLaunch** - 游戏逻辑启动（热更新入口）
- **ProcedureLogin** - 登录流程
- **ProcedureMenu** - 主菜单流程
- **ProcedureGame** - 游戏主流程
- **ProcedureChangeScene** - 场景切换流程

```csharp
public class ProcedureLogin : ProcedureBase
{
    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        // 进入流程时打开登录 UI
        GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset("LoginForm"),
                                Constant.Setting.UIGroupNormal, this);
    }

    protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner,
                                    float elapseSeconds, float realElapseSeconds)
    {
        // 登录成功后切换到主菜单
        if (_loginSuccess)
        {
            ChangeState<ProcedureMenu>(procedureOwner);
        }
    }
}
```

### 3. 事件系统 (Event System)

基于事件池的高性能事件系统，支持事件 ID 和事件订阅：

```csharp
// 定义事件 ID
public static class GameEvent
{
    public static readonly int PlayerDeadId =
        EventRuntimeId.ToRuntimeId("GameLogic.Event.PlayerDead");
}

// 订阅事件
GameEntry.Event.Subscribe(GameEvent.PlayerDeadId, OnPlayerDead);

// 发布事件
GameEntry.Event.Fire(this, GameEvent.PlayerDeadId, userData);

// 取消订阅
GameEntry.Event.Unsubscribe(GameEvent.PlayerDeadId, OnPlayerDead);
```

### 4. 资源管理 (Resource Management)

基于 **YooAsset** 的资源管理系统，支持：

- 异步资源加载
- 资源包更新
- 资源版本管理
- 内存自动管理

```csharp
// 加载预制体
var prefab = await GameEntry.Resource.LoadAsset<GameObject>("Assets/Prefabs/Enemy.prefab");

// 加载并实例化
var enemy = await GameEntry.Resource.InstantiateAsync("Assets/Prefabs/Enemy.prefab");

// 卸载未使用资源
GameEntry.Resource.ForceUnloadUnusedAssets(true);
```

### 5. 对象池系统 (Object Pool)

智能对象池，减少 GC 压力：

```csharp
// 创建对象池
GameEntry.ObjectPool.CreateMultiSpawnObjectPool("Bullet", 100);

// 获取对象
var bullet = GameEntry.ObjectPool.Spawn("Bullet");

// 归还对象
GameEntry.ObjectPool.Despawn(bullet);

// 引用池使用
var data = ReferencePool.Acquire<PlayerData>();
ReferencePool.Release(data);
```

### 6. UI 系统 (UI System)

基于 UGUI 的 UI 管理系统：

```csharp
// 打开 UI
GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset("LoginForm"),
                        Constant.Setting.UIGroupNormal, userData);

// 关闭 UI
GameEntry.UI.CloseUIForm(uiForm);

// UI 组管理
GameEntry.UI.AddUIGroup(uiGroupName, uiGroupHelper);
```

所有 UI 窗口继承自 `UguiForm`：

```csharp
public class LoginForm : UguiForm
{
    protected internal override void OnOpen(object userData)
    {
        // UI 打开时调用
    }

    protected internal override void OnClose(bool isShutdown, object userData)
    {
        // UI 关闭时调用
    }

    protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        // 每帧更新
    }
}
```

### 7. 异步操作 (Async/Await with UniTask)

使用 **UniTask** 实现高效的异步操作：

```csharp
// 异步加载资源
private async UniTaskVoid LoadEnemyAsync()
{
    var assetHandle = GameEntry.Resource.LoadAssetAsync<GameObject>("Enemy");
    await assetHandle;
    var enemy = Instantiate(assetHandle.AssetObject as GameObject);
}

// 异步等待
await UniTask.Delay(1000); // 等待 1 秒
await UniTask.Yield();      // 等待一帧

// 异常处理
try
{
    await SomeAsyncOperation();
}
catch (Exception e)
{
    Log.Error($"Error: {e.Message}");
}
```

### 8. 状态机系统 (FSM System)

灵活的有限状态机系统：

```csharp
// 创建状态机
var fsm = GameEntry.Fsm.CreateFsm<Enemy>("Enemy",
    new IdleState(),
    new PatrolState(),
    new ChaseState(),
    new AttackState()
);

// 启动状态机
fsm.Start<IdleState>();

// 切换状态
fsm.ChangeState<ChaseState>();
```

### 9. 数据节点系统 (Data Node)

树形数据结构，用于管理游戏数据：

```csharp
// 获取根节点
var root = GameEntry.GetDataNode();

// 设置数据
root.SetData("Player.Name", "Hero");
root.SetData("Player.Level", 10);

// 获取数据
var name = root.GetData<string>("Player.Name");
var level = root.GetData<int>("Player.Level");
```

---

## 热更新实现 (HybridCLR)

LFramework 使用 **HybridCLR** 实现全平台原生 C# 热更新：

### HybridCLR 核心优势

- **零学习成本** - 与普通 C# 代码无异
- **完整特性支持** - 泛型、反射、多线程、async/await
- **高性能** - 寄存器解释器，性能接近原生
- **低内存** - 与 AOT 代码内存占用相同
- **全平台支持** - iOS、Android、WebGL、Consoles 等

### 热更新工作流程

1. **AOT 部分（Launcher）** - 预编译代码，负责启动和加载热更新 DLL
2. **热更新部分（GameScripts）** - 可动态更新的游戏逻辑
3. **构建流程** - 通过 HybridCLR 编辑器工具生成热更新 DLL

### 构建命令

```csharp
// 在 Unity Editor 中执行
Assets/LFramework/Editor/HybridCLR/BuildDLLCommand.cs
```

---

## 快速开始

### 1. 环境要求

- Unity 2019.4.x / 2020.3.x / 2021.3.x / 2022.3.x 或更高版本
- Windows / macOS 操作系统
- (可选) Android/iOS 构建环境

### 2. 项目设置

1. 克隆或下载本项目
2. 使用 Unity 打开 `UnityProject` 目录
3. 等待 Unity 导入完成
4. 配置 HybridCLR（参考官方文档）
5. 运行场景测试

### 3. 编写第一个流程

```csharp
using LFramework;

namespace GameLogic
{
    public class ProcedureMyGame : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Log.Info("Enter My Game Procedure");

            // 初始化游戏
            // 加载场景
            // 打开 UI
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner,
                                        float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 游戏逻辑更新
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            // 清理资源
        }
    }
}
```

### 4. 注册流程

在 `GameEntry` 的 `Start` 方法中注册新流程：

```csharp
ProcedureBase[] procedures =
{
    new ProcedureChangeScene(),
    new ProcedureGame(),
    new ProcedureMenu(),
    new ProcedureLogin(),
    new ProcedureMyGame(), // 添加你的流程
    new ProcedureGameLogicLaunch(),
};
```

---

## 最佳实践

### 命名规范

- **类名**: PascalCase（如 `GameManager`、`ProcedureLogin`）
- **方法名**: PascalCase（如 `OnEnter`、`HandleEvent`）
- **字段**: `_camelCase`（如 `_enemyInterval`、`_loginSuccess`）
- **常量**: PascalCase（如 `ConfigAsset`、`DataTableAsset`）
- **接口**: I 前缀（如 `IReference`、`IProcedureManager`）

### 代码组织

- 框架代码放在 `Assets/LFramework/`
- 游戏逻辑放在 `Assets/GameScripts/GameLogic/`
- 常量定义在 `Assets/GameScripts/GameLogic/Definition/Constant/`
- 流程定义在 `Assets/GameScripts/GameLogic/Procedure/`
- UI 窗口定义在 `Assets/GameScripts/GameLogic/UI/`

### 异常处理

```csharp
// 使用框架异常
throw new LFrameworkException("Error message");

// 格式化错误消息
throw new LFrameworkException(Utility.Text.Format("Failed to load {0}", assetPath));
```

### 资源释放

```csharp
// 释放对象池对象
ReferencePool.Release(obj);

// 卸载未使用资源
GameEntry.Resource.ForceUnloadUnusedAssets(true);

// 清理对象池
GameEntry.ObjectPool.ReleaseAllUnused();
```

---

## 调试工具

框架内置调试器，可在运行时查看：

- 各模块状态
- 对象池使用情况
- 资源加载情况
- 性能监控
- 日志输出

启用调试器：

```csharp
GameEntry.Debugger.RegisterDebuggerWindow("MyDebugger", new MyDebuggerWindow());
```

---

## 常见问题 (FAQ)

### Q: 如何切换流程？

```csharp
procedureOwner.ChangeState<ProcedureMenu>(procedureOwner);
```

### Q: 如何异步加载场景？

```csharp
await GameEntry.Scene.LoadSceneAsync(sceneName, this);
```

### Q: 如何处理事件？

```csharp
// 1. 定义事件 ID
public static readonly int MyEventId = EventRuntimeId.ToRuntimeId("Namespace.ClassName.EventName");

// 2. 订阅
GameEntry.Event.Subscribe(MyEventId, OnEventHandler);

// 3. 触发
GameEntry.Event.Fire(this, MyEventId, userData);

// 4. 取消订阅
GameEntry.Event.Unsubscribe(MyEventId, OnEventHandler);
```

### Q: 如何构建热更新 DLL？

在 Unity Editor 中执行菜单：
`Assets > LFramework > HybridCLR > Build All`

或通过命令行：

```bash
Unity.exe -batchmode -quit -projectPath [ProjectPath] -executeMethod BuildDLLCommand.Execute
```

---

## 参考资料

- **YooAsset 文档**: https://github.com/tuyoogame/YooAsset
- **UniTask 文档**: https://github.com/Cysharp/UniTask
- **HybridCLR 文档**: https://hybridclr.doc.code-philosophy.com/docs/intro
- **I2 Localization**: https://inter-illusion.com/assets/I2Localization

---

## 版本历史

- **v1.0** - 初始版本，核心框架实现
  - 模块化架构
  - YooAsset 资源管理
  - UniTask 异步支持
  - HybridCLR 热更新集成
  - 基础组件系统

---

## 许可证

本项目基于 MIT 许可证开源。

---

## 联系方式

如有问题或建议，欢迎提交 Issue 或 Pull Request。

---

**LFramework** - 让 Unity 游戏开发更简单、更高效！
