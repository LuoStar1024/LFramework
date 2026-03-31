using System;
using Cysharp.Threading.Tasks;
using LFramework;
using UnityEngine;
using YooAsset;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    /// <summary>
    /// 初始化默认资源包，并根据资源模式决定后续走离线、更新或边玩边下流程。
    /// </summary>
    public class ProcedureInitPackage : ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get;
        }
        
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            // 进入流程后立即初始化 YooAsset 默认包，避免阻塞主线程状态切换。
            InitPackage(procedureOwner).Forget();
        }
        
        private async UniTaskVoid InitPackage(ProcedureOwner procedureOwner)
        {
            var resComponent = LFrameworkEntry.GetModule<IResourceManager>();
            try
            {
                var initializationOperation = await resComponent.InitPackage(resComponent.DefaultPackageName);
        
                if (initializationOperation.Status == EOperationStatus.Succeed)
                {
                    // 初始化启动阶段使用的文案配置。
                    LoadText.Instance.InitConfigData(null);
        
                    var playMode = resComponent.ResourceMode;
        
                    // 编辑器模式。
                    if (playMode == ResourceMode.EditorSimulate)
                    {
                        Log.Info("Editor resource mode detected.");
                        ChangeState<ProcedureInitResources>(procedureOwner);
                    }
                    // 单机模式。
                    else if (playMode == ResourceMode.Package)
                    {
                        Log.Info("Package resource mode detected.");
                        ChangeState<ProcedureInitResources>(procedureOwner);
                    }
                    // 可更新模式。
                    else if (playMode == ResourceMode.Updatable ||
                             playMode == ResourceMode.WebPlayMode)
                    {
                        // 更新模式下确保更新界面已经显示，后续会继续请求版本与清单。
                        LauncherMgr.Show(UIDefine.UILoadUpdate);
        
                        Log.Info("Updatable resource mode detected.");
                        ChangeState<ProcedureInitResources>(procedureOwner);
                    }
                    else
                    {
                        Log.Error("UnKnow resource mode detected Please check???");
                    }
                }
                else
                {
                    // 初始化失败时展示统一重试界面。
                    LauncherMgr.Show(UIDefine.UILoadUpdate);
        
                    Log.Error($"{initializationOperation.Error}");
        
                    // 将错误原因回显到启动界面。
                    LauncherMgr.Show(UIDefine.UILoadUpdate, $"资源初始化失败！");
        
                    LauncherMgr.ShowMessageBox(
                        $"资源初始化失败！点击确认重试 \n \n <color=#FF0000>原因{initializationOperation.Error}</color>",
                        MessageShowType.TwoButton,
                        LoadStyle.StyleEnum.Style_Retry
                        , () => { Retry(procedureOwner); }, UnityEngine.Application.Quit);
                }
            }
            catch (Exception e)
            {
                OnInitPackageFailed(procedureOwner, e.Message);
            }
        }
        
        private void OnInitPackageFailed(ProcedureOwner procedureOwner, string message)
        {
            // 异常情况下同样展示启动界面，保持用户仍处于启动器流程内。
            LauncherMgr.Show(UIDefine.UILoadUpdate);
        
            Log.Error($"{message}");
        
            // 将失败原因同步到界面，方便直接定位是清单缺失还是网络异常。
            LauncherMgr.Show(UIDefine.UILoadUpdate, $"资源初始化失败！");
        
            if (message.Contains("PackageManifest_DefaultPackage.version Error : HTTP/1.1 404 Not Found"))
            {
                message = "请检查StreamingAssets/package/DefaultPackage/PackageManifest_DefaultPackage.version是否存在";
            }
        
            LauncherMgr.ShowMessageBox($"资源初始化失败！点击确认重试 \n \n <color=#FF0000>原因{message}</color>", MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Retry
                , () => { Retry(procedureOwner); },
                Application.Quit);
        }
        
        private void Retry(ProcedureOwner procedureOwner)
        {
            // 重试时复用当前启动界面，避免重复创建 UI。
            LauncherMgr.Show(UIDefine.UILoadUpdate, $"重新初始化资源中...");
        
            InitPackage(procedureOwner).Forget();
        }
    }
}