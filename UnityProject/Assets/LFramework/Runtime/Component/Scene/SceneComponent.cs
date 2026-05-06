using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LFramework
{
    /// <summary>
    /// 场景组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("LFramework/Scene")]
    public sealed class SceneComponent : MonoBehaviour, ILFrameworkModule, ISceneManager
    {
        private List<string> _loadedSceneAssetNames;
        private List<string> _loadingSceneAssetNames;
        private List<string> _unloadingSceneAssetNames;
        private LoadSceneCallbacks _loadSceneCallbacks;
        private UnloadSceneCallbacks _unloadSceneCallbacks;
        private IResourceManager _resourceManager;


        private readonly SortedDictionary<string, int> _sceneOrder =
            new SortedDictionary<string, int>(StringComparer.Ordinal);

        private Camera _mainCamera = null;
        private Scene _frameworkScene = default(Scene);

        /// <summary>
        /// 获取当前场景主摄像机。
        /// </summary>
        public Camera MainCamera
        {
            get { return _mainCamera; }
        }

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

            _frameworkScene = SceneManager.GetSceneAt(0);
            if (!_frameworkScene.IsValid())
            {
                Log.Fatal("Game Framework scene is invalid.");
                return;
            }
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
        /// 获取场景名称。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景名称。</returns>
        public static string GetSceneName(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return null;
            }

            int sceneNamePosition = sceneAssetName.LastIndexOf('/');
            if (sceneNamePosition + 1 >= sceneAssetName.Length)
            {
                Log.Error("Scene asset name '{0}' is invalid.", sceneAssetName);
                return null;
            }

            string sceneName = sceneAssetName.Substring(sceneNamePosition + 1);
            sceneNamePosition = sceneName.LastIndexOf(".unity");
            if (sceneNamePosition > 0)
            {
                sceneName = sceneName.Substring(0, sceneNamePosition);
            }

            return sceneName;
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
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return false;
            }

            if (_resourceManager == null)
            {
                throw new LFrameworkException("You must set resource manager first.");
            }

            return _resourceManager.HasAsset(sceneAssetName) != HasAssetResult.NotExist;
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="progressCallback">加载进度回调。</param>
        /// <param name="loadSuccessCallBack">加载完成回调。</param>
        public void LoadScene(string sceneAssetName, Action<float> progressCallback = null,
            Action<bool> loadSuccessCallBack = null)
        {
            LoadScene(sceneAssetName, ResourceConstant.DefaultPriority, null, progressCallback, loadSuccessCallBack);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="progressCallback">加载进度回调。</param>
        /// <param name="loadSuccessCallBack">加载完成回调。</param>
        public void LoadScene(string sceneAssetName, int priority, Action<float> progressCallback = null,
            Action<bool> loadSuccessCallBack = null)
        {
            LoadScene(sceneAssetName, priority, null, progressCallback, loadSuccessCallBack);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="progressCallback">加载进度回调。</param>
        /// <param name="loadSuccessCallBack">加载完成回调。</param>
        public void LoadScene(string sceneAssetName, object userData, Action<float> progressCallback = null,
            Action<bool> loadSuccessCallBack = null)
        {
            LoadScene(sceneAssetName, ResourceConstant.DefaultPriority, userData, progressCallback,
                loadSuccessCallBack);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="progressCallback">加载进度回调。</param>
        /// <param name="loadSuccessCallBack">加载完成回调。</param>
        public void LoadScene(string sceneAssetName, int priority, object userData,
            Action<float> progressCallback = null,
            Action<bool> loadSuccessCallBack = null)
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
            _resourceManager.LoadScene(sceneAssetName, priority, _loadSceneCallbacks,
                LoadSceneInfo.Create(userData, progressCallback, loadSuccessCallBack));
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

        /// <summary>
        /// 设置场景顺序。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="sceneOrder">要设置的场景顺序。</param>
        public void SetSceneOrder(string sceneAssetName, int sceneOrder)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return;
            }

            if (!sceneAssetName.StartsWith("Assets/", StringComparison.Ordinal) ||
                !sceneAssetName.EndsWith(".unity", StringComparison.Ordinal))
            {
                Log.Error("Scene asset name '{0}' is invalid.", sceneAssetName);
                return;
            }

            if (SceneIsLoading(sceneAssetName))
            {
                _sceneOrder[sceneAssetName] = sceneOrder;
                return;
            }

            if (SceneIsLoaded(sceneAssetName))
            {
                _sceneOrder[sceneAssetName] = sceneOrder;
                RefreshSceneOrder();
                return;
            }

            Log.Error("Scene '{0}' is not loaded or loading.", sceneAssetName);
        }

        /// <summary>
        /// 刷新当前场景主摄像机。
        /// </summary>
        public void RefreshMainCamera()
        {
            _mainCamera = Camera.main;
        }

        private void RefreshSceneOrder()
        {
            if (_sceneOrder.Count > 0)
            {
                string maxSceneName = null;
                int maxSceneOrder = 0;
                foreach (KeyValuePair<string, int> sceneOrder in _sceneOrder)
                {
                    if (SceneIsLoading(sceneOrder.Key))
                    {
                        continue;
                    }

                    if (maxSceneName == null)
                    {
                        maxSceneName = sceneOrder.Key;
                        maxSceneOrder = sceneOrder.Value;
                        continue;
                    }

                    if (sceneOrder.Value > maxSceneOrder)
                    {
                        maxSceneName = sceneOrder.Key;
                        maxSceneOrder = sceneOrder.Value;
                    }
                }

                if (maxSceneName == null)
                {
                    SetActiveScene(_frameworkScene);
                    return;
                }

                Scene scene = SceneManager.GetSceneByName(GetSceneName(maxSceneName));
                if (!scene.IsValid())
                {
                    Log.Error("Active scene '{0}' is invalid.", maxSceneName);
                    return;
                }

                SetActiveScene(scene);
            }
            else
            {
                SetActiveScene(_frameworkScene);
            }
        }

        private void SetActiveScene(Scene activeScene)
        {
            Scene lastActiveScene = SceneManager.GetActiveScene();
            if (lastActiveScene != activeScene)
            {
                SceneManager.SetActiveScene(activeScene);
            }

            RefreshMainCamera();
        }

        private void LoadSceneSuccessCallback(string sceneAssetName, float duration, object userData)
        {
            _loadingSceneAssetNames.Remove(sceneAssetName);
            _loadedSceneAssetNames.Add(sceneAssetName);

            if (!_sceneOrder.ContainsKey(sceneAssetName))
            {
                _sceneOrder.Add(sceneAssetName, 0);
            }

            RefreshSceneOrder();

            LoadSceneInfo loadSceneInfo = (LoadSceneInfo)userData;
            if (loadSceneInfo == null)
            {
                throw new LFrameworkException("Load scene info is invalid.");
            }

            loadSceneInfo.LoadSuccessCallBack?.Invoke(true);
            ReferencePool.Release(loadSceneInfo);
        }

        private void LoadSceneFailureCallback(string sceneAssetName, LoadResourceStatus status, string errorMessage,
            object userData)
        {
            _loadingSceneAssetNames.Remove(sceneAssetName);
            string appendErrorMessage =
                Utility.Text.Format("Load scene failure, scene asset name '{0}', status '{1}', error message '{2}'.",
                    sceneAssetName, status, errorMessage);
            Log.Error(appendErrorMessage);

            LoadSceneInfo loadSceneInfo = (LoadSceneInfo)userData;
            if (loadSceneInfo == null)
            {
                throw new LFrameworkException("Load scene info is invalid.");
            }

            loadSceneInfo.LoadSuccessCallBack?.Invoke(false);
            ReferencePool.Release(loadSceneInfo);
        }

        private void LoadSceneUpdateCallback(string sceneAssetName, float progress, object userData)
        {
            LoadSceneInfo loadSceneInfo = (LoadSceneInfo)userData;
            if (loadSceneInfo == null)
            {
                throw new LFrameworkException("Load scene info is invalid.");
            }

            loadSceneInfo.ProgressCallback?.Invoke(progress);
        }

        private void UnloadSceneSuccessCallback(string sceneAssetName, object userData)
        {
            _unloadingSceneAssetNames.Remove(sceneAssetName);
            _loadedSceneAssetNames.Remove(sceneAssetName);

            _sceneOrder.Remove(sceneAssetName);
            RefreshSceneOrder();
        }

        private void UnloadSceneFailureCallback(string sceneAssetName, object userData)
        {
            _unloadingSceneAssetNames.Remove(sceneAssetName);

            Log.Error(Utility.Text.Format("Unload scene failure, scene asset name '{0}'.",
                sceneAssetName));
        }
    }
}