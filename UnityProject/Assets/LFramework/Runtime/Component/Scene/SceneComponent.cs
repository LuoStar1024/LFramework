using System;
using System.Collections.Generic;
using UnityEngine;

namespace LFramework
{
    /// <summary>
    /// 场景组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Scene")]
    public sealed class SceneComponent : MonoBehaviour, ILFrameworkModule, ISceneManager
    {
        private List<string> _loadedSceneAssetNames;
        private List<string> _loadingSceneAssetNames;
        private List<string> _unloadingSceneAssetNames;
        private LoadSceneCallbacks _loadSceneCallbacks;
        private UnloadSceneCallbacks _unloadSceneCallbacks;
        private IResourceManager _resourceManager;

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public int Priority
        {
            get { return 2; }
        }
        
        private void Awake()
        {
            LFrameworkEntry.RegisterModule<ISceneManager>(this);
        }

        private void Start()
        {
            _resourceManager = LFrameworkEntry.GetModule<IResourceManager>();
        }

        public void OnInit()
        {
            _loadedSceneAssetNames = new List<string>();
            _loadingSceneAssetNames = new List<string>();
            _unloadingSceneAssetNames = new List<string>();
            _loadSceneCallbacks = new LoadSceneCallbacks(LoadSceneSuccessCallback, LoadSceneFailureCallback,
                LoadSceneUpdateCallback);
            _unloadSceneCallbacks = new UnloadSceneCallbacks(UnloadSceneSuccessCallback, UnloadSceneFailureCallback);
            _resourceManager = null;
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理场景管理器。
        /// </summary>
        public void Shutdown()
        {
            string[] loadedSceneAssetNames = _loadedSceneAssetNames.ToArray();
            foreach (string loadedSceneAssetName in loadedSceneAssetNames)
            {
                if (SceneIsUnloading(loadedSceneAssetName))
                {
                    continue;
                }

                UnloadScene(loadedSceneAssetName);
            }

            _loadedSceneAssetNames.Clear();
            _loadingSceneAssetNames.Clear();
            _unloadingSceneAssetNames.Clear();
        }

        /// <summary>
        /// 获取场景是否已加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否已加载。</returns>
        public bool SceneIsLoaded(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new LFrameworkException("Scene asset name is invalid.");
            }

            return _loadedSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <returns>已加载场景的资源名称。</returns>
        public string[] GetLoadedSceneAssetNames()
        {
            return _loadedSceneAssetNames.ToArray();
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <param name="results">已加载场景的资源名称。</param>
        public void GetLoadedSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new LFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(_loadedSceneAssetNames);
        }

        /// <summary>
        /// 获取场景是否正在加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在加载。</returns>
        public bool SceneIsLoading(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new LFrameworkException("Scene asset name is invalid.");
            }

            return _loadingSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <returns>正在加载场景的资源名称。</returns>
        public string[] GetLoadingSceneAssetNames()
        {
            return _loadingSceneAssetNames.ToArray();
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <param name="results">正在加载场景的资源名称。</param>
        public void GetLoadingSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new LFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(_loadingSceneAssetNames);
        }

        /// <summary>
        /// 获取场景是否正在卸载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在卸载。</returns>
        public bool SceneIsUnloading(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new LFrameworkException("Scene asset name is invalid.");
            }

            return _unloadingSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <returns>正在卸载场景的资源名称。</returns>
        public string[] GetUnloadingSceneAssetNames()
        {
            return _unloadingSceneAssetNames.ToArray();
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <param name="results">正在卸载场景的资源名称。</param>
        public void GetUnloadingSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new LFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(_unloadingSceneAssetNames);
        }

        /// <summary>
        /// 检查场景资源是否存在。
        /// </summary>
        /// <param name="sceneAssetName">要检查场景资源的名称。</param>
        /// <returns>场景资源是否存在。</returns>
        public bool HasScene(string sceneAssetName)
        {
            return _resourceManager.HasAsset(sceneAssetName) != HasAssetResult.NotExist;
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        public void LoadScene(string sceneAssetName)
        {
            LoadScene(sceneAssetName, ResourceConstant.DefaultPriority, null);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        public void LoadScene(string sceneAssetName, int priority)
        {
            LoadScene(sceneAssetName, priority, null);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadScene(string sceneAssetName, object userData)
        {
            LoadScene(sceneAssetName, ResourceConstant.DefaultPriority, userData);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadScene(string sceneAssetName, int priority, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new LFrameworkException("Scene asset name is invalid.");
            }

            if (_resourceManager == null)
            {
                throw new LFrameworkException("You must set resource manager first.");
            }

            if (SceneIsUnloading(sceneAssetName))
            {
                throw new LFrameworkException(Utility.Text.Format("Scene asset '{0}' is being unloaded.",
                    sceneAssetName));
            }

            if (SceneIsLoading(sceneAssetName))
            {
                throw new LFrameworkException(Utility.Text.Format("Scene asset '{0}' is being loaded.",
                    sceneAssetName));
            }

            if (SceneIsLoaded(sceneAssetName))
            {
                throw new LFrameworkException(Utility.Text.Format("Scene asset '{0}' is already loaded.",
                    sceneAssetName));
            }

            _loadingSceneAssetNames.Add(sceneAssetName);
            _resourceManager.LoadScene(sceneAssetName, priority, _loadSceneCallbacks, userData);
        }

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        public void UnloadScene(string sceneAssetName)
        {
            UnloadScene(sceneAssetName, null);
        }

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene(string sceneAssetName, object userData)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new LFrameworkException("Scene asset name is invalid.");
            }

            if (_resourceManager == null)
            {
                throw new LFrameworkException("You must set resource manager first.");
            }

            if (SceneIsUnloading(sceneAssetName))
            {
                throw new LFrameworkException(Utility.Text.Format("Scene asset '{0}' is being unloaded.",
                    sceneAssetName));
            }

            if (SceneIsLoading(sceneAssetName))
            {
                throw new LFrameworkException(Utility.Text.Format("Scene asset '{0}' is being loaded.",
                    sceneAssetName));
            }

            if (!SceneIsLoaded(sceneAssetName))
            {
                throw new LFrameworkException(Utility.Text.Format("Scene asset '{0}' is not loaded yet.",
                    sceneAssetName));
            }

            _unloadingSceneAssetNames.Add(sceneAssetName);
            _resourceManager.UnloadScene(sceneAssetName, _unloadSceneCallbacks, userData);
        }

        private void LoadSceneSuccessCallback(string sceneAssetName, float duration, object userData)
        {
            _loadingSceneAssetNames.Remove(sceneAssetName);
            _loadedSceneAssetNames.Add(sceneAssetName);
            // if (_loadSceneSuccessEventHandler != null)
            // {
            //     LoadSceneSuccessEventArgs loadSceneSuccessEventArgs =
            //         LoadSceneSuccessEventArgs.Create(sceneAssetName, duration, userData);
            //     _loadSceneSuccessEventHandler(this, loadSceneSuccessEventArgs);
            //     ReferencePool.Release(loadSceneSuccessEventArgs);
            // }
        }

        private void LoadSceneFailureCallback(string sceneAssetName, LoadResourceStatus status, string errorMessage,
            object userData)
        {
            _loadingSceneAssetNames.Remove(sceneAssetName);
            string appendErrorMessage =
                Utility.Text.Format("Load scene failure, scene asset name '{0}', status '{1}', error message '{2}'.",
                    sceneAssetName, status, errorMessage);
            // if (_loadSceneFailureEventHandler != null)
            // {
            //     LoadSceneFailureEventArgs loadSceneFailureEventArgs =
            //         LoadSceneFailureEventArgs.Create(sceneAssetName, appendErrorMessage, userData);
            //     _loadSceneFailureEventHandler(this, loadSceneFailureEventArgs);
            //     ReferencePool.Release(loadSceneFailureEventArgs);
            //     return;
            // }

            throw new LFrameworkException(appendErrorMessage);
        }

        private void LoadSceneUpdateCallback(string sceneAssetName, float progress, object userData)
        {
            // if (_loadSceneUpdateEventHandler != null)
            // {
            //     LoadSceneUpdateEventArgs loadSceneUpdateEventArgs =
            //         LoadSceneUpdateEventArgs.Create(sceneAssetName, progress, userData);
            //     _loadSceneUpdateEventHandler(this, loadSceneUpdateEventArgs);
            //     ReferencePool.Release(loadSceneUpdateEventArgs);
            // }
        }

        private void UnloadSceneSuccessCallback(string sceneAssetName, object userData)
        {
            _unloadingSceneAssetNames.Remove(sceneAssetName);
            _loadedSceneAssetNames.Remove(sceneAssetName);
            // if (_unloadSceneSuccessEventHandler != null)
            // {
            //     UnloadSceneSuccessEventArgs unloadSceneSuccessEventArgs =
            //         UnloadSceneSuccessEventArgs.Create(sceneAssetName, userData);
            //     _unloadSceneSuccessEventHandler(this, unloadSceneSuccessEventArgs);
            //     ReferencePool.Release(unloadSceneSuccessEventArgs);
            // }
        }

        private void UnloadSceneFailureCallback(string sceneAssetName, object userData)
        {
            _unloadingSceneAssetNames.Remove(sceneAssetName);
            // if (_unloadSceneFailureEventHandler != null)
            // {
            //     UnloadSceneFailureEventArgs unloadSceneFailureEventArgs =
            //         UnloadSceneFailureEventArgs.Create(sceneAssetName, userData);
            //     _unloadSceneFailureEventHandler(this, unloadSceneFailureEventArgs);
            //     ReferencePool.Release(unloadSceneFailureEventArgs);
            //     return;
            // }

            throw new LFrameworkException(Utility.Text.Format("Unload scene failure, scene asset name '{0}'.",
                sceneAssetName));
        }
    }
}