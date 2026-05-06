using System;
using System.Collections.Generic;
using UnityEngine;

namespace Launcher
{
    /// <summary>
    /// 热更界面加载管理器。
    /// </summary>
    public static class LauncherMgr
    {
        private static string UI_ROOT_PATH = "LauncherUIRoot";
        private static Transform _uiRoot;
        private static readonly Dictionary<string, UIBase> _uiMap = new Dictionary<string, UIBase>();

        public static void Initialize()
        {
            _uiRoot = GameObject.Find(UI_ROOT_PATH)?.transform;
            if (_uiRoot == null)
            {
                Debug.LogError("Failed to Find UIRoot. Please check the resource path");
                return;
            }

            Debug.Log("======== 初始化 LauncherMgr 完成 ========");
        }

        public static void ShowUI<T>(object param = null) where T : UIBase
        {
            Show(typeof(T).Name, param);
        }

        public static void Show(string uiInfo, object param = null)
        {
            if (string.IsNullOrEmpty(uiInfo))
            {
                Debug.LogWarning($"======== LauncherMgr.ShowUI UIName 为空 ========");
                return;
            }

            if (!_uiMap.TryGetValue(uiInfo, out var uiBase))
            {
                var uiTransform = _uiRoot.Find(uiInfo);
                if (uiTransform == null)
                {
                    Debug.LogError($"not find ui:{uiInfo}");
                    return;
                }

                var ui = uiTransform.gameObject;
                ui.transform.SetParent(_uiRoot.transform);
                ui.transform.localScale = Vector3.one;
                ui.transform.localRotation = Quaternion.identity;
                ui.transform.localPosition = Vector3.zero;
                var rect = ui.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = Vector2.zero;
                }

                uiBase = ui.GetComponent<UIBase>();
                if (uiBase == null)
                {
                    Debug.LogError($"ui component missing:{uiInfo}");
                    return;
                }

                _uiMap.Add(uiInfo, uiBase);
            }

            uiBase.Show();
            uiBase.transform.SetAsLastSibling();
            uiBase.OnEnter(param);
        }

        public static void CloseUI(UIBase ui)
        {
            if (ui == null)
            {
                return;
            }

            CloseUI(ui.UIName);
        }

        public static void CloseUI<T>() where T : UIBase
        {
            CloseUI(typeof(T).Name);
        }

        public static void CloseUI(string uiName)
        {
            if (string.IsNullOrEmpty(uiName))
            {
                return;
            }

            if (!_uiMap.TryGetValue(uiName, out var ui))
            {
                return;
            }

            ui.Hide();
            _uiMap.Remove(uiName);
        }

        public static T GetActiveUI<T>() where T : UIBase
        {
            return GetActiveUI(typeof(T).Name) as T;
        }

        public static UIBase GetActiveUI(string uiName)
        {
            return _uiMap.GetValueOrDefault(uiName);
        }

        public static void HideAll()
        {
            foreach (var item in _uiMap.Values)
            {
                if (item != null && item.gameObject != null)
                {
                    item.Hide();
                }
            }

            _uiMap.Clear();
            if (_uiRoot != null)
            {
                _uiRoot.gameObject.SetActive(false);
            }
        }

        public static void ShowMessageBox(string desc, Action onConfirm = null,
            Action onCancel = null, Action onUpdate = null)
        {
            ShowUI<UILoadTip>(desc);
            GetActiveUI<UILoadTip>()?.SetAllCallback(onConfirm, onUpdate, onCancel);
        }

        public static void UpdateUIProgress(float progress)
        {
            ShowUI<UILoadUpdate>();
            GetActiveUI<UILoadUpdate>()?.OnUpdateUIProgress(progress);
        }
    }
}