using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LFramework
{
    /// <summary>
    /// 根节点。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RootComponent : MonoBehaviour
    {
        private static RootComponent _instance = null;

        public static RootComponent Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RootComponent>();
                }

                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;

            Application.lowMemory += OnLowMemory;
        }

        private void Update()
        {
            // 驱动整个框架。
            LFrameworkEntry.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnApplicationQuit()
        {
            Application.lowMemory -= OnLowMemory;
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            LFrameworkEntry.Shutdown();
        }

        private void OnLowMemory()
        {
            Log.Info("Low memory reported...");

            IObjectPoolManager objectPoolManager = LFrameworkEntry.GetModule<IObjectPoolManager>();
            if (objectPoolManager != null)
            {
                objectPoolManager.ReleaseAllUnused();
            }

            IResourceManager resourceManager = LFrameworkEntry.GetModule<IResourceManager>();
            if (resourceManager != null)
            {
                resourceManager.ForceUnloadUnusedAssets(true);
            }
        }

        /// <summary>
        /// 关闭游戏框架。
        /// </summary>
        public void Shutdown(ShutdownType shutdownType)
        {
            Log.Info("Shutdown LFramework ({0})...", shutdownType);

            Destroy(gameObject);

            if (shutdownType == ShutdownType.None)
            {
                return;
            }

            if (shutdownType == ShutdownType.Restart)
            {
                SceneManager.LoadScene(0);
                return;
            }

            if (shutdownType == ShutdownType.Quit)
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }
        }
    }
}