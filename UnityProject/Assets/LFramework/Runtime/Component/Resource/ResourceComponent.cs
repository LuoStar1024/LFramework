using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace LFramework
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("LFramework/Resource")]
    public sealed partial class ResourceComponent : MonoBehaviour, ILFrameworkModule, IResourceManager
    {
        [SerializeField] private bool isEditorSimulate = true;

        [SerializeField] private ResourceMode resourceMode;

        [SerializeField] private bool updatableWhilePlaying;

        [SerializeField] private LoadResourceWayWebGL loadResourceWayWebGL;

        [SerializeField] private EncryptionType encryptionType = EncryptionType.None;

        [SerializeField] private string defaultPackageName = "DefaultPackage";

        [SerializeField] private int downloadingMaxNum = 10;

        [SerializeField] private int failedTryAgain = 3;

        [SerializeField] private long milliseconds = 30;

        [SerializeField] private float assetAutoReleaseInterval = 60f;

        [SerializeField] private int assetCapacity = 64;

        [SerializeField] private float assetExpireTime = 60f;

        [SerializeField] private int assetPriority = 0;

        [SerializeField] private float minUnloadUnusedAssetsInterval = 60f;

        [SerializeField] private float maxUnloadUnusedAssetsInterval = 300f;

        [SerializeField] private bool useSystemUnloadUnusedAssets = true;

        private string _updatePrefixUrl;
        private string _fallbackUpdatePrefixUrl;
        private string _applicableGameVersion;
        private int _internalResourceVersion;

        private float _lastUnloadUnusedAssetsOperationElapseSeconds = 0f;
        private AsyncOperation _asyncOperation = null;
        private bool _forceUnloadUnusedAssets = false;
        private bool _preorderUnloadUnusedAssets = false;
        private bool _performGCCollect = false;

        private string _packageVersion;
        private ResourceDownloaderOperation _downloader;

        /// <summary>
        /// 资源包列表。
        /// </summary>
        private Dictionary<string, ResourcePackage> _packageDict = null;

        /// <summary>
        /// 资源信息列表。
        /// </summary>
        private Dictionary<string, AssetInfo> _assetInfoDict = null;

        /// <summary>
        /// 正在加载的资源列表。
        /// </summary>
        private HashSet<string> _assetLoadingHashSet = null;


        private Dictionary<string, SceneHandle> _subScenes = null;

        /// <summary>
        /// 获取资源模式。
        /// </summary>
        public ResourceMode ResourceMode
        {
            get
            {
#if UNITY_EDITOR
                if (isEditorSimulate)
                {
                    return ResourceMode.EditorSimulate;
                }
#endif
                return resourceMode;
            }
        }

#if UNITY_EDITOR
        private static ResourceComponent _instance;

        public static ResourceComponent Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UnityEngine.Object.FindObjectOfType<ResourceComponent>();

                    if (_instance != null)
                    {
                        return _instance;
                    }
                }

                return _instance;
            }
        }

        public static ResourceMode EditorResourceMode
        {
            get { return Instance != null ? Instance.resourceMode : ResourceMode.Unspecified; }
        }
#endif

        /// <summary>
        /// 是否边玩边下载。
        /// </summary>
        public bool UpdatableWhilePlaying
        {
            get { return updatableWhilePlaying; }
        }

        /// <summary>
        /// WebGL平台加载本地资源/加载远程资源。
        /// </summary>
        public LoadResourceWayWebGL LoadResourceWayWebGL
        {
            get { return loadResourceWayWebGL; }
        }

        /// <summary>
        /// 资源加密方式。
        /// </summary>
        public EncryptionType EncryptionType
        {
            get { return encryptionType; }
        }

        public string UpdatePrefixUrl
        {
            get { return _updatePrefixUrl; }
        }

        /// <summary>
        /// 获取或设置资源更新下载地址。
        /// </summary>
        public string FallbackUpdatePrefixUrl
        {
            get { return _fallbackUpdatePrefixUrl; }
        }

        public string ApplicableGameVersion
        {
            get { return _applicableGameVersion; }
        }

        public int InternalResourceVersion
        {
            get { return _internalResourceVersion; }
        }

        public string DefaultPackageName
        {
            get { return defaultPackageName; }
        }

        public long Milliseconds
        {
            get { return milliseconds; }
        }

        /// <summary>
        /// 获取或设置无用资源释放的最小间隔时间，以秒为单位。
        /// </summary>
        public float MinUnloadUnusedAssetsInterval
        {
            get => minUnloadUnusedAssetsInterval;
            set => minUnloadUnusedAssetsInterval = value;
        }

        /// <summary>
        /// 获取或设置无用资源释放的最大间隔时间，以秒为单位。
        /// </summary>
        public float MaxUnloadUnusedAssetsInterval
        {
            get => maxUnloadUnusedAssetsInterval;
            set => maxUnloadUnusedAssetsInterval = value;
        }

        /// <summary>
        /// 当前最新的包裹版本。
        /// </summary>
        public string PackageVersion
        {
            get => _packageVersion;
            set => _packageVersion = value;
        }

        /// <summary>
        /// 资源下载器，用于下载当前资源版本所有的资源包文件。
        /// </summary>
        public ResourceDownloaderOperation Downloader
        {
            get => _downloader;
            set => _downloader = value;
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public int Priority
        {
            get { return 3; }
        }

        private void Awake()
        {
            LFrameworkEntry.RegisterModule<IResourceManager>(this);
        }

        private void Start()
        {
            var configManager = LFrameworkEntry.GetModule<IConfigManager>();
            _updatePrefixUrl = configManager.UpdateConfig.GetResDownLoadPath();
            _fallbackUpdatePrefixUrl = configManager.UpdateConfig.GetFallbackResDownLoadPath();

            Initialize();
        }

        public void OnInit()
        {
            _packageDict = new Dictionary<string, ResourcePackage>();
            _assetInfoDict = new Dictionary<string, AssetInfo>();
            _assetLoadingHashSet = new HashSet<string>();
            _subScenes = new Dictionary<string, SceneHandle>();
        }

        /// <summary>
        /// 定时器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            _lastUnloadUnusedAssetsOperationElapseSeconds += Time.unscaledDeltaTime;
            if (_asyncOperation == null && (_forceUnloadUnusedAssets ||
                                            _lastUnloadUnusedAssetsOperationElapseSeconds >=
                                            maxUnloadUnusedAssetsInterval ||
                                            _preorderUnloadUnusedAssets &&
                                            _lastUnloadUnusedAssetsOperationElapseSeconds >=
                                            minUnloadUnusedAssetsInterval))
            {
                Log.Info("Unload unused assets...");
                _forceUnloadUnusedAssets = false;
                _preorderUnloadUnusedAssets = false;
                _lastUnloadUnusedAssetsOperationElapseSeconds = 0f;
                _asyncOperation = Resources.UnloadUnusedAssets();
                if (useSystemUnloadUnusedAssets)
                {
                    UnloadUnusedAssets();
                }
            }

            if (_asyncOperation is { isDone: true })
            {
                _asyncOperation = null;
                if (_performGCCollect)
                {
                    Log.Info("GC.Collect...");
                    _performGCCollect = false;
                    GC.Collect();
                }
            }
        }

        /// <summary>
        /// 关闭并清理定时器。
        /// </summary>
        public void Shutdown()
        {
        }

        public void Initialize()
        {
            // 初始化资源系统
            YooAssets.Initialize(new ResourceLogger());
            YooAssets.SetOperationSystemMaxTimeSlice(Milliseconds);

            // 创建默认的资源包
            string packageName = DefaultPackageName;
            var defaultPackage = YooAssets.TryGetPackage(packageName);
            if (defaultPackage == null)
            {
                defaultPackage = YooAssets.CreatePackage(packageName);
            }

            YooAssets.SetDefaultPackage(defaultPackage);

            IObjectPoolManager objectPoolManager = LFrameworkEntry.GetModule<IObjectPoolManager>();
            SetObjectPoolManager(objectPoolManager);
        }

        public async UniTask<InitializationOperation> InitPackage(string packageName)
        {
            EPlayMode playMode = GetCurPlayMode();

            if (_packageDict.TryGetValue(packageName, out var resourcePackage))
            {
                if (resourcePackage.InitializeStatus is EOperationStatus.Processing or EOperationStatus.Succeed)
                {
                    Log.Error($"ResourceSystem has already init package : {packageName}");
                    return null;
                }
                else
                {
                    _packageDict.Remove(packageName);
                }
            }

            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
            {
                package = YooAssets.CreatePackage(packageName);
            }

            _packageDict[packageName] = package;

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            IDecryptionServices decryptionServices = CreateDecryptionServices();

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters(decryptionServices);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = UpdatePrefixUrl;
                string fallbackHostServer = FallbackUpdatePrefixUrl;
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters(decryptionServices);
                createParameters.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, decryptionServices);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
                var createParameters = new WebPlayModeParameters();
                IWebDecryptionServices webDecryptionServices = CreateWebDecryptionServices();
                string defaultHostServer = UpdatePrefixUrl;
                string fallbackHostServer = FallbackUpdatePrefixUrl;
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
                Log.Info("=======================WEIXINMINIGAME=======================");
                // 注意：如果有子目录，请修改此处！
                string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE";
                createParameters.WebServerFileSystemParameters =
 WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices, webDecryptionServices);
#else
                Log.Info("=======================UNITY_WEBGL=======================");
                if (LoadResourceWayWebGL == LoadResourceWayWebGL.Remote)
                {
                    createParameters.WebRemoteFileSystemParameters =
                        FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices,
                            webDecryptionServices);
                }

                createParameters.WebServerFileSystemParameters =
                    FileSystemParameters.CreateDefaultWebServerFileSystemParameters(webDecryptionServices);
#endif
                initializationOperation = package.InitializeAsync(createParameters);
            }

            await initializationOperation.ToUniTask();

            Log.Info($"Init resource package version : {initializationOperation?.Status}");

            return initializationOperation;
        }

        /// <summary>
        /// 获取当前资源包版本。
        /// </summary>
        /// <param name="customPackageName">指定资源包的名称。不传使用默认资源包</param>
        /// <returns>资源包版本。</returns>
        public string GetPackageVersion(string customPackageName = "")
        {
            var package = string.IsNullOrEmpty(customPackageName)
                ? YooAssets.GetPackage(DefaultPackageName)
                : YooAssets.GetPackage(customPackageName);
            if (package == null)
            {
                return string.Empty;
            }

            return package.GetPackageVersion();
        }

        /// <summary>
        /// 异步更新最新包的版本。
        /// </summary>
        /// <param name="appendTimeTicks">请求URL是否需要带时间戳。</param>
        /// <param name="timeout">超时时间。</param>
        /// <param name="customPackageName">指定资源包的名称。不传使用默认资源包</param>
        /// <returns>请求远端包裹的最新版本操作句柄。</returns>
        public RequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks = false, int timeout = 60,
            string customPackageName = "")
        {
            var package = string.IsNullOrEmpty(customPackageName)
                ? YooAssets.GetPackage(DefaultPackageName)
                : YooAssets.GetPackage(customPackageName);
            return package.RequestPackageVersionAsync(appendTimeTicks, timeout);
        }

        /// <summary>
        /// 向网络端请求并更新清单
        /// </summary>
        /// <param name="packageVersion">更新的包裹版本</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        /// <param name="customPackageName">指定资源包的名称。不传使用默认资源包</param>
        public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60,
            string customPackageName = "")
        {
            var package = string.IsNullOrEmpty(customPackageName)
                ? YooAssets.GetPackage(this.DefaultPackageName)
                : YooAssets.GetPackage(customPackageName);
            return package.UpdatePackageManifestAsync(packageVersion, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载当前资源版本所有的资源包文件。
        /// </summary>
        /// <param name="customPackageName">指定资源包的名称。不传使用默认资源包</param>
        public ResourceDownloaderOperation CreateResourceDownloader(string customPackageName = "")
        {
            ResourcePackage package = null;
            if (string.IsNullOrEmpty(customPackageName))
            {
                package = YooAssets.GetPackage(this.DefaultPackageName);
            }
            else
            {
                package = YooAssets.GetPackage(customPackageName);
            }

            Downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            return Downloader;
        }

        /// <summary>
        /// 清理包裹未使用的缓存文件。
        /// </summary>
        /// <param name="clearMode">文件清理方式。</param>
        /// <param name="customPackageName">指定资源包的名称。不传使用默认资源包</param>
        public ClearCacheFilesOperation ClearCacheFilesAsync(
            EFileClearMode clearMode = EFileClearMode.ClearUnusedBundleFiles,
            string customPackageName = "")
        {
            var package = string.IsNullOrEmpty(customPackageName)
                ? YooAssets.GetPackage(DefaultPackageName)
                : YooAssets.GetPackage(customPackageName);
            return package.ClearCacheFilesAsync(clearMode);
        }

        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="assetName">要检查资源的名称。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        public HasAssetResult HasAsset(string assetName, string packageName = "")
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new LFrameworkException("Asset name is invalid.");
            }

            AssetInfo assetInfo = GetAssetInfo(assetName, packageName);

            if (!CheckAssetValid(assetName, packageName))
            {
                return HasAssetResult.Valid;
            }

            if (assetInfo == null)
            {
                return HasAssetResult.NotExist;
            }

            if (IsNeedDownloadFromRemote(assetInfo, packageName))
            {
                return HasAssetResult.AssetOnline;
            }

            return HasAssetResult.AssetOnDisk;
        }

        /// <summary>
        /// 检查资源定位地址是否有效。
        /// </summary>
        /// <param name="assetName">资源的定位地址</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        public bool CheckAssetValid(string assetName, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.CheckLocationValid(assetName);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.CheckLocationValid(assetName);
            }
        }

        private EPlayMode GetCurPlayMode()
        {
#if UNITY_EDITOR
            if (isEditorSimulate)
            {
                return EPlayMode.EditorSimulateMode;
            }
#endif
            switch (resourceMode)
            {
                case ResourceMode.Package:
                    return EPlayMode.OfflinePlayMode;
                case ResourceMode.Updatable:
                    return EPlayMode.HostPlayMode;
                case ResourceMode.WebPlayMode:
                    return EPlayMode.WebPlayMode;
                default:
                    return EPlayMode.OfflinePlayMode;
            }
        }

        /// <summary>
        /// 创建解密服务。
        /// </summary>
        private IDecryptionServices CreateDecryptionServices()
        {
            return EncryptionType switch
            {
                EncryptionType.FileOffSet => new FileOffsetDecryption(),
                EncryptionType.FileStream => new FileStreamDecryption(),
                _ => null
            };
        }

        /// <summary>
        /// 创建Web解密服务。
        /// </summary>
        private IWebDecryptionServices CreateWebDecryptionServices()
        {
            return EncryptionType switch
            {
                EncryptionType.FileOffSet => new FileOffsetWebDecryption(),
                EncryptionType.FileStream => new FileStreamWebDecryption(),
                _ => null
            };
        }

        #region 获取资源信息

        /// <summary>
        /// 是否需要从远端更新下载。
        /// </summary>
        /// <param name="assetName">资源的定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        private bool IsNeedDownloadFromRemote(string assetName, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.IsNeedDownloadFromRemote(assetName);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.IsNeedDownloadFromRemote(assetName);
            }
        }

        /// <summary>
        /// 是否需要从远端更新下载。
        /// </summary>
        /// <param name="assetInfo">资源信息。</param>
        /// <param name="packageName">资源包名称。</param>
        private bool IsNeedDownloadFromRemote(AssetInfo assetInfo, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.IsNeedDownloadFromRemote(assetInfo);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.IsNeedDownloadFromRemote(assetInfo);
            }
        }

        /// <summary>
        /// 获取资源信息列表。
        /// </summary>
        /// <param name="resTag">资源标签。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源信息列表。</returns>
        private AssetInfo[] GetAssetInfos(string resTag, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.GetAssetInfos(resTag);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.GetAssetInfos(resTag);
            }
        }

        /// <summary>
        /// 获取资源信息列表。
        /// </summary>
        /// <param name="resTags">资源标签列表。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源信息列表。</returns>
        private AssetInfo[] GetAssetInfos(string[] resTags, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.GetAssetInfos(resTags);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.GetAssetInfos(resTags);
            }
        }

        /// <summary>
        /// 获取资源信息。
        /// </summary>
        /// <param name="assetName">资源的定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源信息。</returns>
        private AssetInfo GetAssetInfo(string assetName, string packageName = "")
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new LFrameworkException("Asset name is invalid.");
            }

            if (string.IsNullOrEmpty(packageName))
            {
                if (_assetInfoDict.TryGetValue(assetName, out AssetInfo assetInfo))
                {
                    return assetInfo;
                }

                assetInfo = YooAssets.GetAssetInfo(assetName);
                _assetInfoDict[assetName] = assetInfo;
                return assetInfo;
            }
            else
            {
                string key = $"{packageName}/{assetName}";
                if (_assetInfoDict.TryGetValue(key, out AssetInfo assetInfo))
                {
                    return assetInfo;
                }

                var package = YooAssets.GetPackage(packageName);
                if (package == null)
                {
                    throw new LFrameworkException($"The package does not exist. Package Name :{packageName}");
                }

                assetInfo = package.GetAssetInfo(assetName);
                _assetInfoDict[key] = assetInfo;
                return assetInfo;
            }
        }

        /// <summary>
        /// 获取资源定位地址的缓存Key。
        /// </summary>
        /// <param name="assetName">资源定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源定位地址的缓存Key。</returns>
        private string GetCacheKey(string assetName, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName) || packageName.Equals(DefaultPackageName))
            {
                return assetName;
            }

            return $"{packageName}/{assetName}";
        }

        #endregion

        public List<string> GetLoadingAssetInfo()
        {
            List<string> result = new List<string>(_assetLoadingHashSet.Count);

            foreach (var assetInfo in _assetLoadingHashSet)
            {
                result.Add(assetInfo);
            }

            return result;
        }
    }
}
