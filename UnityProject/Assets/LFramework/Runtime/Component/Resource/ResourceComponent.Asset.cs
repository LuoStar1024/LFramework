using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace LFramework
{
    public sealed partial class ResourceComponent
    {
        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        public async void LoadAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData,
            string packageName = "")
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new LFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new LFrameworkException("Load asset callbacks is invalid.");
            }
            
            if (!CheckAssetValid(assetName, packageName))
            {
                string errorMessage = Utility.Text.Format("Could not found assetName [{0}].", assetName);
                Log.Error(errorMessage);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.NotExist, errorMessage, userData);
                }
                return;
            }

            string assetObjectKey = GetCacheKey(assetName, packageName);

            await TryWaitingLoading(assetObjectKey);

            float duration = Time.time;

            AssetObject assetObject = _assetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                loadAssetCallbacks.LoadAssetSuccessCallback(assetName, assetObject.Target, Time.time - duration, userData);
                return;
            }

            _assetLoadingHashSet.Add(assetObjectKey);

            AssetInfo assetInfo = GetAssetInfo(assetName, packageName);

            if (!string.IsNullOrEmpty(assetInfo.Error))
            {
                _assetLoadingHashSet.Remove(assetObjectKey);

                string errorMessage = Utility.Text.Format("Can not load asset '{0}' because :'{1}'.", assetName, assetInfo.Error);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.NotExist, errorMessage, userData);
                    return;
                }

                throw new LFrameworkException(errorMessage);
            }

            AssetHandle handle = GetAssetHandle(assetName, assetInfo.AssetType, packageName: packageName);

            if (loadAssetCallbacks.LoadAssetUpdateCallback != null)
            {
                InvokeProgress(assetName, handle, loadAssetCallbacks.LoadAssetUpdateCallback, userData).Forget();
            }

            await handle.ToUniTask();

            if (handle.AssetObject == null || handle.Status == EOperationStatus.Failed)
            {
                _assetLoadingHashSet.Remove(assetObjectKey);

                string errorMessage = Utility.Text.Format("Can not load asset '{0}'.", assetName);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.NotReady, errorMessage, userData);
                    return;
                }

                throw new LFrameworkException(errorMessage);
            }
            else
            {
                assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle);
                _assetPool.Register(assetObject, true);

                _assetLoadingHashSet.Remove(assetObjectKey);

                if (loadAssetCallbacks.LoadAssetSuccessCallback != null)
                {
                    duration = Time.time - duration;

                    loadAssetCallbacks.LoadAssetSuccessCallback(assetName, handle.AssetObject, duration, userData);
                }
            }
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        public async void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks,
            object userData, string packageName = "")
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new LFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new LFrameworkException("Load asset callbacks is invalid.");
            }
            
            if (!CheckAssetValid(assetName, packageName))
            {
                string errorMessage = Utility.Text.Format("Could not found assetName [{0}].", assetName);
                Log.Error(errorMessage);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.NotExist, errorMessage, userData);
                }
                return;
            }

            string assetObjectKey = GetCacheKey(assetName, packageName);

            await TryWaitingLoading(assetObjectKey);

            float duration = Time.time;

            AssetObject assetObject = _assetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                loadAssetCallbacks.LoadAssetSuccessCallback(assetName, assetObject.Target, Time.time - duration, userData);
                return;
            }

            _assetLoadingHashSet.Add(assetObjectKey);

            AssetInfo assetInfo = GetAssetInfo(assetName, packageName);

            if (!string.IsNullOrEmpty(assetInfo.Error))
            {
                _assetLoadingHashSet.Remove(assetObjectKey);

                string errorMessage = Utility.Text.Format("Can not load asset '{0}' because :'{1}'.", assetName, assetInfo.Error);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.NotExist, errorMessage, userData);
                    return;
                }

                throw new LFrameworkException(errorMessage);
            }

            AssetHandle handle = GetAssetHandle(assetName, assetType, packageName: packageName);

            if (loadAssetCallbacks.LoadAssetUpdateCallback != null)
            {
                InvokeProgress(assetName, handle, loadAssetCallbacks.LoadAssetUpdateCallback, userData).Forget();
            }

            await handle.ToUniTask();

            if (handle.AssetObject == null || handle.Status == EOperationStatus.Failed)
            {
                _assetLoadingHashSet.Remove(assetObjectKey);

                string errorMessage = Utility.Text.Format("Can not load asset '{0}'.", assetName);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.NotReady, errorMessage, userData);
                    return;
                }

                throw new LFrameworkException(errorMessage);
            }
            else
            {
                assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle);
                _assetPool.Register(assetObject, true);

                _assetLoadingHashSet.Remove(assetObjectKey);

                if (loadAssetCallbacks.LoadAssetSuccessCallback != null)
                {
                    duration = Time.time - duration;

                    loadAssetCallbacks.LoadAssetSuccessCallback(assetName, handle.AssetObject, duration, userData);
                }
            }
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="callback">回调函数。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        public async UniTaskVoid LoadAsset<T>(string assetName, Action<T> callback, string packageName = "")
            where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(assetName))
            {
                throw new LFrameworkException("Asset name is invalid.");
            }
            
            if (!CheckAssetValid(assetName, packageName))
            {
                Log.Error($"Could not found assetName [{assetName}].");
                callback?.Invoke(null);
                return;
            }

            string assetObjectKey = GetCacheKey(assetName, packageName);

            await TryWaitingLoading(assetObjectKey);

            AssetObject assetObject = _assetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                callback?.Invoke(assetObject.Target as T);
                return;
            }

            _assetLoadingHashSet.Add(assetObjectKey);

            AssetHandle handle = GetAssetHandle<T>(assetName, packageName: packageName);

            handle.Completed += assetHandle =>
            {
                _assetLoadingHashSet.Remove(assetObjectKey);

                if (assetHandle.AssetObject != null)
                {
                    assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle);
                    _assetPool.Register(assetObject, true);

                    callback?.Invoke(assetObject.Target as T);
                }
                else
                {
                    callback?.Invoke(null);
                }
            };
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>异步资源实例。</returns>
        public async UniTask<T> LoadAsset<T>(string assetName, int priority, CancellationToken cancellationToken = default,
            string packageName = "") where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new LFrameworkException("Asset name is invalid.");
            }
            
            if (!CheckAssetValid(assetName, packageName))
            {
                Log.Error($"Could not found assetName [{assetName}].");
                return null;
            }

            string assetObjectKey = GetCacheKey(assetName, packageName);

            await TryWaitingLoading(assetObjectKey);

            AssetObject assetObject = _assetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                return assetObject.Target as T;
            }

            _assetLoadingHashSet.Add(assetObjectKey);

            AssetHandle handle = GetAssetHandle<T>(assetName, packageName: packageName);
            bool cancelOrFailed = await handle.ToUniTask(cancellationToken: cancellationToken).AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();

            if (cancelOrFailed)
            {
                _assetLoadingHashSet.Remove(assetObjectKey);
                return null;
            }

            assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle);
            _assetPool.Register(assetObject, true);

            _assetLoadingHashSet.Remove(assetObjectKey);

            return handle.AssetObject as T;
        }

        /// <summary>
        /// 加载已有资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadExistAsset<T>(string assetName, string packageName = null) where T : UnityEngine.Object
        {
            string assetObjectKey = GetCacheKey(assetName, packageName);
            AssetObject assetObject = _assetPool.Spawn(assetObjectKey);
            if (assetObject == null)
            {
                return null;
            }

            return assetObject.Target as T;
        }

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        public void UnloadAsset(object asset)
        {
            if (_assetPool != null)
            {
                _assetPool.Unspawn(asset);
            }
        }

        /// <summary>
        /// 资源回收（卸载引用计数为零的资源）
        /// </summary>
        public void UnloadUnusedAssets()
        {
            _assetPool.ReleaseAllUnused();
            foreach (var package in _packageDict.Values)
            {
                if (package is { InitializeStatus: EOperationStatus.Succeed })
                {
                    package.UnloadUnusedAssetsAsync();
                }
            }
        }

        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        public void ForceUnloadAllAssets()
        {
#if UNITY_WEBGL
            Log.Warning($"WebGL not support invoke {nameof(ForceUnloadAllAssets)}");
			return;
#else

            foreach (var package in _packageDict.Values)
            {
                if (package is { InitializeStatus: EOperationStatus.Succeed })
                {
                    package.UnloadAllAssetsAsync();
                }
            }
#endif
        }

        /// <summary>
        /// 强制执行释放未被使用的资源。
        /// </summary>
        /// <param name="performGCCollect">是否使用垃圾回收。</param>
        public void ForceUnloadUnusedAssets(bool performGCCollect)
        {
            _forceUnloadUnusedAssets = true;
            if (performGCCollect)
            {
                _performGCCollect = true;
            }
        }
        
        
        private readonly TimeoutController _timeoutController = new TimeoutController();
        
        private async UniTask TryWaitingLoading(string assetObjectKey)
        {
            if (_assetLoadingHashSet.Contains(assetObjectKey))
            {
                try
                {
                    await UniTask.WaitUntil(() => !_assetLoadingHashSet.Contains(assetObjectKey))
#if UNITY_EDITOR
                        .AttachExternalCancellation(_timeoutController.Timeout(TimeSpan.FromSeconds(60)));
                    _timeoutController.Reset();
#else
                    ;
#endif
                }
                catch (OperationCanceledException ex)
                {
                    if (_timeoutController.IsTimeout())
                    {
                        Log.Error($"LoadAssetAsync Waiting {assetObjectKey} timeout. reason:{ex.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取异步资源句柄。
        /// </summary>
        /// <param name="assetName">资源定位地址。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <returns>资源句柄。</returns>
        private AssetHandle GetAssetHandle<T>(string assetName, string packageName = "") where T : UnityEngine.Object
        {
            return GetAssetHandle(assetName, typeof(T), packageName);
        }

        private AssetHandle GetAssetHandle(string assetName, Type assetType, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.LoadAssetAsync(assetName, assetType);
            }

            var package = YooAssets.GetPackage(packageName);
            return package.LoadAssetAsync(assetName, assetType);
        }
        
        private async UniTaskVoid InvokeProgress(string assetName, AssetHandle assetHandle, LoadAssetUpdateCallback loadAssetUpdateCallback, object userData)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new LFrameworkException("Asset name is invalid.");
            }

            if (loadAssetUpdateCallback != null)
            {
                while (assetHandle is { IsValid: true, IsDone: false })
                {
                    await UniTask.Yield();

                    loadAssetUpdateCallback.Invoke(assetName, assetHandle.Progress, userData);
                }
            }
        }
    }
}