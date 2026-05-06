namespace GameEditor
{
#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.U2D;
    using UnityEngine;
    using UnityEngine.U2D;

    /// <summary>
    /// 图集精灵信息管理类。
    /// 负责追踪精灵资源的变化，管理图集的脏标记，以及生成和更新图集。
    /// 这是图集自动生成系统的核心类，处理所有图集相关的逻辑。
    /// </summary>
    public static class EditorSpriteSaveInfo
    {
        #region 私有字段

        /// <summary>
        /// 需要重新创建的脏图集名称集合。
        /// 这些图集将被删除后重新生成。
        /// </summary>
        private static readonly HashSet<string> _dirtyAtlasNamesNeedCreateNew = new HashSet<string>();

        /// <summary>
        /// 需要更新的脏图集名称集合。
        /// 这些图集将在原有基础上更新。
        /// </summary>
        private static readonly HashSet<string> _dirtyAtlasNames = new HashSet<string>();

        /// <summary>
        /// 图集名称到精灵路径列表的映射。
        /// Key: 图集名称, Value: 该图集包含的所有精灵资源路径列表。
        /// </summary>
        private static readonly Dictionary<string, List<string>> _atlasMap = new Dictionary<string, List<string>>();

        /// <summary>
        /// 图集名称到图集文件路径的映射。
        /// Key: 图集名称, Value: 图集文件的完整路径。
        /// </summary>
        private static readonly Dictionary<string, string> _atlasPathMap = new Dictionary<string, string>();

        /// <summary>
        /// 是否已初始化标志。
        /// </summary>
        private static bool _initialized;

        /// <summary>
        /// 是否正在扫描现有精灵标志。
        /// 用于防止在扫描过程中触发重复处理。
        /// </summary>
        private static bool _isInScanExistingSprites;

        /// <summary>
        /// 是否为构建变更模式标志。
        /// 在此模式下会比较时间戳来决定是否需要重新生成。
        /// </summary>
        private static bool _isBuildChange = false;

        #endregion

        /// <summary>
        /// 获取图集配置实例的便捷属性。
        /// </summary>
        private static AtlasConfiguration Config => AtlasConfiguration.Instance;

        /// <summary>
        /// 静态构造函数。
        /// 注册编辑器更新回调并初始化系统。
        /// </summary>
        static EditorSpriteSaveInfo()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            Initialize();
        }

        /// <summary>
        /// 初始化图集管理系统。
        /// 扫描现有的精灵资源并建立映射关系。
        /// </summary>
        private static void Initialize()
        {
            if (_initialized) return;
            ScanExistingSprites(false);
            _initialized = true;
        }

        #region 公共方法

        /// <summary>
        /// 处理精灵导入事件。
        /// 当新的精灵资源被导入时调用，将其添加到对应的图集映射中。
        /// </summary>
        /// <param name="assetPath">导入的资源路径。</param>
        /// <param name="isCreateNew">是否需要重新创建图集。</param>
        public static void OnImportSprite(string assetPath, bool isCreateNew = false)
        {
            assetPath = assetPath.Replace("\\", "/");
            if (!ShouldProcess(assetPath)) return;

            var atlasName = GetAtlasName(assetPath);
            if (string.IsNullOrEmpty(atlasName)) return;

            // 检查是否需要生成单独图集
            if (CheckIsNeedGenerateSingleAtlas(assetPath))
            {
                atlasName = GetSingleAtlasName(assetPath);
            }
            // 检查是否需要按根目录子级生成图集
            else if (CheckIsNeedGenerateRootChildDirAtlas(assetPath))
            {
                atlasName = GetRootChildDirAtlasName(assetPath);
            }

            // 添加到图集映射
            if (!_atlasMap.TryGetValue(atlasName, out var list))
            {
                list = new List<string>();
                _atlasMap[atlasName] = list;
            }

            if (!list.Contains(assetPath))
            {
                list.Add(assetPath);
                MarkDirty(atlasName, isCreateNew);
                MarkParentAtlasesDirty(assetPath, isCreateNew);
            }
        }

        /// <summary>
        /// 处理精灵删除事件。
        /// 当精灵资源被删除时调用，将其从对应的图集映射中移除。
        /// </summary>
        /// <param name="assetPath">删除的资源路径。</param>
        /// <param name="isCreateNew">是否需要重新创建图集。</param>
        public static void OnDeleteSprite(string assetPath, bool isCreateNew = true)
        {
            assetPath = assetPath.Replace("\\", "/");
            if (!ShouldProcess(assetPath)) return;

            var atlasName = GetAtlasName(assetPath);
            if (string.IsNullOrEmpty(atlasName)) return;

            // 检查是否需要生成单独图集
            if (CheckIsNeedGenerateSingleAtlas(assetPath))
            {
                atlasName = GetSingleAtlasName(assetPath);
            }
            // 检查是否需要按根目录子级生成图集
            else if (CheckIsNeedGenerateRootChildDirAtlas(assetPath))
            {
                atlasName = GetRootChildDirAtlasName(assetPath);
            }

            // 从图集映射中移除
            if (_atlasMap.TryGetValue(atlasName, out var list))
            {
                if (list.Remove(assetPath))
                {
                    MarkDirty(atlasName, isCreateNew);
                    MarkParentAtlasesDirty(assetPath, isCreateNew);
                }
            }
        }

        /// <summary>
        /// 菜单项：立即重新生成变动的图集数据。
        /// 只重新生成有变化的图集，不会删除所有图集。
        /// </summary>
        [MenuItem("Tools/图集工具/立即重新生成变动的图集数据")]
        public static void ForceGenerateAll()
        {
            _isBuildChange = true;
            ForceGenerateAll(false);
            _isBuildChange = false;
        }

        /// <summary>
        /// 强制重新生成所有图集。
        /// </summary>
        /// <param name="isClearAll">是否清除所有现有图集后重新生成。</param>
        public static void ForceGenerateAll(bool isClearAll)
        {
            _isInScanExistingSprites = true;
            if (isClearAll)
            {
                _atlasPathMap.Clear();
                ClearCache();
                ClearAllAtlas();
            }

            _atlasMap.Clear();
            ScanExistingSprites();

            // 根据模式决定哪些图集需要重新生成
            if (_isBuildChange)
            {
                // 构建变更模式：只重新生成时间戳较旧的图集
                foreach (var item in _atlasMap)
                {
                    if (GetLatestAtlasTime(item.Key) >= GetLatestSpriteTime(item.Key))
                    {
                        continue;
                    }
                    else
                    {
                        _dirtyAtlasNamesNeedCreateNew.Add(item.Key);
                    }
                }
            }
            else
            {
                // 全量重新生成模式
                _dirtyAtlasNamesNeedCreateNew.UnionWith(_atlasMap.Keys);
            }

            ProcessDirtyAtlases(true);
            _isInScanExistingSprites = false;
        }

        /// <summary>
        /// 清除所有缓存数据。
        /// 包括脏标记集合和图集映射。
        /// </summary>
        public static void ClearCache()
        {
            _dirtyAtlasNamesNeedCreateNew.Clear();
            _dirtyAtlasNames.Clear();
            _atlasMap.Clear();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 标记父级图集为脏。
        /// 当子目录中的精灵发生变化时，需要同时更新父级目录对应的图集。
        /// </summary>
        /// <param name="assetPath">资源路径。</param>
        /// <param name="isCreateNew">是否需要重新创建。</param>
        public static void MarkParentAtlasesDirty(string assetPath, bool isCreateNew)
        {
            var currentPath = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");

            if (string.IsNullOrEmpty(currentPath)) return;
            var tempRootDirArr = new List<string>(Config.sourceAtlasRootDir);
            tempRootDirArr.AddRange(Config.rootChildAtlasDir);

            // 遍历所有根目录，向上查找并标记父级图集
            foreach (var rootPath in tempRootDirArr)
            {
                var tempPath = rootPath.Replace("\\", "/").TrimEnd('/');
                var tempCurrentPath = currentPath;

                if (!tempCurrentPath.StartsWith(tempPath))
                {
                    continue;
                }

                // 向上遍历目录树
                while (tempCurrentPath != null && tempCurrentPath.StartsWith(tempPath))
                {
                    var parentAtlasName = GetAtlasNameForDirectory(tempCurrentPath);

                    if (!string.IsNullOrEmpty(parentAtlasName))
                    {
                        MarkDirty(parentAtlasName, isCreateNew);
                    }

                    tempCurrentPath = Path.GetDirectoryName(tempCurrentPath)?.Replace("\\", "/");
                }
            }
        }

        #endregion

        #region 私有方法 - 更新处理

        /// <summary>
        /// 编辑器更新回调。
        /// 在每帧检查是否有脏图集需要处理。
        /// </summary>
        private static void OnUpdate()
        {
            if (_isInScanExistingSprites) return;
            if (_dirtyAtlasNames.Count > 0 || _dirtyAtlasNamesNeedCreateNew.Count > 0)
            {
                ProcessDirtyAtlases();
            }
        }

        /// <summary>
        /// 处理所有脏图集。
        /// 遍历脏标记集合，生成或更新对应的图集。
        /// </summary>
        /// <param name="force">是否强制处理，忽略时间戳检查。</param>
        private static void ProcessDirtyAtlases(bool force = false)
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                // 处理需要更新的图集
                while (_dirtyAtlasNames.Count > 0)
                {
                    var atlasName = _dirtyAtlasNames.First();
                    if (force || ShouldUpdateAtlas(atlasName))
                    {
                        GenerateAtlas(atlasName, false);
                    }

                    _dirtyAtlasNames.Remove(atlasName);
                }

                // 处理需要重新创建的图集
                while (_dirtyAtlasNamesNeedCreateNew.Count > 0)
                {
                    var atlasName = _dirtyAtlasNamesNeedCreateNew.First();
                    if (force || ShouldUpdateAtlas(atlasName))
                    {
                        GenerateAtlas(atlasName, true);
                    }

                    _dirtyAtlasNamesNeedCreateNew.Remove(atlasName);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        #endregion

        #region 私有方法 - 图集生成

        /// <summary>
        /// 清除所有已生成的图集文件。
        /// 删除输出目录下的所有 .spriteatlas 和 .spriteatlasv2 文件。
        /// </summary>
        private static void ClearAllAtlas()
        {
            string[] atlasV2Files =
                Directory.GetFiles(Config.outputAtlasDir, "*.spriteatlasv2", SearchOption.AllDirectories);
            string[] atlasFiles =
                Directory.GetFiles(Config.outputAtlasDir, "*.spriteatlas", SearchOption.AllDirectories);

            foreach (string filePath in atlasFiles)
            {
                AssetDatabase.DeleteAsset(filePath);
            }

            foreach (string filePath in atlasV2Files)
            {
                AssetDatabase.DeleteAsset(filePath);
            }

            AssetDatabase.Refresh();
            Debug.Log($"已删除 {atlasFiles?.Length + atlasV2Files?.Length} 个图集文件");
        }

        /// <summary>
        /// 生成指定名称的图集。
        /// </summary>
        /// <param name="atlasName">图集名称。</param>
        /// <param name="createNew">是否删除现有图集后重新创建。</param>
        private static void GenerateAtlas(string atlasName, bool createNew = false)
        {
            var outputPath = $"{Config.outputAtlasDir}/{atlasName}.spriteatlas";
            var outputPathV2 = outputPath.Replace(".spriteatlas", ".spriteatlasv2");
            string deletePath = outputPath;

            // 根据配置决定使用 V1 还是 V2 格式
            if (Config.enableV2)
            {
                DeleteAtlas(outputPath);
                deletePath = outputPathV2;
            }
            else
            {
                DeleteAtlas(outputPathV2);
                deletePath = outputPath;
            }

            // 如果需要重新创建，先删除现有文件
            if (createNew && File.Exists(deletePath))
            {
                AssetDatabase.DeleteAsset(deletePath);
            }

            var sprites = LoadValidSprites(atlasName);
            EnsureOutputDirectory();

            // 如果没有精灵，删除图集文件
            if (sprites.Count == 0)
            {
                DeleteAtlas(deletePath);
                return;
            }

            AssetDatabase.Refresh();
            EditorApplication.delayCall += () => { InternalGenerateAtlas(atlasName, sprites, outputPath); };
        }

        /// <summary>
        /// 内部图集生成方法。
        /// 实际创建或更新 SpriteAtlas 资源。
        /// </summary>
        /// <param name="atlasName">图集名称。</param>
        /// <param name="sprites">要打入图集的精灵列表。</param>
        /// <param name="outputPath">输出路径。</param>
        /// <returns>最终的图集文件路径。</returns>
        private static string InternalGenerateAtlas(string atlasName, List<Sprite> sprites, string outputPath)
        {
            SpriteAtlasAsset spriteAtlasAsset = null;
            SpriteAtlas atlas = null;

            // V2 格式处理
            if (Config.enableV2)
            {
                outputPath = outputPath.Replace(".spriteatlas", ".spriteatlasv2");

                if (!File.Exists(outputPath))
                {
                    spriteAtlasAsset = new SpriteAtlasAsset();
                    atlas = new SpriteAtlas();
                }
                else
                {
                    spriteAtlasAsset = SpriteAtlasAsset.Load(outputPath);
                    atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(outputPath);
                    if (atlas != null)
                    {
                        var olds = atlas.GetPackables();

                        if (olds != null)
                        {
                            spriteAtlasAsset.Remove(olds);
                        }
                    }
                }
            }

            // 根据格式版本进行不同处理
            if (Config.enableV2)
            {
                spriteAtlasAsset?.Add(sprites.ToArray());
                SpriteAtlasAsset.Save(spriteAtlasAsset, outputPath);
                AssetDatabase.Refresh();
                EditorApplication.delayCall += () =>
                {
#if UNITY_2022_1_OR_NEWER
                    SpriteAtlasImporter sai = (SpriteAtlasImporter)AssetImporter.GetAtPath(outputPath);
                    ConfigureAtlasV2Settings(sai);
#else
                    ConfigureAtlasV2Settings(spriteAtlasAsset);
                    SpriteAtlasAsset.Save(spriteAtlasAsset, outputPath);
#endif
                    AssetDatabase.WriteImportSettingsIfDirty(outputPath);
                    AssetDatabase.Refresh();
                };
            }
            else
            {
                // V1 格式处理
                atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(outputPath);

                if (atlas != null)
                {
                    var olds = atlas.GetPackables();
                    if (olds != null)
                    {
                        atlas.Remove(olds);
                    }

                    ConfigureAtlasSettings(atlas);
                    atlas.Add(sprites.ToArray());
                    atlas.SetIsVariant(false);
                }
                else
                {
                    atlas = new SpriteAtlas();
                    ConfigureAtlasSettings(atlas);
                    atlas.Add(sprites.ToArray());
                    atlas.SetIsVariant(false);
                    AssetDatabase.CreateAsset(atlas, outputPath);
                }
            }

            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (File.Exists(outputPath))
            {
                _atlasPathMap[atlasName] = outputPath;
            }

            if (Config.enableLogging)
            {
                Debug.Log($"<b>[Generate Atlas]</b>: {atlasName} ({sprites.Count} sprites)");
            }

            return outputPath;
        }

        /// <summary>
        /// 加载指定图集的所有有效精灵。
        /// </summary>
        /// <param name="atlasName">图集名称。</param>
        /// <returns>精灵列表。</returns>
        private static List<Sprite> LoadValidSprites(string atlasName)
        {
            if (_atlasMap.TryGetValue(atlasName, out List<string> spriteList))
            {
                var allSprites = new List<Sprite>();

                foreach (var assetPath in spriteList.Where(File.Exists))
                {
                    // 加载所有子图（支持多精灵纹理）
                    var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                        .OfType<Sprite>()
                        .Where(s => s != null)
                        .ToArray();

                    allSprites.AddRange(sprites);
                }

                return allSprites;
            }

            return new List<Sprite>();
        }

        #endregion

        #region 私有方法 - 图集配置

#if UNITY_2022_1_OR_NEWER
        /// <summary>
        /// 配置 V2 图集设置（Unity 2022.1+）。
        /// 使用 SpriteAtlasImporter 进行配置。
        /// </summary>
        /// <param name="atlasImporter">图集导入器。</param>
        private static void ConfigureAtlasV2Settings(SpriteAtlasImporter atlasImporter)
        {
            // 设置平台特定格式的内部方法
            void SetPlatform(string platform, TextureImporterFormat format)
            {
                var settings = atlasImporter.GetPlatformSettings(platform);
                if (settings == null) return;
                ;
                settings.overridden = true;
                settings.format = format;
                settings.compressionQuality = Config.compressionQuality;
                atlasImporter.SetPlatformSettings(settings);
            }
            
            SetPlatform("Android", Config.androidFormat);
            SetPlatform("iPhone", Config.iosFormat);
            SetPlatform("WebGL", Config.webglFormat);
            
            // 配置打包设置
            var packingSettings = new SpriteAtlasPackingSettings
            {
                padding = Config.padding,
                enableRotation = Config.enableRotation,
                blockOffset = Config.blockOffset,
                enableTightPacking = Config.tightPacking,
                enableAlphaDilation = true
            };
            atlasImporter.packingSettings = packingSettings;
        }
#else
        /// <summary>
        /// 配置 V2 图集设置（Unity 2022.1 之前版本）。
        /// 使用 SpriteAtlasAsset 进行配置。
        /// </summary>
        /// <param name="spriteAtlasAsset">图集资源。</param>
        private static void ConfigureAtlasV2Settings(SpriteAtlasAsset spriteAtlasAsset)
        {
            // 设置平台特定格式的内部方法
            void SetPlatform(string platform, TextureImporterFormat format)
            {
                var settings = spriteAtlasAsset.GetPlatformSettings(platform);
                if (settings == null) return;
                ;
                settings.overridden = true;
                settings.format = format;
                settings.compressionQuality = Config.compressionQuality;
                spriteAtlasAsset.SetPlatformSettings(settings);
            }

            SetPlatform("Android", Config.androidFormat);
            SetPlatform("iPhone", Config.iosFormat);
            SetPlatform("WebGL", Config.webglFormat);

            // 配置打包设置
            var packingSettings = new SpriteAtlasPackingSettings
            {
                padding = Config.padding,
                enableRotation = Config.enableRotation,
                blockOffset = Config.blockOffset,
                enableTightPacking = Config.tightPacking,
                enableAlphaDilation = true
            };
            spriteAtlasAsset.SetPackingSettings(packingSettings);
        }
#endif

        /// <summary>
        /// 配置 V1 图集设置。
        /// </summary>
        /// <param name="atlas">图集对象。</param>
        private static void ConfigureAtlasSettings(SpriteAtlas atlas)
        {
            // 设置平台特定格式的内部方法
            void SetPlatform(string platform, TextureImporterFormat format)
            {
                var settings = atlas.GetPlatformSettings(platform);
                settings.overridden = true;
                settings.format = format;
                settings.compressionQuality = Config.compressionQuality;
                atlas.SetPlatformSettings(settings);
            }

            SetPlatform("Android", Config.androidFormat);
            SetPlatform("iPhone", Config.iosFormat);
            SetPlatform("WebGL", Config.webglFormat);

            // 配置打包设置
            var packingSettings = new SpriteAtlasPackingSettings
            {
                padding = Config.padding,
                enableRotation = Config.enableRotation,
                blockOffset = Config.blockOffset,
                enableTightPacking = Config.tightPacking,
            };
            atlas.SetPackingSettings(packingSettings);
        }

        #endregion

        #region 私有方法 - 图集名称计算

        /// <summary>
        /// 根据资源路径获取图集名称。
        /// 图集名称由根目录名和相对路径组成，使用下划线连接。
        /// </summary>
        /// <param name="assetPath">资源路径。</param>
        /// <returns>图集名称，如果不在配置的目录下则返回 null。</returns>
        private static string GetAtlasName(string assetPath)
        {
            var tempRootDirArr = new List<string>(Config.sourceAtlasRootDir);
            tempRootDirArr.AddRange(Config.rootChildAtlasDir);
            foreach (var rootPath in tempRootDirArr)
            {
                var tempPath = rootPath.Replace("\\", "/").TrimEnd('/');
                if (!assetPath.StartsWith(tempPath + "/"))
                {
                    continue;
                }

                var relativePath = assetPath.Substring(tempPath.Length + 1).Split('/');
                // 根目录下文件不处理
                if (relativePath.Length < 2)
                {
                    return null;
                }

                // 提取目录部分（排除文件名）
                var directories = relativePath.Take(relativePath.Length - 1);
                var atlasNames = string.Join("_", directories);
                // 根目录文件名
                var rootFolderName = Path.GetFileName(tempPath);
                return $"{rootFolderName}_{atlasNames}";
            }

            return null;
        }

        #endregion

        #region 私有方法 - 过滤和验证

        /// <summary>
        /// 判断资源是否应该被处理。
        /// </summary>
        /// <param name="assetPath">资源路径。</param>
        /// <returns>是否应该处理。</returns>
        private static bool ShouldProcess(string assetPath)
        {
            return IsImageFile(assetPath) && !IsExcluded(assetPath);
        }

        /// <summary>
        /// 判断资源是否被排除。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <returns>是否被排除。</returns>
        private static bool IsExcluded(string path)
        {
            return CheckIsExcludeFolder(path)
                   || Config.excludeKeywords.Any(key => path.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// 判断是否为图片文件。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <returns>是否为图片文件。</returns>
        private static bool IsImageFile(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg";
        }

        /// <summary>
        /// 标记图集为脏，需要重新生成。
        /// </summary>
        /// <param name="atlasName">图集名称。</param>
        /// <param name="isCreateNew">是否需要重新创建。</param>
        private static void MarkDirty(string atlasName, bool isCreateNew = false)
        {
            // 构建变更模式下，检查时间戳
            if (_isBuildChange)
            {
                if (GetLatestAtlasTime(atlasName) > GetLatestSpriteTime(atlasName))
                {
                    return;
                }
            }

            if (isCreateNew)
            {
                _dirtyAtlasNamesNeedCreateNew.Add(atlasName);
            }
            else
            {
                if (!_dirtyAtlasNamesNeedCreateNew.Contains(atlasName))
                {
                    _dirtyAtlasNames.Add(atlasName);
                }
            }
        }

        /// <summary>
        /// 判断图集是否需要更新。
        /// </summary>
        /// <param name="atlasName">图集名称。</param>
        /// <returns>是否需要更新。</returns>
        private static bool ShouldUpdateAtlas(string atlasName)
        {
            return true;
        }

        /// <summary>
        /// 获取图集中最新精灵的修改时间。
        /// </summary>
        /// <param name="atlasName">图集名称。</param>
        /// <returns>最新修改时间。</returns>
        private static DateTime GetLatestSpriteTime(string atlasName)
        {
            if (_atlasMap.TryGetValue(atlasName, out List<string> list))
            {
                return list
                    .Select(p => new FileInfo(p).LastWriteTime)
                    .DefaultIfEmpty()
                    .Max();
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// 获取图集文件的修改时间。
        /// </summary>
        /// <param name="atlasName">图集名称。</param>
        /// <returns>图集文件修改时间。</returns>
        private static DateTime GetLatestAtlasTime(string atlasName)
        {
            if (_atlasPathMap.TryGetValue(atlasName, out var atlasPath))
            {
                return new FileInfo(atlasPath).LastWriteTime;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// 删除指定路径的图集文件。
        /// </summary>
        /// <param name="path">图集文件路径。</param>
        private static void DeleteAtlas(string path)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                if (Config.enableLogging)
                    Debug.Log($"Deleted empty atlas: {Path.GetFileName(path)}");
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 确保输出目录存在。
        /// </summary>
        private static void EnsureOutputDirectory()
        {
            if (!Directory.Exists(Config.outputAtlasDir))
            {
                Directory.CreateDirectory(Config.outputAtlasDir);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 扫描现有的精灵资源。
        /// 遍历配置的源目录，建立精灵到图集的映射关系。
        /// </summary>
        /// <param name="isCreateNew">是否标记为需要重新创建。</param>
        private static void ScanExistingSprites(bool isCreateNew = true)
        {
            List<string> sprites = new List<string>();
            var guids = AssetDatabase.FindAssets("t:sprite", Config.sourceAtlasRootDir);
            sprites.AddRange(guids);
            guids = AssetDatabase.FindAssets("t:sprite", Config.rootChildAtlasDir);
            sprites.AddRange(guids);
            foreach (var guid in sprites)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (ShouldProcess(path))
                {
                    OnImportSprite(path, isCreateNew);
                }
            }
        }

        /// <summary>
        /// 根据目录路径获取图集名称。
        /// </summary>
        /// <param name="directoryPath">目录路径。</param>
        /// <returns>图集名称。</returns>
        private static string GetAtlasNameForDirectory(string directoryPath)
        {
            foreach (var rootPath in Config.sourceAtlasRootDir)
            {
                var tempPath = rootPath.Replace("\\", "/").TrimEnd('/');
                if (!directoryPath.StartsWith(tempPath + "/"))
                {
                    continue;
                }

                var relativePath = directoryPath.Substring(rootPath.Length + 1).Split('/');
                var atlasNamePart = string.Join("_", relativePath);
                var rootFolderName = Path.GetFileName(rootPath);
                return $"{rootFolderName}_{atlasNamePart}";
            }

            return null;
        }

        /// <summary>
        /// 获取单独图集的名称。
        /// 用于配置为每张图单独生成图集的目录。
        /// </summary>
        /// <param name="spritePath">精灵路径。</param>
        /// <returns>单独图集名称。</returns>
        private static string GetSingleAtlasName(string spritePath)
        {
            foreach (var rootPath in Config.sourceAtlasRootDir)
            {
                var tempPath = rootPath.Replace("\\", "/").TrimEnd('/');
                if (!spritePath.StartsWith(tempPath + "/"))
                {
                    continue;
                }

                var relativePath = spritePath.Substring(tempPath.Length + 1).Split('/');
                // 根目录下文件不处理
                if (relativePath.Length < 2)
                {
                    return null;
                }

                // 使用文件名（不含扩展名）作为图集名称的一部分
                relativePath[^1] = Path.GetFileNameWithoutExtension(spritePath);
                var atlasNames = string.Join("_", relativePath);
                var rootFolderName = Path.GetFileName(tempPath);
                return $"{rootFolderName}_{atlasNames}";
            }

            return null;
        }

        /// <summary>
        /// 检查是否需要为该精灵生成单独图集。
        /// </summary>
        /// <param name="spritePath">精灵路径。</param>
        /// <returns>是否需要单独图集。</returns>
        private static bool CheckIsNeedGenerateSingleAtlas(string spritePath)
        {
            return !CheckIsExcludeFolder(spritePath)
                   && Config.singleAtlasDir.Any(key =>
                       spritePath.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// 检查是否需要按根目录子级生成图集。
        /// </summary>
        /// <param name="spritePath">精灵路径。</param>
        /// <returns>是否需要按子级生成。</returns>
        private static bool CheckIsNeedGenerateRootChildDirAtlas(string spritePath)
        {
            return !CheckIsExcludeFolder(spritePath)
                   && Config.rootChildAtlasDir.Any(key =>
                       spritePath.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// 获取根目录子级图集的名称。
        /// </summary>
        /// <param name="spritePath">精灵路径。</param>
        /// <returns>子级图集名称。</returns>
        private static string GetRootChildDirAtlasName(string spritePath)
        {
            foreach (var rootPath in Config.rootChildAtlasDir)
            {
                var tempPath = rootPath.Replace("\\", "/").TrimEnd('/');
                if (spritePath.StartsWith(tempPath))
                {
                    string[] subDirectories = AssetDatabase.GetSubFolders(tempPath);
                    foreach (var subDirectory in subDirectories)
                    {
                        if (spritePath.StartsWith(subDirectory))
                        {
                            string rootName = Path.GetFileName(tempPath);
                            string directoryName = Path.GetFileName(subDirectory);
                            return $"{rootName}_{directoryName}";
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 检查路径是否在排除目录中。
        /// </summary>
        /// <param name="assetPath">资源路径。</param>
        /// <returns>是否被排除。</returns>
        private static bool CheckIsExcludeFolder(string assetPath)
        {
            foreach (var rootPath in AtlasConfiguration.Instance.excludeFolder)
            {
                var tempPath = rootPath.Replace("\\", "/").TrimEnd('/');
                if (assetPath.StartsWith(tempPath + "/"))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }

#endif
}