namespace GameEditor
{
#if UNITY_EDITOR
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// 图集配置类。
    /// 使用 EditorScriptableSingleton 实现单例模式，配置数据保存在 ProjectSettings 目录下。
    /// 提供图集生成的所有配置选项，包括目录设置、平台格式、打包参数等。
    /// </summary>
    [FilePath("ProjectSettings/AtlasConfiguration.asset")]
    public class AtlasConfiguration : EditorScriptableSingleton<AtlasConfiguration>
    {
        #region 目录设置

        /// <summary>
        /// 生成的图集输出目录。
        /// 所有自动生成的 SpriteAtlas 文件将保存到此目录。
        /// </summary>
        [Header("目录设置")] [Tooltip("生成的图集输出目录")]
        public string outputAtlasDir = "Assets/GameResArt/Atlas";

        /// <summary>
        /// 需要生成图集的UI根目录数组。
        /// 系统会扫描这些目录下的所有图片资源，并按目录结构自动生成对应的图集。
        /// </summary>
        [Tooltip("需要生成图集的UI根目录")] public string[] sourceAtlasRootDir = new string[] { "Assets/GameResRaw/Sprite" };

        /// <summary>
        /// 以当前目录的子级生成子级图集的目录数组。
        /// 这些目录下的每个子文件夹会生成一个独立的图集，而不是按完整路径生成。
        /// </summary>
        [Tooltip("以当前目录的子级生成子级图集")] public string[] rootChildAtlasDir = new string[] { };

        /// <summary>
        /// 每张图都单独生成图集的目录数组。
        /// 这些目录下的每张图片都会生成一个独立的图集文件。
        /// 适用于大图或需要单独管理的资源。
        /// </summary>
        [Tooltip("每张图都单独生成图集")] public string[] singleAtlasDir = new string[] { };

        /// <summary>
        /// 不需要生成图集的UI目录数组。
        /// 这些目录下的图片资源将被排除，不会被打入任何图集。
        /// </summary>
        [Tooltip("不需要生成图集的UI目录")] public string[] excludeFolder = new string[] { };

        #endregion

        #region 平台格式设置

        /// <summary>
        /// Android 平台的纹理压缩格式。
        /// 默认使用 ASTC_6x6，在质量和压缩率之间取得平衡。
        /// </summary>
        [Header("平台格式设置")] public TextureImporterFormat androidFormat = TextureImporterFormat.ASTC_6x6;

        /// <summary>
        /// iOS 平台的纹理压缩格式。
        /// 默认使用 ASTC_5x5，iOS 设备对 ASTC 格式有良好支持。
        /// </summary>
        public TextureImporterFormat iosFormat = TextureImporterFormat.ASTC_5x5;

        /// <summary>
        /// WebGL 平台的纹理压缩格式。
        /// 默认使用 ASTC_6x6。
        /// </summary>
        public TextureImporterFormat webglFormat = TextureImporterFormat.ASTC_6x6;

        #endregion

        #region 打包设置

        /// <summary>
        /// 图集中精灵之间的间距（像素）。
        /// 用于防止精灵边缘出现渗色问题。
        /// </summary>
        [Header("PackingSetting")] public int padding = 2;

        /// <summary>
        /// 是否允许旋转精灵以获得更好的打包效率。
        /// 启用后可能会提高图集空间利用率。
        /// </summary>
        public bool enableRotation = true;

        /// <summary>
        /// 块偏移量，用于图集打包算法。
        /// </summary>
        public int blockOffset = 1;

        /// <summary>
        /// 是否启用紧密打包（剔除透明区域）。
        /// 启用后会根据精灵的实际像素边界进行打包，而不是矩形边界。
        /// </summary>
        public bool tightPacking = true;

        #endregion

        #region 其他设置

        /// <summary>
        /// 纹理压缩质量（0-100）。
        /// 值越高质量越好，但文件体积也越大。
        /// </summary>
        [Header("其他设置")] [Range(0, 100)] public int compressionQuality = 50;

        /// <summary>
        /// 是否自动生成图集。
        /// 启用后，当图片资源发生变化时会自动更新对应的图集。
        /// </summary>
        public bool autoGenerate = true;

        /// <summary>
        /// 是否启用日志输出。
        /// 启用后会在控制台输出图集生成的详细信息。
        /// </summary>
        public bool enableLogging = true;

        /// <summary>
        /// 是否启用 V2 版本的 SpriteAtlas 格式（.spriteatlasv2）。
        /// V2 格式在 Unity 2020.1+ 中提供更好的性能和功能。
        /// </summary>
        public bool enableV2 = true;

        #endregion

        #region Sprite导入设置

        /// <summary>
        /// 是否检查 Mipmap 导入设置。
        /// 启用后会在导入精灵时检查并修正 Mipmap 设置。
        /// </summary>
        [Header("Sprite导入设置")] public bool checkMipmaps = true;

        /// <summary>
        /// 是否为精灵启用 Mipmap。
        /// UI 精灵通常不需要 Mipmap，禁用可以减少内存占用。
        /// </summary>
        public bool enableMipmaps = false;

        #endregion

        #region 排除关键词

        /// <summary>
        /// 排除关键词数组。
        /// 文件路径中包含这些关键词的资源将被排除，不会被打入图集。
        /// 常用于排除临时文件或待删除的资源。
        /// </summary>
        [Header("排除关键词")] public string[] excludeKeywords = { "_Delete", "_Temp" };

        #endregion
    }

#endif
}