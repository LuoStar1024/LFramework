using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using LFramework;
using UnityEngine;
using YooAsset;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    /// <summary>
    /// 预加载启动阶段依赖的配置和资源，为热更程序集启动做准备。
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        private float _progress = 0f;

        private readonly Dictionary<string, bool> _loadedFlag = new Dictionary<string, bool>();

        public override bool UseNativeDialog => true;

        private readonly bool _needProLoadConfig = true;

        private ProcedureOwner _procedureOwner;

        private IResourceManager _resComponent;

        /// <summary>
        /// 预加载回调。
        /// </summary>
        private LoadAssetCallbacks _preLoadAssetCallbacks;

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            _procedureOwner = procedureOwner;
            _preLoadAssetCallbacks = new LoadAssetCallbacks(OnPreLoadAssetSuccess, OnPreLoadAssetFailure);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _resComponent = LFrameworkEntry.GetModule<IResourceManager>();
            _loadedFlag.Clear();

            // 预加载阶段主要展示配置和启动资源的加载进度。
            LauncherMgr.ShowUI<UILoadUpdate>(Utility.Text.Format(LoadText.Instance.LabelLoadLoadProgress, 0));

            // GameEvent.Send("UILoadUpdate.RefreshVersion");

            PreloadResources();
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            _progress = 0f;
            _loadedFlag.Clear();
            _procedureOwner = null;
            _resComponent = null;
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            var totalCount = _loadedFlag.Count <= 0 ? 1 : _loadedFlag.Count;

            var loadCount = _loadedFlag.Count <= 0 ? 1 : 0;

            foreach (KeyValuePair<string, bool> loadedFlag in _loadedFlag)
            {
                if (!loadedFlag.Value)
                {
                    break;
                }
                else
                {
                    loadCount++;
                }
            }

            if (_loadedFlag.Count != 0)
            {
                LauncherMgr.ShowUI<UILoadUpdate>(Utility.Text.Format(LoadText.Instance.LabelLoadLoadProgress,
                    (float)loadCount / totalCount * 100));
            }
            else
            {
                LauncherMgr.UpdateUIProgress(_progress);

                string progressStr = $"{_progress * 100:f1}";

                if (Math.Abs(_progress - 1f) < 0.001f)
                {
                    LauncherMgr.ShowUI<UILoadUpdate>(LoadText.Instance.LabelLoadLoadComplete);
                }
                else
                {
                    LauncherMgr.ShowUI<UILoadUpdate>(Utility.Text.Format(LoadText.Instance.LabelLoadLoadProgress,
                        progressStr));
                }
            }

            if (loadCount < totalCount)
            {
                return;
            }

            // 所有预加载资源准备完成后，进入热更程序集加载阶段。
            ChangeProcedureToLoadAssembly();
        }


        private async UniTaskVoid SmoothValue(float value, float duration, Action callback = null)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                var result = Mathf.Lerp(0, value, time / duration);
                _progress = result;
                await UniTask.Yield();
            }

            _progress = value;
            callback?.Invoke();
        }

        private void PreloadResources()
        {
            if (_needProLoadConfig)
            {
                LoadAllConfig();
            }
        }

        private void LoadAllConfig()
        {
            // 通过 PRELOAD 标签统一收集启动阶段必须优先准备的资源。
            AssetInfo[] assetInfos = YooAssets.GetAssetInfos("PRELOAD");
            foreach (var assetInfo in assetInfos)
            {
                PreLoad(Path.ChangeExtension(assetInfo.AssetPath, null));
            }
#if UNITY_WEBGL
            AssetInfo[] webAssetInfos = _resComponent.GetAssetInfos("WEBGL_PRELOAD");
            foreach (var assetInfo in webAssetInfos)
            {
                PreLoad(assetInfo.Address);
            }
#endif
            if (_loadedFlag.Count <= 0)
            {
                // SmoothValue(1, 1f, ChangeProcedureToLoadAssembly).Forget();
                return;
            }
        }

        private void PreLoad(string location)
        {
            _loadedFlag.Add(location, false);
            // 这里只负责触发异步加载，完成状态在回调里更新。
            _resComponent.LoadAsset(location, 100, _preLoadAssetCallbacks, null);
        }

        private void OnPreLoadAssetFailure(string assetName, LoadResourceStatus status, string errormessage,
            object userdata)
        {
            Log.Warning("Can not preload asset from '{0}' with error message '{1}'.", assetName, errormessage);
            _loadedFlag[assetName] = true;
        }

        private void OnPreLoadAssetSuccess(string assetName, object asset, float duration, object userdata)
        {
            Log.Debug("Success preload asset from '{0}' duration '{1}'.", assetName, duration);
            _loadedFlag[assetName] = true;
        }

        private void ChangeProcedureToLoadAssembly()
        {
            ChangeState<ProcedureLoadAssembly>(_procedureOwner);
        }
    }
}