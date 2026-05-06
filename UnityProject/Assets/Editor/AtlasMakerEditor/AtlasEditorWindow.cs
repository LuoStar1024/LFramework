namespace GameEditor
{
#if UNITY_EDITOR
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 图集配置编辑器窗口。
    /// 提供可视化界面来配置图集生成的各项参数，包括目录设置、平台格式、打包参数等。
    /// 通过菜单 Tools/图集工具/配置面板 打开。
    /// </summary>
    public class AtlasConfigWindow : EditorWindow
    {
        /// <summary>
        /// 菜单项：打开图集配置窗口。
        /// </summary>
        [MenuItem("Tools/图集工具/配置面板")]
        public static void ShowWindow()
        {
            var window = GetWindow<AtlasConfigWindow>();
            window.titleContent = new GUIContent(" 图集配置窗口", EditorGUIUtility.IconContent("Settings").image);
            window.minSize = new Vector2(450, 400);
        }

        #region 私有字段

        /// <summary>
        /// 滚动视图位置。
        /// </summary>
        private Vector2 _scrollPosition;

        /// <summary>
        /// Padding 可选值枚举。
        /// </summary>
        private int[] _paddingEnum = new int[] { 2, 4, 8 };

        /// <summary>
        /// 排除关键词折叠状态。
        /// </summary>
        private bool _showExcludeKeywords = false;

        /// <summary>
        /// 单张图集目录折叠状态。
        /// </summary>
        private bool _showSingleAtlasPath = false;

        /// <summary>
        /// 根目录子级图集目录折叠状态。
        /// </summary>
        private bool _showRootDirAtlasPath = false;

        /// <summary>
        /// 源图集根目录折叠状态。
        /// </summary>
        private bool _showSourceAtlasRootPath = false;

        /// <summary>
        /// 排除目录折叠状态。
        /// </summary>
        private bool _showExcludeAtlasPath = false;

        #endregion

        #region Unity 生命周期

        /// <summary>
        /// 绘制窗口 GUI。
        /// </summary>
        private void OnGUI()
        {
            var config = AtlasConfiguration.Instance;

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;

                EditorGUI.BeginChangeCheck();

                // 绘制各个设置区域
                DrawFolderSettings(config);
                DrawPlatformSettings(config);
                DrawPackingSettings(config);
                DrawSpriteImportSettings(config);
                DrawAdvancedSettings(config);

                // 如果有修改，保存配置
                if (EditorGUI.EndChangeCheck())
                {
                    AtlasConfiguration.Save(true);
                    AssetDatabase.Refresh();
                }

                DrawActionButtons();
            }
        }

        #endregion

        #region 绘制方法 - 设置区域

        /// <summary>
        /// 绘制 Sprite 导入设置区域。
        /// </summary>
        /// <param name="config">配置实例。</param>
        private void DrawSpriteImportSettings(AtlasConfiguration config)
        {
            EditorGUILayout.BeginVertical("box");
            var labelGUIContent = new GUIContent(" Sprite导入设置", EditorGUIUtility.IconContent("Sprite Icon").image);
            GUILayout.Label(labelGUIContent, EditorStyles.boldLabel, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            var checkMipmapsContent =
                new GUIContent(" 检查Mipmap导入设置", EditorGUIUtility.IconContent("LODGroup Icon").image);
            config.checkMipmaps = EditorGUILayout.Toggle(checkMipmapsContent, config.checkMipmaps);
            if (config.checkMipmaps)
            {
                var enableMipmapsContent =
                    new GUIContent(" 允许Mipmap", EditorGUIUtility.IconContent("FilterByType").image);
                config.enableMipmaps = EditorGUILayout.Toggle(enableMipmapsContent, config.enableMipmaps);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// 绘制目录设置区域。
        /// 包含输出目录和各种路径数组配置（收集目录、排除目录、子级图集目录、单张图集目录）。
        /// </summary>
        /// <param name="config">配置实例。</param>
        private void DrawFolderSettings(AtlasConfiguration config)
        {
            EditorGUILayout.BeginVertical("box");
            var labelGUIContent = new GUIContent(" 目录设置", EditorGUIUtility.IconContent("Folder Icon").image);
            GUILayout.Label(labelGUIContent, EditorStyles.boldLabel, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            config.outputAtlasDir = DrawFolderField("输出目录", "FolderOpened Icon", config.outputAtlasDir);
            DrawPathArrItem("收集目录", "收集目录", "Collab.FolderAdded", ref config.sourceAtlasRootDir,
                ref _showSourceAtlasRootPath);
            DrawPathArrItem("排除目录", "排除目录", "Collab.FolderIgnored", ref config.excludeFolder,
                ref _showExcludeAtlasPath);
            DrawPathArrItem("以根目录的子级目录生成图集", "根目录", "Collab.FolderAdded", ref config.rootChildAtlasDir,
                ref _showRootDirAtlasPath);
            DrawPathArrItem("每张图都单独生成图集的目录", "单张图集目录", "Collab.FolderAdded", ref config.singleAtlasDir,
                ref _showSingleAtlasPath);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// 绘制路径数组配置项。
        /// 支持折叠、添加、删除和清空操作。
        /// </summary>
        /// <param name="label">标签文本。</param>
        /// <param name="itemLabel">单项标签。</param>
        /// <param name="iconName">图标名称。</param>
        /// <param name="paths">路径数组引用。</param>
        /// <param name="isShow">折叠状态引用。</param>
        private void DrawPathArrItem(string label, string itemLabel, string iconName, ref string[] paths,
            ref bool isShow)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            isShow = EditorGUILayout.BeginFoldoutHeaderGroup(isShow, label);
            if (isShow)
            {
                GUILayout.Label("数量:", GUILayout.ExpandWidth(false));
                int newSize = EditorGUILayout.IntField(paths.Length, GUILayout.Width(40));
                newSize = Mathf.Max(0, newSize);
                if (newSize != paths.Length)
                {
                    Array.Resize(ref paths, newSize);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), GUILayout.Width(25),
                        GUILayout.Height(20)))
                {
                    Array.Resize(ref paths, paths.Length + 1);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(25),
                        GUILayout.Height(20)) && paths.Length > 0)
                {
                    Array.Resize(ref paths, paths.Length - 1);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (isShow)
            {
                EditorGUILayout.BeginVertical("box");
                for (int i = 0; i < paths.Length; i++)
                {
                    paths[i] = DrawFolderField($"{itemLabel}[{i}]", iconName, paths[i]);
                }

                GUILayout.Space(2);
                if (GUILayout.Button(new GUIContent(" 清空", EditorGUIUtility.IconContent("d_TreeEditor.Trash").image),
                        GUILayout.Height(25)))
                {
                    paths = Array.Empty<string>();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// 绘制文件夹选择字段。
        /// 包含文本输入框和文件夹选择按钮。
        /// </summary>
        /// <param name="label">标签文本。</param>
        /// <param name="labelIcon">标签图标名称。</param>
        /// <param name="path">当前路径。</param>
        /// <returns>修改后的路径。</returns>
        private string DrawFolderField(string label, string labelIcon, string path)
        {
            using var horizontalScope = new EditorGUILayout.HorizontalScope();
            var buttonGUIContent = new GUIContent("选择", EditorGUIUtility.IconContent("Folder Icon").image);
            var labelGUIContent = new GUIContent(" " + label, EditorGUIUtility.IconContent(labelIcon).image);
            path = EditorGUILayout.TextField(labelGUIContent, path);

            if (GUILayout.Button(buttonGUIContent, GUILayout.Width(60), GUILayout.Height(20)))
            {
                var newPath = EditorUtility.OpenFolderPanel(label, Application.dataPath, string.Empty);

                if (!string.IsNullOrEmpty(newPath) && newPath.StartsWith(Application.dataPath))
                {
                    path = "Assets" + newPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    Debug.LogError("路径不在Unity项目内: " + newPath);
                }
            }

            return path;
        }

        /// <summary>
        /// 绘制平台设置区域。
        /// 配置各平台的纹理压缩格式（Android、iOS、WebGL）和压缩质量。
        /// </summary>
        /// <param name="config">配置实例。</param>
        private void DrawPlatformSettings(AtlasConfiguration config)
        {
            EditorGUILayout.BeginVertical("box");
            var labelGUIContent =
                new GUIContent(" 平台设置", EditorGUIUtility.IconContent("BuildSettings.Standalone").image);
            GUILayout.Label(labelGUIContent, EditorStyles.boldLabel, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            var androidContent = new GUIContent(" Android 格式",
                EditorGUIUtility.IconContent("BuildSettings.Android.Small").image);
            config.androidFormat =
                (TextureImporterFormat)EditorGUILayout.EnumPopup(androidContent, config.androidFormat);
            var iosContent =
                new GUIContent(" iOS 格式", EditorGUIUtility.IconContent("BuildSettings.iPhone.Small").image);
            config.iosFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup(iosContent, config.iosFormat);
            var webGLContent =
                new GUIContent(" WebGL 格式", EditorGUIUtility.IconContent("BuildSettings.WebGL.Small").image);
            config.webglFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup(webGLContent, config.webglFormat);
            var compressionContent = new GUIContent(" 压缩质量", EditorGUIUtility.IconContent("MeshRenderer Icon").image);
            config.compressionQuality =
                EditorGUILayout.IntSlider(compressionContent, config.compressionQuality, 0, 100);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// 绘制图集打包设置区域。
        /// 配置 Padding、Block Offset、旋转、紧密打包等参数。
        /// </summary>
        /// <param name="config">配置实例。</param>
        private void DrawPackingSettings(AtlasConfiguration config)
        {
            EditorGUILayout.BeginVertical("box");
            var labelGUIContent = new GUIContent(" 图集设置", EditorGUIUtility.IconContent("SpriteAtlas Icon").image);
            GUILayout.Label(labelGUIContent, EditorStyles.boldLabel, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            GUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.IconContent("RectTransformBlueprint"), GUILayout.Width(16),
                GUILayout.Height(18));
            config.padding = EditorGUILayout.IntPopup("Padding", config.padding,
                Array.ConvertAll(_paddingEnum, x => x.ToString()), _paddingEnum, GUILayout.Height(20));
            GUILayout.EndHorizontal();
            var offsetContent = new GUIContent(" Block Offset", EditorGUIUtility.IconContent("MoveTool").image);
            config.blockOffset = EditorGUILayout.IntField(offsetContent, config.blockOffset);
            var rotationContent = new GUIContent(" Enable Rotation", EditorGUIUtility.IconContent("RotateTool").image);
            config.enableRotation = EditorGUILayout.Toggle(rotationContent, config.enableRotation);
            var tightPackingContent = new GUIContent(" 剔除透明区域", EditorGUIUtility.IconContent("ViewToolOrbit").image);
            config.tightPacking = EditorGUILayout.Toggle(tightPackingContent, config.tightPacking);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// 绘制高级设置区域。
        /// 配置自动生成、日志、V2格式和排除关键词。
        /// </summary>
        /// <param name="config">配置实例。</param>
        private void DrawAdvancedSettings(AtlasConfiguration config)
        {
            EditorGUILayout.BeginVertical("box");
            var labelGUIContent = new GUIContent(" 高级设置", EditorGUIUtility.IconContent("ToolHandleGlobal").image);
            GUILayout.Label(labelGUIContent, EditorStyles.boldLabel, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            var autoGenerateContent = new GUIContent(" 自动生成", EditorGUIUtility.IconContent("PlayButton").image);
            config.autoGenerate = EditorGUILayout.Toggle(autoGenerateContent, config.autoGenerate);
            var enableLoggingContent =
                new GUIContent(" 启用日志", EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow").image);
            config.enableLogging = EditorGUILayout.Toggle(enableLoggingContent, config.enableLogging);
            var enableV2Content = new GUIContent(" 启用V2打包", EditorGUIUtility.IconContent("CollabNew").image);
            config.enableV2 = EditorGUILayout.Toggle(enableV2Content, config.enableV2);
            EditorGUILayout.BeginHorizontal();
            _showExcludeKeywords = EditorGUILayout.BeginFoldoutHeaderGroup(_showExcludeKeywords, "排除关键词");
            if (_showExcludeKeywords)
            {
                GUILayout.Label("数量:", GUILayout.ExpandWidth(false));
                int newSize = EditorGUILayout.IntField(config.excludeKeywords.Length, GUILayout.Width(40));
                newSize = Mathf.Max(0, newSize);
                if (newSize != config.excludeKeywords.Length)
                {
                    Array.Resize(ref config.excludeKeywords, newSize);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), GUILayout.Width(25),
                        GUILayout.Height(20)))
                {
                    Array.Resize(ref config.excludeKeywords, config.excludeKeywords.Length + 1);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(25),
                        GUILayout.Height(20)) && config.excludeKeywords.Length > 0)
                {
                    Array.Resize(ref config.excludeKeywords, config.excludeKeywords.Length - 1);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (_showExcludeKeywords)
            {
                EditorGUILayout.BeginVertical("box");
                for (int i = 0; i < config.excludeKeywords.Length; i++)
                {
                    var keywordsContent =
                        new GUIContent($" 关键词 [{i}]", EditorGUIUtility.IconContent("FilterByLabel").image);
                    config.excludeKeywords[i] = EditorGUILayout.TextField(keywordsContent, config.excludeKeywords[i]);
                }

                GUILayout.Space(2);
                if (GUILayout.Button(new GUIContent(" 清空", EditorGUIUtility.IconContent("TreeEditor.Trash").image),
                        GUILayout.Height(25)))
                {
                    config.excludeKeywords = Array.Empty<string>();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// 绘制操作按钮区域。
        /// 包含立即重新生成、重新生成变动图集和清空缓存按钮。
        /// </summary>
        private void DrawActionButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Color originalColor = GUI.color;
                GUI.color = Color.yellow;
                if (GUILayout.Button(new GUIContent(" 立即重新生成", EditorGUIUtility.IconContent("Refresh").image),
                        GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("确认删除", "此操作将会立即删除相关路径下的所有图集资源，并重新生成，确定继续吗？", "删除", "取消"))
                    {
                        EditorSpriteSaveInfo.ForceGenerateAll(true);
                    }
                }

                GUI.color = originalColor;

                if (GUILayout.Button(new GUIContent("重新生成有变动的图集数据", EditorGUIUtility.IconContent("Refresh").image),
                        GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    EditorSpriteSaveInfo.ForceGenerateAll();
                }

                if (GUILayout.Button(new GUIContent(" 清空缓存", EditorGUIUtility.IconContent("TreeEditor.Trash").image),
                        GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    EditorSpriteSaveInfo.ClearCache();
                }
            }
        }

        #endregion
    }

#endif
}