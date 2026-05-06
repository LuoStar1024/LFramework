using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace LFramework
{
    public sealed partial class ResourceComponent
    {
        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, string packageName = "")
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new LFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new LFrameworkException("Load scene callbacks is invalid.");
            }

            LoadScene(sceneAssetName, ResourceConstant.DefaultPriority, loadSceneCallbacks, null, packageName);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks,
            string packageName = "")
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new LFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new LFrameworkException("Load scene callbacks is invalid.");
            }

            LoadScene(sceneAssetName, priority, loadSceneCallbacks, null, packageName);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        public void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData,
            string packageName = "")
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new LFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new LFrameworkException("Load scene callbacks is invalid.");
            }

            LoadScene(sceneAssetName, ResourceConstant.DefaultPriority, loadSceneCallbacks, userData, packageName);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks,
            object userData, string packageName = "")
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new LFrameworkException("Scene asset name is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new LFrameworkException("Load scene callbacks is invalid.");
            }

            float duration = Time.time;
            SceneHandle subScene;
            if (string.IsNullOrEmpty(packageName))
            {
                subScene = YooAssets.LoadSceneAsync(sceneAssetName, LoadSceneMode.Additive, LocalPhysicsMode.None,
                    false,
                    (uint)priority);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                if (package == null)
                {
                    throw new LFrameworkException($"The package does not exist. Package Name :{packageName}");
                }

                subScene = package.LoadSceneAsync(sceneAssetName, LoadSceneMode.Additive, LocalPhysicsMode.None, false,
                    (uint)priority);
            }

            subScene.Completed += handle =>
            {
                duration = Time.time - duration;
                if (handle.Status == EOperationStatus.Succeed && handle.SceneObject.IsValid() &&
                    handle.SceneObject.isLoaded)
                {
                    loadSceneCallbacks.LoadSceneSuccessCallback(sceneAssetName, duration, userData);
                    return;
                }

                loadSceneCallbacks.LoadSceneFailureCallback?.Invoke(sceneAssetName, LoadResourceStatus.AssetError,
                    handle.LastError, userData);
            };

            if (loadSceneCallbacks.LoadSceneUpdateCallback != null)
            {
                InvokeProgress(subScene, loadSceneCallbacks.LoadSceneUpdateCallback, sceneAssetName, userData).Forget();
            }

            _subScenes.Add(sceneAssetName, subScene);
        }

        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new LFrameworkException("Scene asset name is invalid.");
            }

            if (unloadSceneCallbacks == null)
            {
                throw new LFrameworkException("Unload scene callbacks is invalid.");
            }

            UnloadScene(sceneAssetName, unloadSceneCallbacks, null);
        }

        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new LFrameworkException("Scene asset name is invalid.");
            }

            if (unloadSceneCallbacks == null)
            {
                throw new LFrameworkException("Unload scene callbacks is invalid.");
            }

            _subScenes.TryGetValue(sceneAssetName, out SceneHandle subScene);
            if (subScene != null)
            {
                var unloadOperation = subScene.UnloadAsync();
                unloadOperation.Completed += @base =>
                {
                    if (@base.Status == EOperationStatus.Succeed)
                    {
                        _subScenes.Remove(sceneAssetName);
                        unloadSceneCallbacks.UnloadSceneSuccessCallback.Invoke(sceneAssetName, userData);
                        return;
                    }

                    unloadSceneCallbacks.UnloadSceneFailureCallback?.Invoke(sceneAssetName, userData);
                };
                return;
            }

            unloadSceneCallbacks.UnloadSceneFailureCallback?.Invoke(sceneAssetName, userData);
        }

        private async UniTaskVoid InvokeProgress(SceneHandle sceneHandle,
            LoadSceneUpdateCallback loadSceneUpdateCallback, string sceneAssetName, object userData)
        {
            if (sceneHandle == null)
            {
                return;
            }

            while (!sceneHandle.IsDone && sceneHandle.IsValid)
            {
                await UniTask.Yield();

                loadSceneUpdateCallback(sceneAssetName, sceneHandle.Progress, userData);
            }
        }
    }
}