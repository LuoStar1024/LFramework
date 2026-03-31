using System;
using LFramework;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 界面。
    /// </summary>
    public sealed class UIForm : MonoBehaviour, IUIForm
    {
        private int _serialId;
        private string _uiFormAssetName;
        private IUIGroup _uiGroup;
        private int _depthInUIGroup;
        private bool _pauseCoveredUIForm;
        private UIFormLogic _uiFormLogic;

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        public int SerialId
        {
            get { return _serialId; }
        }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string UIFormAssetName
        {
            get { return _uiFormAssetName; }
        }

        /// <summary>
        /// 获取界面实例。
        /// </summary>
        public object Handle
        {
            get { return gameObject; }
        }

        /// <summary>
        /// 获取界面所属的界面组。
        /// </summary>
        public IUIGroup UIGroup
        {
            get { return _uiGroup; }
        }

        /// <summary>
        /// 获取界面深度。
        /// </summary>
        public int DepthInUIGroup
        {
            get { return _depthInUIGroup; }
        }

        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm
        {
            get { return _pauseCoveredUIForm; }
        }

        /// <summary>
        /// 获取界面逻辑。
        /// </summary>
        public UIFormLogic Logic
        {
            get { return _uiFormLogic; }
        }

        /// <summary>
        /// 初始化界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroup">界面所处的界面组。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="isNewInstance">是否是新实例。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnInit(int serialId, string uiFormAssetName, IUIGroup uiGroup, bool pauseCoveredUIForm,
            bool isNewInstance, object userData)
        {
            _serialId = serialId;
            _uiFormAssetName = uiFormAssetName;
            _uiGroup = uiGroup;
            _depthInUIGroup = 0;
            _pauseCoveredUIForm = pauseCoveredUIForm;

            if (!isNewInstance)
            {
                return;
            }

            _uiFormLogic = GetComponent<UIFormLogic>();
            if (_uiFormLogic == null)
            {
                Log.Error("UI form '{0}' can not get UI form logic.", uiFormAssetName);
                return;
            }

            try
            {
                _uiFormLogic.OnInit(userData);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnInit with exception '{2}'.", _serialId, _uiFormAssetName, exception);
            }
        }

        /// <summary>
        /// 界面回收。
        /// </summary>
        public void OnRecycle()
        {
            try
            {
                _uiFormLogic.OnRecycle();
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnRecycle with exception '{2}'.", _serialId, _uiFormAssetName, exception);
            }

            _serialId = 0;
            _depthInUIGroup = 0;
            _pauseCoveredUIForm = true;
        }

        /// <summary>
        /// 界面打开。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnOpen(object userData)
        {
            try
            {
                _uiFormLogic.OnOpen(userData);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnOpen with exception '{2}'.", _serialId, _uiFormAssetName, exception);
            }
        }

        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnClose(bool isShutdown, object userData)
        {
            try
            {
                _uiFormLogic.OnClose(isShutdown, userData);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnClose with exception '{2}'.", _serialId, _uiFormAssetName, exception);
            }
        }

        /// <summary>
        /// 界面暂停。
        /// </summary>
        public void OnPause()
        {
            try
            {
                _uiFormLogic.OnPause();
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnPause with exception '{2}'.", _serialId, _uiFormAssetName, exception);
            }
        }

        /// <summary>
        /// 界面暂停恢复。
        /// </summary>
        public void OnResume()
        {
            try
            {
                _uiFormLogic.OnResume();
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnResume with exception '{2}'.", _serialId, _uiFormAssetName, exception);
            }
        }

        /// <summary>
        /// 界面遮挡。
        /// </summary>
        public void OnCover()
        {
            try
            {
                _uiFormLogic.OnCover();
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnCover with exception '{2}'.", _serialId, _uiFormAssetName, exception);
            }
        }

        /// <summary>
        /// 界面遮挡恢复。
        /// </summary>
        public void OnReveal()
        {
            try
            {
                _uiFormLogic.OnReveal();
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnReveal with exception '{2}'.", _serialId, _uiFormAssetName, exception);
            }
        }

        /// <summary>
        /// 界面激活。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnRefocus(object userData)
        {
            try
            {
                _uiFormLogic.OnRefocus(userData);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnRefocus with exception '{2}'.", _serialId, _uiFormAssetName, exception);
            }
        }

        /// <summary>
        /// 界面轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            try
            {
                _uiFormLogic.OnUpdate(elapseSeconds, realElapseSeconds);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnUpdate with exception '{2}'.", _serialId, _uiFormAssetName, exception);
            }
        }

        /// <summary>
        /// 界面深度改变。
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度。</param>
        public void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            _depthInUIGroup = depthInUIGroup;
            try
            {
                _uiFormLogic.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnDepthChanged with exception '{2}'.", _serialId, _uiFormAssetName,
                    exception);
            }
        }
    }
}