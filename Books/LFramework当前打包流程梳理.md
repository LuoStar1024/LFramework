# LFramework 当前打包流程梳理

## 概述

本文基于当前项目代码实际实现整理，重点对应以下文件：

- `Assets/Editor/Misc/ReleaseTools.cs`
- `Assets/Editor/Misc/CommandLineReader.cs`
- `Assets/LFramework/Editor/HybridCLR/BuildDLLCommand.cs`
- `Assets/LFramework/Runtime/Component/Config/UpdateConfig.cs`
- `Assets/Launcher/Scripts/Procedure/*.cs`

目标是说明：

1. 当前启动器如何消费打包结果
2. 当前编辑器菜单里的实际打包顺序
3. 按现有代码可执行的一套打包流程
4. 现阶段需要注意的实现细节

---

## 一、当前运行时资源流程

启动器当前流程如下：

```text
ProcedureLaunch
  -> ProcedureSplash
  -> ProcedureInitPackage
  -> ProcedureInitResources
      -> ProcedureCreateDownloader
      -> ProcedureDownloadFile
      -> ProcedureDownloadOver
  -> ProcedurePreload
  -> ProcedureLoadAssembly
  -> ProcedureStartGame
```

### 1. `ProcedureInitPackage`

- 初始化 YooAsset 默认包 `DefaultPackage`
- 根据资源模式进入后续流程：
  - `EditorSimulate`：编辑器模拟
  - `Package`：单机包模式
  - `Updatable` / `WebPlayMode`：可更新模式

### 2. `ProcedureInitResources`

- 请求远端版本号：`RequestPackageVersionAsync()`
- 更新远端 Manifest：`UpdatePackageManifestAsync(packageVersion)`
- 如果是可更新模式，后续决定是否进入下载器流程
- 如果是单机模式，则直接进入预加载

### 3. `ProcedureCreateDownloader` / `ProcedureDownloadFile`

- 根据最新 Manifest 创建下载器
- 有差异文件则下载补丁
- 无差异则直接进入 `ProcedureDownloadOver`

### 4. `ProcedureDownloadOver`

- 将最新 `PackageVersion` 写入本地 `GAME_VERSION`
- 然后进入 `ProcedurePreload`

### 5. 结论

当前项目的打包结果最终会被启动器这样使用：

- **首包内资源**：从 `StreamingAssets` 读取
- **热更新资源**：从 `UpdateConfig` 配置的远端地址读取
- **热更 DLL / AOT 元数据**：通过 `BuildDLLCommand` 复制到 `GameResRaw/Dll`

---

## 二、当前打包入口

## 1. AssetBundle 打包入口

文件：`Assets/Editor/Misc/ReleaseTools.cs`

菜单：

- `LFramework/Build/一键打包AssetBundle`

实际执行逻辑：

1. `BuildDLLCommand.BuildAndCopyDlls()`
2. `BuildInternal(...)`
3. `AssetDatabase.Refresh()`
4. `CopyStreamingAssetsFiles()`

其中 `BuildInternal(...)` 负责调用 YooAsset 构建资源包。

## 2. 客户端打包入口

菜单：

- `LFramework/Build/一键打包Window`
- `LFramework/Build/一键打包Android`
- `LFramework/Build/一键打包IOS`

实际执行逻辑：

1. `BuildInternal(...)`
2. `BuildImp(...)`

也就是：

- 先构建资源包
- 再构建 Player

## 3. HybridCLR 相关入口

文件：`Assets/LFramework/Editor/HybridCLR/BuildDLLCommand.cs`

菜单：

- `HybridCLR/Define Symbols/Enable HybridCLR`
- `HybridCLR/Define Symbols/Disable HybridCLR`
- `HybridCLR/Build/BuildAssets And CopyTo AssemblyTextAssetPath`

作用：

- 编译热更 DLL
- 拷贝热更 DLL 到 `GameResRaw/Dll`
- 拷贝 AOT 元数据 DLL 到 `GameResRaw/Dll`

---

## 三、当前配置点

文件：`Assets/Launcher/Res/Configs/UpdateConfig.asset`

当前关键配置如下：

```yaml
projectName: Demo
ResDownLoadPath: http://127.0.0.1:8081
FallbackResDownLoadPath: http://127.0.0.1:8082
isAutoAssetCopeToBuildAddress: 0
BuildAddress: ../../Builds/Unity_Data/StreamingAssets
AssemblyTextAssetPath: GameResRaw/Dll
```

说明：

- 远端资源默认发布地址按 `项目名/平台名` 组织
- 当前远端地址还是本地测试地址 `127.0.0.1`
- `isAutoAssetCopeToBuildAddress = 0`，表示当前**不会自动把 StreamingAssets 再复制到最终包目录**

---

## 四、按当前代码可执行的打包流程

下面这套流程是结合现有实现整理出来的“可落地流程”。

## 1. 前置检查

打包前先确认：

1. Unity 当前切换到了目标平台
2. `UpdateConfig.asset` 中远端地址、项目名配置正确
3. 如果使用 HybridCLR，先确认已经启用 `ENABLE_HYBRIDCLR`
4. YooAsset 的 `DefaultPackage` 收集规则已配置完成

## 2. 首次打某个平台时，先生成一次 Player

原因：

- `BuildDLLCommand.CopyAOTAssembliesToAssetPath()` 需要裁剪后的 AOT DLL
- 这些 DLL 通常要在 `BuildPlayer` 后才会生成

因此如果是该平台第一次正式打包，建议先执行一次：

- `LFramework/Build/一键打包Window`
- 或 `LFramework/Build/一键打包Android`
- 或 `LFramework/Build/一键打包IOS`

这样可以先把该平台需要的 AOT 元数据产物生成出来。

## 3. 生成热更 DLL 与资源包

推荐顺序：

1. 执行 `HybridCLR/Build/BuildAssets And CopyTo AssemblyTextAssetPath`
2. 执行 `LFramework/Build/一键打包AssetBundle`

这一步完成后，核心结果包括：

- 热更 DLL `.bytes`
- AOT 元数据 DLL `.bytes`
- `DefaultPackage` 对应的 YooAsset 资源包
- `StreamingAssets` 内的内置资源清单

## 4. 发布远端资源

将 YooAsset 构建出的包内容发布到资源服务器。

当前启动器在运行时会从：

```text
{ResDownLoadPath}/{projectName}/{platform}
```

读取版本与 Manifest。

按当前配置示例，大致会落到：

```text
http://127.0.0.1:8081/Demo/Windows64
http://127.0.0.1:8081/Demo/Android
http://127.0.0.1:8081/Demo/IOS
```

实际发布时，应以本次构建日志里输出的 `OutputPackageDirectory` 为准。

## 5. 再执行一次客户端打包

为了确保最新 `StreamingAssets`、DLL 文本资源、内置清单都进入首包，推荐最后再执行一次目标平台客户端打包：

- `LFramework/Build/一键打包Window`
- `LFramework/Build/一键打包Android`
- `LFramework/Build/一键打包IOS`

这样最终客户端会包含当前最新首包资源。

---

## 五、推荐发布顺序

如果要做一套相对稳妥的正式发布，建议按下面顺序：

```text
1. 切换目标平台
2. 检查 UpdateConfig / 资源服务器配置
3. 首次平台构建一次 Player（生成 AOT 裁剪 DLL）
4. 执行 HybridCLR/Build/BuildAssets And CopyTo AssemblyTextAssetPath
5. 执行 LFramework/Build/一键打包AssetBundle
6. 上传资源包到远端资源服务器
7. 再执行一次目标平台客户端打包
8. 用启动器验证：初始化 -> 拉版本 -> 更新 Manifest -> 下载差量 -> 进入游戏
```

---

## 六、当前实现里的注意点

## 1. `BuildInternal` 没有真正使用传入的 `outputRoot`

在 `ReleaseTools.BuildInternal(...)` 里，实际写死使用的是：

```csharp
buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
```

这意味着：

- 外部传入的 `outputRoot` 当前主要没有生效
- 最终资源输出目录要以 YooAsset 默认目录和控制台日志为准

## 2. `BuildAssetBundle()` 读取了 `packageVersion`，但没有传入 `BuildInternal`

也就是说命令行入口虽然取到了：

```csharp
string packageVersion = CommandLineReader.GetCustomArgument("packageVersion");
```

但实际调用是：

```csharp
BuildInternal(target, outputRoot);
```

当前命令行打包入口的版本号参数并没有真正用进去。

## 3. `CommandLineReader.cs` 里的示例已经过时

示例里写的是：

```text
TEngine.ReleaseTools.BuildPackage
```

但当前项目实际类名与命名空间是：

```text
GameEditor.ReleaseTools
```

如果后续要走命令行打包，需要按当前项目真实入口重新整理命令。

## 4. 平台一键打包里，`BuildDLLCommand.BuildAndCopyDlls(target)` 目前被注释了

说明：

- `一键打包Window/Android/IOS` 并不会自动刷新 HybridCLR DLL
- 所以更稳妥的做法仍然是先手动执行一次 `HybridCLR/Build/BuildAssets And CopyTo AssemblyTextAssetPath`

## 5. `CopyStreamingAssetsFiles()` 只有在特定条件下才有意义

当前 `UpdateConfig.asset` 中：

```yaml
isAutoAssetCopeToBuildAddress: 0
```

因此默认不会自动复制到 `BuildAddress` 指定目录。

---

## 七、结论

结合当前代码，项目的实际打包思路可以总结为：

1. **先准备 HybridCLR DLL**
2. **再构建 YooAsset 包与内置清单**
3. **上传远端资源**
4. **最后构建客户端首包**

如果后续要把这套流程完全自动化，建议下一步优先整理：

- 命令行入口
- `outputRoot` 与 `packageVersion` 参数传递
- 客户端打包与 DLL 刷新的统一顺序
