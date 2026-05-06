using UnityEngine;

namespace LFramework
{
    /// <summary>
    /// 配置组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("LFramework/Config")]
    public sealed partial class ConfigComponent : MonoBehaviour, ILFrameworkModule, IConfigManager
    {
        [SerializeField] private UpdateConfig updateConfig;

        public UpdateConfig UpdateConfig
        {
            get { return updateConfig; }
        }

        public int Priority
        {
            get { return 0; }
        }

        private void Awake()
        {
            LFrameworkEntry.RegisterModule<IConfigManager>(this);
        }

        public void OnInit()
        {
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理配置管理器。
        /// </summary>
        public void Shutdown()
        {
        }

#if UNITY_EDITOR
        private static ConfigComponent _instance;

        public static ConfigComponent Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UnityEngine.Object.FindObjectOfType<ConfigComponent>();

                    if (_instance != null)
                    {
                        return _instance;
                    }
                }

                return _instance;
            }
        }

        public static UpdateConfig EditorUpdateConfig
        {
            get { return Instance != null ? Instance.updateConfig : null; }
        }
#endif
    }
}