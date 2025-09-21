using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace LFramework
{
    /// <summary>
    /// 资源管理器接口。
    /// </summary>
    public interface IResourceManager
    {
        /// <summary>
        /// 获取资源模式。
        /// </summary>
        ResourceMode ResourceMode
        {
            get;
        }

        /// <summary>
        /// 是否边玩边下载。
        /// </summary>
        bool UpdatableWhilePlaying
        {
            get;
        }
        
        /// <summary>
        /// WebGL平台加载本地资源/加载远程资源。
        /// </summary>
        LoadResourceWayWebGL LoadResourceWayWebGL
        {
            get;
        }
        
        /// <summary>
        /// 资源加密方式。
        /// </summary>
        EncryptionType EncryptionType
        {
            get;
        }
        
        /// <summary>
        /// 获取或设置资源更新下载地址。
        /// </summary>
        string UpdatePrefixUrl
        {
            get;
        }
        
        /// <summary>
        /// 获取或设置资源更新下载地址。
        /// </summary>
        string FallbackUpdatePrefixUrl
        {
            get;
        }
        
        /// <summary>
        /// 获取当前资源适用的游戏版本号。
        /// </summary>
        string ApplicableGameVersion
        {
            get;
        }

        /// <summary>
        /// 获取当前内部资源版本号。
        /// </summary>
        int InternalResourceVersion
        {
            get;
        }

        /// <summary>
        /// 获取正在应用的资源包路径。
        /// </summary>
        string DefaultPackageName
        {
            get;
        }

        /// <summary>
        /// 获取或设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）。
        /// </summary>
        long Milliseconds
        {
            get;
        }

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        float AssetAutoReleaseInterval
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        int AssetCapacity
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        float AssetExpireTime
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置资源对象池的优先级。
        /// </summary>
        int AssetPriority
        {
            get;
            set;
        }
        
        /// <summary>
        /// 获取或设置无用资源释放的最小间隔时间，以秒为单位。
        /// </summary>
        float MinUnloadUnusedAssetsInterval
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置无用资源释放的最大间隔时间，以秒为单位。
        /// </summary>
        float MaxUnloadUnusedAssetsInterval
        {
            get;
            set;
        }

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        void SetObjectPoolManager(IObjectPoolManager objectPoolManager);

        /// <summary>
        /// 初始化资源。
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 初始化操作。
        /// </summary>
        /// <param name="packageName">资源包名称。</param>
        UniTask<InitializationOperation> InitPackage(string packageName);

        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="assetName">要检查资源的名称。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <returns>检查资源是否存在的结果。</returns>
        HasAssetResult HasAsset(string assetName, string packageName = "");

        /// <summary>
        /// 检查资源定位地址是否有效。
        /// </summary>
        /// <param name="assetName">资源的定位地址</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        bool CheckAssetValid(string assetName, string packageName = "");

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData, string packageName = "");

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData, string packageName = "");

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="callback">回调函数。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        UniTaskVoid LoadAsset<T>(string assetName, Action<T> callback, string packageName = "") where T : UnityEngine.Object;
        
        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>异步资源实例。</returns>
        UniTask<T> LoadAsset<T>(string assetName, int priority, CancellationToken cancellationToken = default, string packageName = "") where T : UnityEngine.Object;

        /// <summary>
        /// 加载已有资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        T LoadExistAsset<T>(string assetName, string packageName = null) where T : UnityEngine.Object;
        
        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        void UnloadAsset(object asset);
        
        /// <summary>
        /// 资源回收（卸载引用计数为零的资源）
        /// </summary>
        void UnloadUnusedAssets();

        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        void ForceUnloadAllAssets();

        /// <summary>
        /// 强制执行释放未被使用的资源。
        /// </summary>
        /// <param name="performGCCollect">是否使用垃圾回收。</param>
        void ForceUnloadUnusedAssets(bool performGCCollect);

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, string packageName = "");
        
        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, string packageName = "");
        
        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData, string packageName = "");
        
        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData, string packageName = "");
        
        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks);
        
        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData);
        //
        // /// <summary>
        // /// 异步加载二进制资源。
        // /// </summary>
        // /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        // /// <param name="loadBinaryCallbacks">加载二进制资源回调函数集。</param>
        // /// <param name="packageName">资源包名称。</param>
        // void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, string packageName = "");
        //
        // /// <summary>
        // /// 异步加载二进制资源。
        // /// </summary>
        // /// <param name="binaryAssetName">要加载二进制资源的名称。</param>
        // /// <param name="loadBinaryCallbacks">加载二进制资源回调函数集。</param>
        // /// <param name="userData">用户自定义数据。</param>
        // /// <param name="packageName">资源包名称。</param>
        // void LoadBinary(string binaryAssetName, LoadBinaryCallbacks loadBinaryCallbacks, object userData, string packageName = "");
    }
}