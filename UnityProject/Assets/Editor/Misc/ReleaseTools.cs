using System;
using System.IO;
using System.Linq;
using LFramework;
using LFramework.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;
using BuildResult = UnityEditor.Build.Reporting.BuildResult;

namespace GameEditor
{
    /// <summary>
    /// 打包工具类。
    /// <remarks>通过CommandLineReader可以不前台开启Unity实现静默打包以及CLI工作流，详见CommandLineReader.cs example1</remarks>
    /// </summary>
    public static class ReleaseTools
    {
        private const string LastOneClickBuildPlatformKey = "LFramework.ReleaseTools.LastOneClickBuildPlatform";

        public static void BuildDll()
        {
            bool success = false;
            try
            {
                string platform = CommandLineReader.GetCustomArgument("platform");
                if (string.IsNullOrEmpty(platform))
                {
                    Debug.LogError($"Build DLL Error！platform is null");
                    CompleteCommandLineBuild(false);
                    return;
                }

                BuildTarget target = GetBuildTarget(platform);
                if (!TrySwitchBuildTarget(target))
                {
                    CompleteCommandLineBuild(false);
                    return;
                }

                BuildDLLCommand.BuildAndCopyDlls(target);
                success = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            CompleteCommandLineBuild(success);
        }

        public static void BuildAssetBundle()
        {
            bool success = false;
            try
            {
                string outputRoot = CommandLineReader.GetCustomArgument("outputRoot");
                if (string.IsNullOrEmpty(outputRoot))
                {
                    Debug.LogError($"Build Asset Bundle Error！outputRoot is null");
                    CompleteCommandLineBuild(false);
                    return;
                }

                string packageVersion = CommandLineReader.GetCustomArgument("packageVersion", GetBuildPackageVersion());
                if (string.IsNullOrEmpty(packageVersion))
                {
                    Debug.LogError($"Build Asset Bundle Error！packageVersion is null");
                    CompleteCommandLineBuild(false);
                    return;
                }

                string platform = CommandLineReader.GetCustomArgument("platform");
                if (string.IsNullOrEmpty(platform))
                {
                    Debug.LogError($"Build Asset Bundle Error！platform is null");
                    CompleteCommandLineBuild(false);
                    return;
                }

                BuildTarget target = GetBuildTarget(platform);
                success = BuildAssetBundlesForTarget(target, outputRoot, packageVersion);
                if (success)
                {
                    Debug.LogWarning($"Start BuildPackage BuildTarget:{target} outputPath:{outputRoot}");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            CompleteCommandLineBuild(success);
        }

        public static void BuildPackage()
        {
            bool success = false;
            try
            {
                string platform = CommandLineReader.GetCustomArgument("platform");
                if (string.IsNullOrEmpty(platform))
                {
                    Debug.LogError($"Build Package Error！platform is null");
                    CompleteCommandLineBuild(false);
                    return;
                }

                BuildTarget target = GetBuildTarget(platform);
                string packageVersion = CommandLineReader.GetCustomArgument("packageVersion", GetBuildPackageVersion());
                string outputRoot = CommandLineReader.GetCustomArgument("outputRoot", GetAssetBundleOutputRoot(target));
                string playerOutput = CommandLineReader.GetCustomArgument("playerOutput",
                    GetDefaultPlayerOutputPath(target, packageVersion));
                bool buildPlayer = GetBoolArgument("buildPlayer", true);

                success = buildPlayer
                    ? BuildFullPackage(GetBuildTargetGroup(target), target, outputRoot, playerOutput, packageVersion)
                    : BuildAssetBundlesForTarget(target, outputRoot, packageVersion);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            CompleteCommandLineBuild(success);
        }

        [MenuItem("LFramework/Build/一键打包AssetBundle _F8", false, 75)]
        public static void BuildCurrentPlatformAB()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildAssetBundlesForTarget(target, GetAssetBundleOutputRoot(target), GetBuildPackageVersion(), true);
        }

        /// <summary>
        /// 复制StreamingAssets文件去打包目录
        /// </summary>
        public static void CopyStreamingAssetsFiles()
        {
            if (!ConfigComponent.EditorUpdateConfig.IsAutoAssetCopeToBuildAddress())
            {
                Debug.Log("UpdateSetting.IsAutoAssetCopeToBuildAddress关闭,并不会生产到打包目录中");
                return;
            }

            // 获取StreamingAssets路径
            string streamingAssetsPath = Application.streamingAssetsPath;

            // 目标路径，可以是任何你想要的目录
            string targetPath = ConfigComponent.EditorUpdateConfig.GetBuildAddress();

            // 判断目标路径是相对路径还是绝对路径
            if (!System.IO.Path.IsPathRooted(targetPath))
            {
                // 如果是相对路径，结合 StreamingAssets 的路径进行合并
                targetPath = System.IO.Path.Combine(streamingAssetsPath, targetPath);
            }

            // 如果目标目录不存在，创建它
            if (!System.IO.Directory.Exists(targetPath))
            {
                Debug.LogError("打包目录不存在,检查UpdateSetting BuildAddress:" + targetPath);
                return;
            }

            // 删除目标路径下的所有文件
            string[] Deletefiles = System.IO.Directory.GetFiles(targetPath);
            foreach (var file in Deletefiles)
            {
                System.IO.File.Delete(file);
                Debug.Log($"删除文件: {file}");
            }

            // 删除目标路径下的所有子目录
            string[] directories = System.IO.Directory.GetDirectories(targetPath);
            foreach (var directory in directories)
            {
                System.IO.Directory.Delete(directory, true); // true 表示递归删除子目录及其中内容
                Debug.Log($"删除目录: {directory}");
            }

            // 获取StreamingAssets中的所有文件，排除.meta文件
            string[] files =
                System.IO.Directory.GetFiles(streamingAssetsPath, "*", System.IO.SearchOption.AllDirectories);

            // 遍历并复制文件到目标目录
            foreach (var file in files)
            {
                // 排除.meta文件
                if (file.EndsWith(".meta"))
                    continue;

                // 获取相对路径，用于在目标目录中创建相同的文件结构
                string relativePath = file.Substring(streamingAssetsPath.Length + 1);
                string destinationFilePath = System.IO.Path.Combine(targetPath, relativePath);

                // 确保目标文件夹存在
                string destinationDir = System.IO.Path.GetDirectoryName(destinationFilePath);
                if (!System.IO.Directory.Exists(destinationDir))
                {
                    System.IO.Directory.CreateDirectory(destinationDir);
                }

                // 复制文件
                System.IO.File.Copy(file, destinationFilePath, true); // true 表示覆盖已存在的文件
            }

            Debug.Log($"复制文件完成：{targetPath}");
        }

        private static BuildTarget GetBuildTarget(string platform)
        {
            BuildTarget target = BuildTarget.NoTarget;
            switch (platform.Trim().ToLowerInvariant())
            {
                case "android":
                    target = BuildTarget.Android;
                    break;
                case "ios":
                    target = BuildTarget.iOS;
                    break;
                case "windows":
                    target = BuildTarget.StandaloneWindows64;
                    break;
                case "macos":
                    target = BuildTarget.StandaloneOSX;
                    break;
                case "linux":
                    target = BuildTarget.StandaloneLinux64;
                    break;
                case "webgl":
                    target = BuildTarget.WebGL;
                    break;
                case "switch":
                    target = BuildTarget.Switch;
                    break;
                case "ps4":
                    target = BuildTarget.PS4;
                    break;
                case "ps5":
                    target = BuildTarget.PS5;
                    break;
            }

            return target;
        }

        private static bool TrySwitchBuildTarget(BuildTarget buildTarget)
        {
            BuildTargetGroup buildTargetGroup = GetBuildTargetGroup(buildTarget);
            if (buildTarget == BuildTarget.NoTarget || buildTargetGroup == BuildTargetGroup.Unknown)
            {
                Debug.LogError($"Unsupported build target: {buildTarget}");
                return false;
            }

            if (EditorUserBuildSettings.activeBuildTarget == buildTarget)
            {
                return true;
            }

            Debug.Log($"Switch build target: {EditorUserBuildSettings.activeBuildTarget} -> {buildTarget}");
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget))
            {
                Debug.LogError($"Switch build target failed: {buildTargetGroup}/{buildTarget}");
                return false;
            }

            AssetDatabase.Refresh();
            return true;
        }

        private static BuildTargetGroup GetBuildTargetGroup(BuildTarget buildTarget)
        {
            return BuildPipeline.GetBuildTargetGroup(buildTarget);
        }

        private static string GetAssetBundleOutputRoot(BuildTarget buildTarget)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BuildBundles", buildTarget.ToString()));
        }

        private static string GetPlayerOutputPath(string platformName, string fileName)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BuildApp", platformName, fileName));
        }

        private static string GetDefaultPlayerOutputPath(BuildTarget buildTarget, string packageVersion)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return GetPlayerOutputPath("Windows", "Release_Windows.exe");
                case BuildTarget.Android:
                    return GetPlayerOutputPath("Android", $"{packageVersion}_Android.apk");
                case BuildTarget.iOS:
                    return GetPlayerOutputPath("IOS", "XCode_Project");
                default:
                    return GetPlayerOutputPath(buildTarget.ToString(), buildTarget.ToString());
            }
        }

        private static bool GetBoolArgument(string argumentName, bool defaultValue)
        {
            string value = CommandLineReader.GetCustomArgument(argumentName, defaultValue.ToString());
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }

        private static void CompleteCommandLineBuild(bool success)
        {
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(success ? 0 : 1);
            }
        }

        private static bool BuildAssetBundlesForTarget(BuildTarget buildTarget, string outputRoot,
            string packageVersion,
            bool copyStreamingAssetsToConfiguredBuildAddress = false)
        {
            if (!TrySwitchBuildTarget(buildTarget))
            {
                return false;
            }

            BuildDLLCommand.BuildAndCopyDlls(buildTarget);
            AssetDatabase.Refresh();

            bool success = BuildInternal(buildTarget, outputRoot, packageVersion);
            AssetDatabase.Refresh();

            if (success && copyStreamingAssetsToConfiguredBuildAddress)
            {
                CopyStreamingAssetsFiles();
            }

            return success;
        }

        private static bool BuildFullPackage(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget,
            string assetBundleOutputRoot, string playerOutputPath, string packageVersion = null)
        {
            if (string.IsNullOrEmpty(packageVersion))
            {
                packageVersion = GetBuildPackageVersion();
            }

            if (!PrepareHybridCLRGenerateForOneClickBuild(buildTarget))
            {
                return false;
            }

            if (!BuildAssetBundlesForTarget(buildTarget, assetBundleOutputRoot, packageVersion))
            {
                Debug.LogError($"Build player skipped because AssetBundle build failed: {buildTarget}");
                return false;
            }

            return BuildImp(buildTargetGroup, buildTarget, playerOutputPath);
        }

        private static bool PrepareHybridCLRGenerateForOneClickBuild(BuildTarget buildTarget)
        {
            string currentPlatform = GetOneClickBuildPlatformKey(buildTarget);
            if (string.IsNullOrEmpty(currentPlatform))
            {
                return true;
            }

            if (!TrySwitchBuildTarget(buildTarget))
            {
                return false;
            }

            string lastPlatform = LoadLastOneClickBuildPlatform();
            if (string.Equals(lastPlatform, currentPlatform, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"[ReleaseTools] Last one-click build platform is {lastPlatform}. Skip HybridCLR Generate.All.");
                return true;
            }

            Debug.LogWarning(
                $"[ReleaseTools] One-click build platform changed: '{lastPlatform}' -> '{currentPlatform}'. Run HybridCLR Generate.All.");

            try
            {
#if ENABLE_HYBRIDCLR && ENABLE_OBFUZ
                Obfuz4HybridCLR.PrebuildCommandExt.GenerateAll();
                AssetDatabase.Refresh();
#elif ENABLE_HYBRIDCLR
                HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
                AssetDatabase.Refresh();
#else
                Debug.LogWarning("[ReleaseTools] ENABLE_HYBRIDCLR is not defined. Skip HybridCLR Generate.All.");
#endif
                SaveLastOneClickBuildPlatform(buildTarget);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        private static string GetOneClickBuildPlatformKey(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.iOS:
                    return "IOS";
                default:
                    return string.Empty;
            }
        }

        private static string LoadLastOneClickBuildPlatform()
        {
            return EditorUserSettings.GetConfigValue(LastOneClickBuildPlatformKey)?.Trim() ?? string.Empty;
        }

        private static void SaveLastOneClickBuildPlatform(BuildTarget buildTarget)
        {
            string platform = GetOneClickBuildPlatformKey(buildTarget);
            if (string.IsNullOrEmpty(platform))
            {
                return;
            }

            EditorUserSettings.SetConfigValue(LastOneClickBuildPlatformKey, platform);
            Debug.Log($"[ReleaseTools] Save last one-click build platform: {platform}");
        }

        private static bool BuildInternal(BuildTarget buildTarget, string outputRoot, string packageVersion = "1.0",
            EBuildPipeline buildPipeline = EBuildPipeline.ScriptableBuildPipeline)
        {
            Debug.Log($"开始构建 : {buildTarget}");
            Directory.CreateDirectory(outputRoot);

            IBuildPipeline pipeline = null;
            BuildParameters buildParameters = null;

            if (buildPipeline == EBuildPipeline.BuiltinBuildPipeline)
            {
                // 构建参数
                BuiltinBuildParameters builtinBuildParameters = new BuiltinBuildParameters();

                // 执行构建
                pipeline = new BuiltinBuildPipeline();
                buildParameters = builtinBuildParameters;

                builtinBuildParameters.CompressOption = ECompressOption.LZ4;
            }
            else
            {
                ScriptableBuildParameters scriptableBuildParameters = new ScriptableBuildParameters();

                // 执行构建
                pipeline = new ScriptableBuildPipeline();
                buildParameters = scriptableBuildParameters;

                scriptableBuildParameters.CompressOption = ECompressOption.LZ4;

                scriptableBuildParameters.BuiltinShadersBundleName = GetBuiltinShaderBundleName("DefaultPackage");
                // TODO
                // scriptableBuildParameters.ReplaceAssetPathWithAddress = ConfigComponent.EditorUpdateConfig.GetReplaceAssetPathWithAddress();
            }

            buildParameters.BuildOutputRoot = outputRoot; // AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = buildPipeline.ToString();
            buildParameters.BuildTarget = buildTarget;
            buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
            buildParameters.PackageName = "DefaultPackage";
            buildParameters.PackageVersion = packageVersion;
            buildParameters.VerifyBuildingResult = true;
            // 启用共享资源打包
            buildParameters.EnableSharePackRule = true;
            buildParameters.FileNameStyle = EFileNameStyle.BundleName_HashName;
            if (ResourceComponent.EditorResourceMode == ResourceMode.Package)
            {
                buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
                buildParameters.BuildinFileCopyParams = string.Empty;
            }
            else
            {
                buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyByTags;
                buildParameters.BuildinFileCopyParams = "Launcher";
            }

            buildParameters.EncryptionServices =
                GetEncryptionFromResourceComponent(); // CreateEncryptionInstance("DefaultPackage",buildPipeline);
            buildParameters.ClearBuildCacheFiles = false; //不清理构建缓存，启用增量构建，可以提高打包速度！
            buildParameters.UseAssetDependencyDB = true; //使用资源依赖关系数据库，可以提高打包速度！

            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
            {
                Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
                return true;
            }
            else
            {
                Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
                return false;
            }
        }

        /// <summary>
        /// 内置着色器资源包名称
        /// 注意：和自动收集的着色器资源包名保持一致！
        /// </summary>
        private static string GetBuiltinShaderBundleName(string packageName)
        {
            var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            return packRuleResult.GetBundleName(packageName, uniqueBundleName);
        }

        /// <summary>
        /// 根据 ResourceModuleDriver 的 encryptionType 获取对应的加密服务
        /// </summary>
        private static IEncryptionServices GetEncryptionFromResourceComponent()
        {
            // 通过名字查找 GameEntry 预制体
            var guids = AssetDatabase.FindAssets("t:Prefab LFramework");
            if (guids.Length == 0)
            {
                Debug.LogWarning("[BuildInternal] Failed to find LFramework.prefab");
                return null;
            }

            var gameEntryPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var gameEntryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gameEntryPath);
            if (gameEntryPrefab == null)
            {
                Debug.LogWarning("[BuildInternal] Failed to load LFramework.prefab");
                return null;
            }

            var resourceModuleDriver = gameEntryPrefab.GetComponentInChildren<ResourceComponent>();
            if (resourceModuleDriver == null)
            {
                Debug.LogWarning("[BuildInternal] ResourceComponent not found in LFramework.prefab");
                return null;
            }

            var encryptionType = resourceModuleDriver.EncryptionType;
            Debug.Log($"[BuildInternal] Use EncryptionType from ResourceComponent: {encryptionType}");

            return encryptionType switch
            {
                EncryptionType.FileOffSet => new FileOffsetEncryption(),
                EncryptionType.FileStream => new FileStreamEncryption(),
                _ => null // EncryptionType.None
            };
        }

        /// <summary>
        /// 创建加密类实例
        /// </summary>
        private static IEncryptionServices CreateEncryptionInstance(string packageName, EBuildPipeline buildPipeline)
        {
            var encryptionClassName =
                AssetBundleBuilderSetting.GetPackageEncyptionServicesClassName(packageName, buildPipeline.ToString());
            var encryptionClassTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
            var classType =
                encryptionClassTypes.Find(x => x.FullName != null && x.FullName.Equals(encryptionClassName));
            if (classType != null)
            {
                Debug.Log($"Use Encryption {classType}");
                return (IEncryptionServices)Activator.CreateInstance(classType);
            }
            else
            {
                return null;
            }
        }

        [MenuItem("LFramework/Build/一键打包Window", false, 75)]
        public static void AutomationBuild()
        {
            BuildFullPackage(
                BuildTargetGroup.Standalone,
                BuildTarget.StandaloneWindows64,
                GetAssetBundleOutputRoot(BuildTarget.StandaloneWindows64),
                GetPlayerOutputPath("Windows", "Release_Windows.exe"));
        }

        // 构建版本相关
        private static string GetBuildPackageVersion()
        {
            int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
        }

        [MenuItem("LFramework/Build/一键打包Android", false, 75)]
        public static void AutomationBuildAndroid()
        {
            string packageVersion = GetBuildPackageVersion();
            BuildFullPackage(
                BuildTargetGroup.Android,
                BuildTarget.Android,
                GetAssetBundleOutputRoot(BuildTarget.Android),
                GetPlayerOutputPath("Android", $"{packageVersion}_Android.apk"),
                packageVersion);
        }

        [MenuItem("LFramework/Build/一键打包IOS", false, 75)]
        public static void AutomationBuildIOS()
        {
            BuildFullPackage(
                BuildTargetGroup.iOS,
                BuildTarget.iOS,
                GetAssetBundleOutputRoot(BuildTarget.iOS),
                GetPlayerOutputPath("IOS", "XCode_Project"));
        }

        public static bool BuildImp(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, string locationPathName)
        {
            BuildTargetGroup actualBuildTargetGroup = GetBuildTargetGroup(buildTarget);
            if (buildTargetGroup != actualBuildTargetGroup)
            {
                Debug.LogWarning(
                    $"BuildTargetGroup mismatch. Use {actualBuildTargetGroup} for {buildTarget} instead of {buildTargetGroup}.");
                buildTargetGroup = actualBuildTargetGroup;
            }

            if (!TrySwitchBuildTarget(buildTarget))
            {
                return false;
            }

            string directoryName = Path.GetDirectoryName(locationPathName);
            if (!string.IsNullOrEmpty(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray(),
                locationPathName = locationPathName,
                targetGroup = buildTargetGroup,
                target = buildTarget,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build success: {summary.totalSize / 1024 / 1024} MB");
                return true;
            }
            else
            {
                Debug.LogError($"Build Failed: {summary.result}");
                return false;
            }
        }
    }
}
