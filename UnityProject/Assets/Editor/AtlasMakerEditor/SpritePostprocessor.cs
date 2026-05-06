using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameEditor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 精灵资源后处理器。
    /// 继承自 AssetPostprocessor，用于在资源导入、删除、移动时自动处理图集相关逻辑。
    /// 主要功能包括：
    /// - 自动将导入的图片设置为 Sprite 类型
    /// - 检查文件名是否包含空格
    /// - 检查是否存在同名资源冲突
    /// - 根据配置自动调整 Mipmap 设置
    /// - 触发图集的自动更新
    /// </summary>
    public class SpritePostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// 需要删除的资源列表。
        /// 用于在发现文件名包含空格或同名冲突时标记待删除的资源。
        /// </summary>
        private static List<string> m_resourcesToDelete = new List<string>();

        /// <summary>
        /// 资源后处理回调。
        /// 当任何资源发生变化（导入、删除、移动）时，Unity 会自动调用此方法。
        /// </summary>
        /// <param name="importedAssets">新导入的资源路径数组。</param>
        /// <param name="deletedAssets">被删除的资源路径数组。</param>
        /// <param name="movedAssets">移动后的资源路径数组。</param>
        /// <param name="movedFromAssetPaths">移动前的资源路径数组。</param>
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            m_resourcesToDelete.Clear();
            var config = AtlasConfiguration.Instance;

            // 如果禁用了自动生成，直接返回
            if (!config.autoGenerate) return;

            try
            {
                ProcessAssetChanges(
                    importedAssets: importedAssets,
                    deletedAssets: deletedAssets,
                    movedAssets: movedAssets,
                    movedFromPaths: movedFromAssetPaths
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"Atlas processing error: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                // 处理需要删除的资源
                bool isDelete = m_resourcesToDelete.Count > 0;
                foreach (var res in m_resourcesToDelete)
                {
                    AssetDatabase.DeleteAsset(res);
                }

                if (isDelete)
                {
                    Debug.LogError(
                        $"<color=red>针对 {config.sourceAtlasRootDir} 路径下资源</color>\n<color=red>移除了空格和同名资源，请检查重新合入相关资源</color>");
                }

                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 处理资源变更。
        /// 分别处理导入、删除和移动的资源。
        /// </summary>
        /// <param name="importedAssets">导入的资源路径数组。</param>
        /// <param name="deletedAssets">删除的资源路径数组。</param>
        /// <param name="movedAssets">移动的资源路径数组。</param>
        /// <param name="movedFromPaths">移动前的资源路径数组。</param>
        private static void ProcessAssetChanges(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromPaths)
        {
            // 处理导入的资源
            ProcessAssets(importedAssets, (path) =>
            {
                EditorSpriteSaveInfo.OnImportSprite(path);
                LogProcessed("[Added]", path);
            });

            // 处理删除的资源
            ProcessAssets(deletedAssets, (path) =>
            {
                EditorSpriteSaveInfo.OnDeleteSprite(path);
                LogProcessed("[Deleted]", path);
            });

            // 处理移动的资源
            ProcessMovedAssets(movedFromPaths, movedAssets);
        }

        /// <summary>
        /// 处理资源数组。
        /// 遍历资源数组，对每个资源执行指定的处理操作。
        /// </summary>
        /// <param name="assets">资源路径数组。</param>
        /// <param name="processor">处理操作委托。</param>
        /// <param name="isDelete">是否为删除操作。</param>
        private static void ProcessAssets(string[] assets, Action<string> processor, bool isDelete = false)
        {
            if (assets == null) return;

            foreach (var asset in assets)
            {
                if (ShouldProcessAsset(asset))
                {
                    // 检查文件名是否包含空格、是否存在同名资源、是否需要修改导入设置
                    if (!isDelete && (CheckFileNameContainsSpace(asset) || CheckDuplicateAssetName(asset) ||
                                      ChangeSpriteTextureType(asset)))
                    {
                        continue;
                    }

                    processor?.Invoke(asset);
                }
            }
        }

        /// <summary>
        /// 修改纹理导入设置。
        /// 将纹理类型设置为 Sprite，并根据配置调整 Mipmap 设置。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <returns>是否进行了修改。</returns>
        private static bool ChangeSpriteTextureType(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                return false;
            }

            bool isChange = false;

            // 设置为 Sprite 类型
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                isChange = true;
            }

            // 根据配置调整 Mipmap 设置
            if (AtlasConfiguration.Instance.checkMipmaps)
            {
                if (AtlasConfiguration.Instance.enableMipmaps && !importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = true;
                    isChange = true;
                }
                else if (!AtlasConfiguration.Instance.enableMipmaps && importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    isChange = true;
                }
            }

            // 保存并重新导入
            if (isChange)
            {
                LogProcessed("[Sprite Import Changed Reimport]", path);
                importer.SaveAndReimport();
            }

            return isChange;
        }

        /// <summary>
        /// 检查文件名是否包含空格。
        /// 如果发现文件名包含空格，将该资源标记为待删除。
        /// </summary>
        /// <param name="assetPath">资源路径。</param>
        /// <returns>是否包含空格。</returns>
        private static bool CheckFileNameContainsSpace(string assetPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(assetPath);

            if (fileName.Contains(" "))
            {
                m_resourcesToDelete.Add(assetPath);
                Debug.LogError($"<color=red>发现资源名存在空格: {assetPath}</color>");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否存在同名资源冲突。
        /// 在同一目录树下，如果存在同名但不同路径的资源，将当前资源标记为待删除。
        /// </summary>
        /// <param name="assetPath">资源路径。</param>
        /// <returns>是否存在同名冲突。</returns>
        private static bool CheckDuplicateAssetName(string assetPath)
        {
            var currentFileName = Path.GetFileNameWithoutExtension(assetPath);

            string rootDir = "";
            var tempRootDirArr = new List<string>(AtlasConfiguration.Instance.sourceAtlasRootDir);
            tempRootDirArr.AddRange(AtlasConfiguration.Instance.rootChildAtlasDir);

            // 查找资源所属的根目录
            foreach (var rootPath in tempRootDirArr)
            {
                var tempPath = rootPath.Replace("\\", "/");
                if (!assetPath.StartsWith(tempPath))
                {
                    continue;
                }

                rootDir = tempPath;
            }

            if (string.IsNullOrEmpty(rootDir))
            {
                return false;
            }

            // 获取当前目录下所有图片文件
            var filesInDirectory = Directory.GetFiles(rootDir, "*.*", SearchOption.AllDirectories)
                .Where(CheckIsValidImageFile)
                .ToArray();
            var normalizedCurrentPath = Path.GetFullPath(assetPath).Replace("\\", "/");

            foreach (var file in filesInDirectory)
            {
                var normalizedFile = Path.GetFullPath(file).Replace("\\", "/");
                if (normalizedFile.Equals(normalizedCurrentPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue; // 跳过自身
                }

                var otherFileName = Path.GetFileNameWithoutExtension(file);
                if (string.Equals(currentFileName, otherFileName, StringComparison.OrdinalIgnoreCase))
                {
                    m_resourcesToDelete.Add(assetPath);
                    Debug.LogError($"<color=red>发现同名资源冲突: 合入资源: {assetPath} 存在资源: {file}</color>");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 检查是否为有效的图片文件。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <returns>是否为有效图片文件。</returns>
        private static bool CheckIsValidImageFile(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext.Equals(".png") || ext.Equals(".jpg") || ext.Equals(".jpeg");
        }

        /// <summary>
        /// 处理移动的资源。
        /// 对于移动操作，需要先删除旧路径的精灵，再导入新路径的精灵。
        /// </summary>
        /// <param name="oldPaths">旧路径数组。</param>
        /// <param name="newPaths">新路径数组。</param>
        private static void ProcessMovedAssets(string[] oldPaths, string[] newPaths)
        {
            if (oldPaths == null || newPaths == null) return;

            for (int i = 0; i < oldPaths.Length; i++)
            {
                // 处理旧路径（删除）
                if (ShouldProcessAsset(oldPaths[i]))
                {
                    EditorSpriteSaveInfo.OnDeleteSprite(oldPaths[i]);
                    LogProcessed("[Moved From]", oldPaths[i]);
                    EditorSpriteSaveInfo.MarkParentAtlasesDirty(oldPaths[i], true);
                }

                // 处理新路径（导入）
                if (ShouldProcessAsset(newPaths[i]))
                {
                    if (CheckFileNameContainsSpace(newPaths[i]) || CheckDuplicateAssetName(newPaths[i]) ||
                        ChangeSpriteTextureType(newPaths[i]))
                    {
                        continue;
                    }

                    EditorSpriteSaveInfo.OnImportSprite(newPaths[i]);
                    LogProcessed("[Moved To]", newPaths[i]);
                    EditorSpriteSaveInfo.MarkParentAtlasesDirty(newPaths[i], false);
                }
            }
        }

        /// <summary>
        /// 判断资源是否应该被处理。
        /// 检查资源是否在配置的源目录下、是否被排除、是否为有效图片文件、是否包含排除关键词等。
        /// </summary>
        /// <param name="assetPath">资源路径。</param>
        /// <returns>是否应该处理。</returns>
        private static bool ShouldProcessAsset(string assetPath)
        {
            var config = AtlasConfiguration.Instance;

            if (string.IsNullOrEmpty(assetPath)) return false;
            if (assetPath.StartsWith("Packages/")) return false;

            if (!CheckIsShowProcessPath(assetPath)) return false;
            if (CheckIsExcludeFolder(assetPath)) return false;

            if (!IsValidImageFile(assetPath)) return false;

            // 检查是否包含排除关键词
            foreach (var keyword in config.excludeKeywords)
            {
                if (assetPath.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 检查资源是否在配置的源目录下。
        /// </summary>
        /// <param name="assetPath">资源路径。</param>
        /// <returns>是否在源目录下。</returns>
        private static bool CheckIsShowProcessPath(string assetPath)
        {
            var tempRootDirArr = new List<string>(AtlasConfiguration.Instance.sourceAtlasRootDir);
            tempRootDirArr.AddRange(AtlasConfiguration.Instance.rootChildAtlasDir);
            foreach (var rootPath in tempRootDirArr)
            {
                var tempPath = rootPath.Replace("\\", "/").TrimEnd('/');
                if (!assetPath.StartsWith(tempPath + "/"))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查资源是否在排除目录中。
        /// </summary>
        /// <param name="assetPath">资源路径。</param>
        /// <returns>是否在排除目录中。</returns>
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

        /// <summary>
        /// 判断是否为有效的图片文件。
        /// 支持的格式：png、jpg、jpeg。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <returns>是否为有效图片文件。</returns>
        private static bool IsValidImageFile(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext switch
            {
                ".png" => true,
                ".jpg" => true,
                ".jpeg" => true,
                _ => false
            };
        }

        /// <summary>
        /// 输出处理日志。
        /// 仅在配置中启用日志时输出。
        /// </summary>
        /// <param name="operation">操作类型。</param>
        /// <param name="path">资源路径。</param>
        private static void LogProcessed(string operation, string path)
        {
            if (AtlasConfiguration.Instance.enableLogging)
            {
                Debug.Log($"{operation} {Path.GetFileName(path)}\nPath: {path}");
            }
        }
    }
}