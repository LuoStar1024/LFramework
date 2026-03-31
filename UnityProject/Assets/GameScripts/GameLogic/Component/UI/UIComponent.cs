using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LFramework;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// UI组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("LFramework/UI")]
    public sealed partial class UIComponent : MonoBehaviour, ILFrameworkModule, IUIManager, IUIRelease
    {
        [SerializeField]
        private float instanceAutoReleaseInterval = 60f;

        [SerializeField]
        private int instanceCapacity = 16;

        [SerializeField]
        private float instanceExpireTime = 60f;

        [SerializeField]
        private int instancePriority = 0;
        
        private const int DefaultPriority = 0;
        private readonly List<IUIForm> _internalUIFormResults = new List<IUIForm>();
        
        private Dictionary<string, UIGroup> _uiGroups;
        private Dictionary<int, string> _uiFormsBeingLoaded;
        private HashSet<int> _uiFormsToReleaseOnLoad;
        private Queue<IUIForm> _recycleQueue;
        private LoadAssetCallbacks _loadAssetCallbacks;
        private IObjectPoolManager _objectPoolManager;
        private IResourceManager _resourceManager;
        private IObjectPool<UIFormInstanceObject> _instancePool;
        private int _serial;
        private bool _isShutdown;

        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        public int UIGroupCount
        {
            get { return _uiGroups.Count; }
        }

        /// <summary>
        /// 获取或设置界面实例对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float InstanceAutoReleaseInterval
        {
            get { return _instancePool.AutoReleaseInterval; }
            set { _instancePool.AutoReleaseInterval = instanceAutoReleaseInterval = value; }
        }

        /// <summary>
        /// 获取或设置界面实例对象池的容量。
        /// </summary>
        public int InstanceCapacity
        {
            get { return _instancePool.Capacity; }
            set { _instancePool.Capacity = instanceCapacity = value; }
        }

        /// <summary>
        /// 获取或设置界面实例对象池对象过期秒数。
        /// </summary>
        public float InstanceExpireTime
        {
            get { return _instancePool.ExpireTime; }
            set { _instancePool.ExpireTime = instanceExpireTime = value; }
        }

        /// <summary>
        /// 获取或设置界面实例对象池的优先级。
        /// </summary>
        public int InstancePriority
        {
            get { return _instancePool.Priority; }
            set { _instancePool.Priority = instancePriority = value; }
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public int Priority
        {
            get
            {
                return 0;
            }
        }

        private void Awake()
        {
            LFrameworkEntry.RegisterModule<IUIManager>(this);
        }

        private void Start()
        {
            var res = LFrameworkEntry.GetModule<IResourceManager>();
            SetResourceManager(res);

            var pool = LFrameworkEntry.GetModule<IObjectPoolManager>();
            SetObjectPoolManager(pool);
            InstanceAutoReleaseInterval = instanceAutoReleaseInterval;
            InstanceCapacity = instanceCapacity;
            InstanceExpireTime = instanceExpireTime;
            InstancePriority = instancePriority;

            gameObject.layer = LayerMask.NameToLayer("UI");
        }

        public void OnInit()
        {
            _uiGroups = new Dictionary<string, UIGroup>(StringComparer.Ordinal);
            _uiFormsBeingLoaded = new Dictionary<int, string>();
            _uiFormsToReleaseOnLoad = new HashSet<int>();
            _recycleQueue = new Queue<IUIForm>();
            _loadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback, LoadAssetUpdateCallback);
            _objectPoolManager = null;
            _resourceManager = null;
            _instancePool = null;
            _serial = 0;
            _isShutdown = false;
        }

        /// <summary>
        /// 界面管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            while (_recycleQueue.Count > 0)
            {
                IUIForm uiForm = _recycleQueue.Dequeue();
                uiForm.OnRecycle();
                _instancePool.Unspawn(uiForm.Handle);
            }

            foreach (KeyValuePair<string, UIGroup> uiGroup in _uiGroups)
            {
                uiGroup.Value.OnUpdate(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理界面管理器。
        /// </summary>
        public void Shutdown()
        {
            _isShutdown = true;
            CloseAllLoadedUIForms();
            // 提前释放资源，防止与ResourceManager的对象池资源冲突
            while (_recycleQueue.Count > 0)
            {
                IUIForm uiForm = _recycleQueue.Dequeue();
                uiForm.OnRecycle();
                _instancePool.Unspawn(uiForm.Handle);
            }
            _instancePool.ReleaseAllUnused();
            _uiGroups.Clear();
            _uiFormsBeingLoaded.Clear();
            _uiFormsToReleaseOnLoad.Clear();
            _recycleQueue.Clear();
        }

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
        {
            if (objectPoolManager == null)
            {
                throw new LFrameworkException("Object pool manager is invalid.");
            }

            _objectPoolManager = objectPoolManager;
            _instancePool = _objectPoolManager.CreateSingleSpawnObjectPool<UIFormInstanceObject>("UI Instance Pool");
        }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        public void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new LFrameworkException("Resource manager is invalid.");
            }

            _resourceManager = resourceManager;
        }

        /// <summary>
        /// 是否存在界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否存在界面组。</returns>
        public bool HasUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new LFrameworkException("UI group name is invalid.");
            }

            return _uiGroups.ContainsKey(uiGroupName);
        }

        /// <summary>
        /// 获取界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面组。</returns>
        public IUIGroup GetUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new LFrameworkException("UI group name is invalid.");
            }

            UIGroup uiGroup = null;
            if (_uiGroups.TryGetValue(uiGroupName, out uiGroup))
            {
                return uiGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <returns>所有界面组。</returns>
        public IUIGroup[] GetAllUIGroups()
        {
            int index = 0;
            IUIGroup[] results = new IUIGroup[_uiGroups.Count];
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uiGroups)
            {
                results[index++] = uiGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <param name="results">所有界面组。</param>
        public void GetAllUIGroups(List<IUIGroup> results)
        {
            if (results == null)
            {
                throw new LFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uiGroups)
            {
                results.Add(uiGroup.Value);
            }
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName)
        {
            return AddUIGroup(uiGroupName, 0);
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, int uiGroupDepth)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new LFrameworkException("UI group name is invalid.");
            }

            if (HasUIGroup(uiGroupName))
            {
                return false;
            }

            // 创建实例化
            var uiGroup = new GameObject().AddComponent<UIGroup>();
            uiGroup.name = Utility.Text.Format("UI Group - {0}", uiGroupName);
            uiGroup.gameObject.layer = LayerMask.NameToLayer("UI");
            uiGroup.Depth = uiGroupDepth;
            Transform trans = uiGroup.transform;
            trans.SetParent(gameObject.transform);
            trans.localScale = Vector3.one;

            _uiGroups.Add(uiGroupName, uiGroup);

            return true;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uiGroups)
            {
                if (uiGroup.Value.HasUIForm(serialId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new LFrameworkException("UI form asset name is invalid.");
            }

            foreach (KeyValuePair<string, UIGroup> uiGroup in _uiGroups)
            {
                if (uiGroup.Value.HasUIForm(uiFormAssetName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm GetUIForm(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uiGroups)
            {
                IUIForm uiForm = uiGroup.Value.GetUIForm(serialId);
                if (uiForm != null)
                {
                    return (UIForm)uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm GetUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new LFrameworkException("UI form asset name is invalid.");
            }

            foreach (KeyValuePair<string, UIGroup> uiGroup in _uiGroups)
            {
                IUIForm uiForm = uiGroup.Value.GetUIForm(uiFormAssetName);
                if (uiForm != null)
                {
                    return (UIForm)uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm[] GetUIForms(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new LFrameworkException("UI form asset name is invalid.");
            }

            List<IUIForm> results = new List<IUIForm>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uiGroups)
            {
                results.AddRange(uiGroup.Value.GetUIForms(uiFormAssetName));
            }

            UIForm[] uiFormImpls = new UIForm[results.Count];
            for (int i = 0; i < results.Count; i++)
            {
                uiFormImpls[i] = (UIForm)results[i];
            }

            return uiFormImpls;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="results">要获取的界面。</param>
        public void GetUIForms(string uiFormAssetName, List<UIForm> results)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new LFrameworkException("UI form asset name is invalid.");
            }

            if (results == null)
            {
                throw new LFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uiGroups)
            {
                uiGroup.Value.InternalGetUIForms(uiFormAssetName, _internalUIFormResults);
            }
            
            foreach (IUIForm uiForm in _internalUIFormResults)
            {
                results.Add((UIForm)uiForm);
            }
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <returns>所有已加载的界面。</returns>
        public UIForm[] GetAllLoadedUIForms()
        {
            List<IUIForm> uiForms = new List<IUIForm>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uiGroups)
            {
                uiForms.AddRange(uiGroup.Value.GetAllUIForms());
            }

            UIForm[] uiFormImpls = new UIForm[uiForms.Count];
            for (int i = 0; i < uiForms.Count; i++)
            {
                uiFormImpls[i] = (UIForm)uiForms[i];
            }

            return uiFormImpls;
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <param name="results">所有已加载的界面。</param>
        public void GetAllLoadedUIForms(List<UIForm> results)
        {
            if (results == null)
            {
                throw new LFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uiGroups)
            {
                uiGroup.Value.InternalGetAllUIForms(_internalUIFormResults);
            }
            
            foreach (IUIForm uiForm in _internalUIFormResults)
            {
                results.Add((UIForm)uiForm);
            }
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <returns>所有正在加载界面的序列编号。</returns>
        public int[] GetAllLoadingUIFormSerialIds()
        {
            int index = 0;
            int[] results = new int[_uiFormsBeingLoaded.Count];
            foreach (KeyValuePair<int, string> uiFormBeingLoaded in _uiFormsBeingLoaded)
            {
                results[index++] = uiFormBeingLoaded.Key;
            }

            return results;
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载界面的序列编号。</param>
        public void GetAllLoadingUIFormSerialIds(List<int> results)
        {
            if (results == null)
            {
                throw new LFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, string> uiFormBeingLoaded in _uiFormsBeingLoaded)
            {
                results.Add(uiFormBeingLoaded.Key);
            }
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(int serialId)
        {
            return _uiFormsBeingLoaded.ContainsKey(serialId);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new LFrameworkException("UI form asset name is invalid.");
            }

            return _uiFormsBeingLoaded.ContainsValue(uiFormAssetName);
        }

        /// <summary>
        /// 是否是合法的界面。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <returns>界面是否合法。</returns>
        public bool IsValidUIForm(UIForm uiForm)
        {
            if (uiForm == null)
            {
                return false;
            }

            return HasUIForm(uiForm.SerialId);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, 100, false, null);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, priority, false, null);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, 100, pauseCoveredUIForm, null);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, object userData)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, 100, false, userData);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, null);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, object userData)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, priority, false, userData);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData)
        {
            return OpenUIForm(uiFormAssetName, uiGroupName, 100, pauseCoveredUIForm, userData);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm,
            object userData)
        {
            if (_resourceManager == null)
            {
                throw new LFrameworkException("You must set resource manager first.");
            }

            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new LFrameworkException("UI form asset name is invalid.");
            }

            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new LFrameworkException("UI group name is invalid.");
            }

            UIGroup uiGroup = (UIGroup)GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                throw new LFrameworkException(Utility.Text.Format("UI group '{0}' is not exist.", uiGroupName));
            }

            int serialId = ++_serial;
            UIFormInstanceObject uiFormInstanceObject = _instancePool.Spawn(uiFormAssetName);
            if (uiFormInstanceObject == null)
            {
                _uiFormsBeingLoaded.Add(serialId, uiFormAssetName);
                _resourceManager.LoadAsset(uiFormAssetName, priority, _loadAssetCallbacks,
                    OpenUIFormInfo.Create(serialId, uiGroup, pauseCoveredUIForm, userData));
            }
            else
            {
                InternalOpenUIForm(serialId, uiFormAssetName, uiGroup, uiFormInstanceObject.Target, pauseCoveredUIForm,
                    false, 0f, userData);
            }

            return serialId;
        }

        /// <summary>
        /// 打开界面。（可等待）
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public async UniTask<int> OpenUIFormAwait(string uiFormAssetName, string uiGroupName, int priority,
            bool pauseCoveredUIForm, object userData)
        {
            if (_resourceManager == null)
            {
                throw new LFrameworkException("You must set resource manager first.");
            }

            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new LFrameworkException("UI form asset name is invalid.");
            }

            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new LFrameworkException("UI group name is invalid.");
            }

            UIGroup uiGroup = (UIGroup)GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                throw new LFrameworkException(Utility.Text.Format("UI group '{0}' is not exist.", uiGroupName));
            }

            int serialId = ++_serial;
            UIFormInstanceObject uiFormInstanceObject = _instancePool.Spawn(uiFormAssetName);
            if (uiFormInstanceObject == null)
            {
                float duration = Time.time;
                _uiFormsBeingLoaded.Add(serialId, uiFormAssetName);
                var uiFormAsset = await _resourceManager.LoadAsset<GameObject>(uiFormAssetName, priority);
                duration = Time.time - duration;
                if (uiFormAsset != null)
                {
                    LoadAssetSuccessCallback(uiFormAssetName, uiFormAsset, duration,
                        OpenUIFormInfo.Create(serialId, uiGroup, pauseCoveredUIForm, userData));
                }
                else
                {
                    LoadAssetFailureCallback(uiFormAssetName, LoadResourceStatus.AssetError, null,
                        OpenUIFormInfo.Create(serialId, uiGroup, pauseCoveredUIForm, userData));
                }
            }
            else
            {
                InternalOpenUIForm(serialId, uiFormAssetName, uiGroup, uiFormInstanceObject.Target, pauseCoveredUIForm,
                    false, 0f, userData);
            }

            return serialId;
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        public void CloseUIForm(int serialId)
        {
            CloseUIForm(serialId, null);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(int serialId, object userData)
        {
            if (IsLoadingUIForm(serialId))
            {
                _uiFormsToReleaseOnLoad.Add(serialId);
                _uiFormsBeingLoaded.Remove(serialId);
                return;
            }

            UIForm uiForm = GetUIForm(serialId);
            if (uiForm == null)
            {
                throw new LFrameworkException(Utility.Text.Format("Can not find UI form '{0}'.", serialId));
            }

            CloseUIForm(uiForm, userData);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        public void CloseUIForm(UIForm uiForm)
        {
            CloseUIForm(uiForm, null);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(UIForm uiForm, object userData)
        {
            if (uiForm == null)
            {
                throw new LFrameworkException("UI form is invalid.");
            }

            UIGroup uiGroup = (UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new LFrameworkException("UI group is invalid.");
            }

            uiGroup.RemoveUIForm(uiForm);
            uiForm.OnClose(_isShutdown, userData);
            uiGroup.Refresh();

            // if (_closeUIFormCompleteEventHandler != null)
            // {
            //     CloseUIFormCompleteEventArgs closeUIFormCompleteEventArgs =
            //         CloseUIFormCompleteEventArgs.Create(uiForm.SerialId, uiForm.UIFormAssetName, uiGroup, userData);
            //     _closeUIFormCompleteEventHandler(this, closeUIFormCompleteEventArgs);
            //     ReferencePool.Release(closeUIFormCompleteEventArgs);
            // }

            _recycleQueue.Enqueue(uiForm);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        public void CloseAllLoadedUIForms()
        {
            CloseAllLoadedUIForms(null);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseAllLoadedUIForms(object userData)
        {
            UIForm[] uiForms = GetAllLoadedUIForms();
            foreach (UIForm uiForm in uiForms)
            {
                if (!HasUIForm(uiForm.SerialId))
                {
                    continue;
                }

                CloseUIForm(uiForm, userData);
            }
        }

        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        public void CloseAllLoadingUIForms()
        {
            foreach (KeyValuePair<int, string> uiFormBeingLoaded in _uiFormsBeingLoaded)
            {
                _uiFormsToReleaseOnLoad.Add(uiFormBeingLoaded.Key);
            }

            _uiFormsBeingLoaded.Clear();
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        public void RefocusUIForm(UIForm uiForm)
        {
            RefocusUIForm(uiForm, null);
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(UIForm uiForm, object userData)
        {
            if (uiForm == null)
            {
                throw new LFrameworkException("UI form is invalid.");
            }

            UIGroup uiGroup = (UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new LFrameworkException("UI group is invalid.");
            }

            uiGroup.RefocusUIForm(uiForm, userData);
            uiGroup.Refresh();
            uiForm.OnRefocus(userData);
        }

        /// <summary>
        /// 设置界面实例是否被加锁。
        /// </summary>
        /// <param name="uiFormInstance">要设置是否被加锁的界面实例。</param>
        /// <param name="locked">界面实例是否被加锁。</param>
        public void SetUIFormInstanceLocked(object uiFormInstance, bool locked)
        {
            if (uiFormInstance == null)
            {
                throw new LFrameworkException("UI form instance is invalid.");
            }

            _instancePool.SetLocked(uiFormInstance, locked);
        }

        /// <summary>
        /// 设置界面实例的优先级。
        /// </summary>
        /// <param name="uiFormInstance">要设置优先级的界面实例。</param>
        /// <param name="priority">界面实例优先级。</param>
        public void SetUIFormInstancePriority(object uiFormInstance, int priority)
        {
            if (uiFormInstance == null)
            {
                throw new LFrameworkException("UI form instance is invalid.");
            }

            _instancePool.SetPriority(uiFormInstance, priority);
        }
        
        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="uiFormAsset">要释放的界面资源。</param>
        /// <param name="uiFormInstance">要释放的界面实例。</param>
        public void ReleaseUIForm(object uiFormAsset, object uiFormInstance)
        {
            _resourceManager.UnloadAsset(uiFormAsset);
            Destroy((UnityEngine.Object)uiFormInstance);
        }

        private void InternalOpenUIForm(int serialId, string uiFormAssetName, UIGroup uiGroup, object uiFormInstance,
            bool pauseCoveredUIForm, bool isNewInstance, float duration, object userData)
        {
            try
            {
                IUIForm uiForm = CreateUIForm(uiFormInstance, uiGroup, userData);
                if (uiForm == null)
                {
                    throw new LFrameworkException("Can not create UI form in UI form helper.");
                }

                uiForm.OnInit(serialId, uiFormAssetName, uiGroup, pauseCoveredUIForm, isNewInstance, userData);
                uiGroup.AddUIForm(uiForm);
                uiForm.OnOpen(userData);
                uiGroup.Refresh();

                // if (_openUIFormSuccessEventHandler != null)
                // {
                //     OpenUIFormSuccessEventArgs openUIFormSuccessEventArgs =
                //         OpenUIFormSuccessEventArgs.Create(uiForm, duration, userData);
                //     _openUIFormSuccessEventHandler(this, openUIFormSuccessEventArgs);
                //     ReferencePool.Release(openUIFormSuccessEventArgs);
                // }
            }
            catch (Exception exception)
            {
                // if (_openUIFormFailureEventHandler != null)
                // {
                //     OpenUIFormFailureEventArgs openUIFormFailureEventArgs = OpenUIFormFailureEventArgs.Create(serialId,
                //         uiFormAssetName, uiGroup.Name, pauseCoveredUIForm, exception.ToString(), userData);
                //     _openUIFormFailureEventHandler(this, openUIFormFailureEventArgs);
                //     ReferencePool.Release(openUIFormFailureEventArgs);
                //     return;
                // }

                throw;
            }
        }

        private void LoadAssetSuccessCallback(string uiFormAssetName, object uiFormAsset, float duration,
            object userData)
        {
            OpenUIFormInfo openUIFormInfo = (OpenUIFormInfo)userData;
            if (openUIFormInfo == null)
            {
                throw new LFrameworkException("Open UI form info is invalid.");
            }

            if (_uiFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                _uiFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                ReferencePool.Release(openUIFormInfo);
                ReleaseUIForm(uiFormAsset, null);
                return;
            }

            _uiFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
            UIFormInstanceObject uiFormInstanceObject = UIFormInstanceObject.Create(uiFormAssetName, uiFormAsset,
                InstantiateUIForm(uiFormAsset), this);
            _instancePool.Register(uiFormInstanceObject, true);

            InternalOpenUIForm(openUIFormInfo.SerialId, uiFormAssetName, openUIFormInfo.UIGroup,
                uiFormInstanceObject.Target, openUIFormInfo.PauseCoveredUIForm, true, duration,
                openUIFormInfo.UserData);
            ReferencePool.Release(openUIFormInfo);
        }

        private void LoadAssetFailureCallback(string uiFormAssetName, LoadResourceStatus status, string errorMessage,
            object userData)
        {
            OpenUIFormInfo openUIFormInfo = (OpenUIFormInfo)userData;
            if (openUIFormInfo == null)
            {
                throw new LFrameworkException("Open UI form info is invalid.");
            }

            if (_uiFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                _uiFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                return;
            }

            _uiFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
            string appendErrorMessage =
                Utility.Text.Format("Load UI form failure, asset name '{0}', status '{1}', error message '{2}'.",
                    uiFormAssetName, status, errorMessage);
            // if (_openUIFormFailureEventHandler != null)
            // {
            //     OpenUIFormFailureEventArgs openUIFormFailureEventArgs =
            //         OpenUIFormFailureEventArgs.Create(openUIFormInfo.SerialId, uiFormAssetName,
            //             openUIFormInfo.UIGroup.Name, openUIFormInfo.PauseCoveredUIForm, appendErrorMessage,
            //             openUIFormInfo.UserData);
            //     _openUIFormFailureEventHandler(this, openUIFormFailureEventArgs);
            //     ReferencePool.Release(openUIFormFailureEventArgs);
            //     return;
            // }

            throw new LFrameworkException(appendErrorMessage);
        }

        private void LoadAssetUpdateCallback(string uiFormAssetName, float progress, object userData)
        {
            OpenUIFormInfo openUIFormInfo = (OpenUIFormInfo)userData;
            if (openUIFormInfo == null)
            {
                throw new LFrameworkException("Open UI form info is invalid.");
            }

            // if (_openUIFormUpdateEventHandler != null)
            // {
            //     OpenUIFormUpdateEventArgs openUIFormUpdateEventArgs =
            //         OpenUIFormUpdateEventArgs.Create(openUIFormInfo.SerialId, uiFormAssetName,
            //             openUIFormInfo.UIGroup.Name, openUIFormInfo.PauseCoveredUIForm, progress,
            //             openUIFormInfo.UserData);
            //     _openUIFormUpdateEventHandler(this, openUIFormUpdateEventArgs);
            //     ReferencePool.Release(openUIFormUpdateEventArgs);
            // }
        }

        /// <summary>
        /// 实例化界面。
        /// </summary>
        /// <param name="uiFormAsset">要实例化的界面资源。</param>
        /// <returns>实例化后的界面。</returns>
        private object InstantiateUIForm(object uiFormAsset)
        {
            return Instantiate((UnityEngine.Object)uiFormAsset);
        }

        /// <summary>
        /// 创建界面。
        /// </summary>
        /// <param name="uiFormInstance">界面实例。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面。</returns>
        private IUIForm CreateUIForm(object uiFormInstance, IUIGroup uiGroup, object userData)
        {
            GameObject tempGo = uiFormInstance as GameObject;
            if (tempGo == null)
            {
                Log.Error("UI form instance is invalid.");
                return null;
            }

            Transform tempTrans = tempGo.transform;
            tempTrans.SetParent(((UIGroup)uiGroup).transform);
            tempTrans.localScale = Vector3.one;

            return tempGo.GetOrAddComponent<UIForm>();
        }
    }
}