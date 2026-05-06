using System.Collections;
using LFramework;
using UnityEngine;
using YooAsset;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    /// <summary>
    /// 初始化资源版本与清单，判断当前是否需要进入补丁下载阶段。
    /// </summary>
    public class ProcedureInitResources : ProcedureBase
    {
        private bool _initResourcesComplete = false;
        private ProcedureOwner _procedureOwner;
        private IResourceManager _resComponent;
        private IConfigManager _configComponent;
        private ISettingManager _settingComponent;
        private IUnityWrapperManager _unityWrapperComponent;

        public override bool UseNativeDialog
        {
            get { return true; }
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _initResourcesComplete = false;
            _procedureOwner = procedureOwner;
            _resComponent = LFrameworkEntry.GetModule<IResourceManager>();
            _configComponent = LFrameworkEntry.GetModule<IConfigManager>();
            _settingComponent = LFrameworkEntry.GetModule<ISettingManager>();

            // 这一阶段会请求版本文件与清单，因此先给用户明确的初始化提示。
            LauncherMgr.ShowUI<UILoadUpdate>("初始化资源中...");

            // 注意：使用单机模式并初始化资源前，需要先构建 AssetBundle 并复制到 StreamingAssets 中，否则会产生 HTTP 404 错误
            _unityWrapperComponent = LFrameworkEntry.GetModule<IUnityWrapperManager>();
            _unityWrapperComponent.StartCoroutineWrapper(InitResources(procedureOwner));
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            _initResourcesComplete = false;
            _procedureOwner = null;
            _resComponent = null;
            _configComponent = null;
            _settingComponent = null;
            _unityWrapperComponent = null;
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!_initResourcesComplete)
            {
                // 初始化资源未完成则继续等待
                return;
            }

            if (_resComponent.ResourceMode == ResourceMode.Updatable ||
                _resComponent.ResourceMode == ResourceMode.WebPlayMode)
            {
                // 这里的 PackageVersion 已经是最新远端版本，用于后续下载器与版本记录。
                Log.Debug(
                    $"Updated package Version : from {_resComponent.GetPackageVersion()} to {_resComponent.PackageVersion}");
                // WebGL 或边玩边下不需要阻塞在完整下载流程，直接进入预加载。
                if (_resComponent.ResourceMode == ResourceMode.WebPlayMode ||
                    _resComponent.UpdatableWhilePlaying)
                {
                    // 边玩边下载还可以拓展首包支持。
                    ChangeToPreloadState(procedureOwner);
                    return;
                }

                ChangeToCreateDownloaderState(procedureOwner);
                return;
            }

            ChangeToPreloadState(procedureOwner);
        }

        /// <summary>
        /// 请求远端版本并更新资源清单，为后续下载器和资源定位提供依据。
        /// </summary>
        /// <remarks>YooAsset 需要保持编辑器、单机、联机模式流程一致。</remarks>
        private IEnumerator InitResources(ProcedureOwner procedureOwner)
        {
            Log.Info("更新资源清单！！！");
            LauncherMgr.ShowUI<UILoadUpdate>("更新清单文件...");

            // 1. 获取资源清单的版本信息
            var operation1 = _resComponent.RequestPackageVersionAsync();
            yield return operation1;
            if (operation1.Status != EOperationStatus.Succeed)
            {
                OnInitResourcesError(procedureOwner, operation1.Error);
                yield break;
            }

            var packageVersion = operation1.PackageVersion;
            _resComponent.PackageVersion = packageVersion;

            if (_settingComponent.HasSetting("GAME_VERSION"))
            {
                _settingComponent.SetString("GAME_VERSION", _resComponent.PackageVersion);
            }

            Log.Info($"Init resource package version : {packageVersion}");

            // 2. 根据版本号拉取对应 manifest，后续资源定位和增量下载都依赖该清单。
            var operation2 = _resComponent.UpdatePackageManifestAsync(packageVersion);
            yield return operation2;
            if (operation2.Status != EOperationStatus.Succeed)
            {
                OnInitResourcesError(procedureOwner, operation2.Error);
                yield break;
            }

            _initResourcesComplete = true;
        }

        private void ChangeToPreloadState(ProcedureOwner procedureOwner)
        {
            ChangeState<ProcedurePreload>(procedureOwner);
        }

        private void ChangeToCreateDownloaderState(ProcedureOwner procedureOwner)
        {
            ChangeState<ProcedureCreateDownloader>(procedureOwner);
        }

        private void OnInitResourcesError(ProcedureOwner procedureOwner, string message)
        {
            // 更新模式下优先判断是否允许离线继续，否则直接提示重试或退出。
            if (_resComponent.ResourceMode == ResourceMode.Updatable)
            {
                if (!IsNeedUpdate())
                {
                    return;
                }
                else
                {
                    Log.Error(message);
                    LauncherMgr.ShowMessageBox($"获取远程版本失败！点击确认重试\n <color=#FF0000>{message}</color>",
                        () => { _unityWrapperComponent.StartCoroutineWrapper(InitResources(procedureOwner)); },
                        Application.Quit);
                    return;
                }
            }

            Log.Error(message);
            LauncherMgr.ShowMessageBox($"初始化资源失败！点击确认重试 \n <color=#FF0000>{message}</color>",
                () => { _unityWrapperComponent.StartCoroutineWrapper(InitResources(procedureOwner)); },
                Application.Quit);
        }

        private bool IsNeedUpdate()
        {
            // 非强更且不能联网时，尝试回退到本地已记录版本继续进入游戏。
            if (_configComponent.UpdateConfig.UpdateStyle == UpdateStyle.Optional &&
                !_resComponent.UpdatableWhilePlaying)
            {
                // 获取上次成功记录的版本
                string packageVersion = _settingComponent.GetString("GAME_VERSION", string.Empty);
                if (string.IsNullOrEmpty(packageVersion))
                {
                    LauncherMgr.ShowUI<UILoadUpdate>(LoadText.Instance.LabelNetUnReachable);
                    LauncherMgr.ShowMessageBox("没有找到本地版本记录，需要更新资源！",
                        () => { _unityWrapperComponent.StartCoroutineWrapper(InitResources(_procedureOwner)); },
                        Application.Quit);
                    return false;
                }

                _resComponent.PackageVersion = packageVersion;

                if (_configComponent.UpdateConfig.UpdateNotice == UpdateNotice.Notice)
                {
                    LauncherMgr.ShowUI<UILoadUpdate>(LoadText.Instance.LabelLoadNotice);
                    LauncherMgr.ShowMessageBox($"更新失败，检测到可选资源更新，推荐完成更新提升游戏体验！ \\n \\n 确定再试一次，取消进入游戏",
                        () => { _unityWrapperComponent.StartCoroutineWrapper(InitResources(_procedureOwner)); },
                        () => { ChangeState<ProcedurePreload>(_procedureOwner); });
                }
                else
                {
                    ChangeState<ProcedurePreload>(_procedureOwner);
                }

                return false;
            }

            return true;
        }
    }
}