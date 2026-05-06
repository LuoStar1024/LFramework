using System;
using Cysharp.Threading.Tasks;
using LFramework;
using UnityEngine;
using YooAsset;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    /// <summary>
    /// 创建补丁下载器，统计待更新文件数量并让用户决定是否开始下载。
    /// </summary>
    public class ProcedureCreateDownloader : ProcedureBase
    {
        private int _curTryCount;

        private const int MAX_TRY_COUNT = 3;

        public override bool UseNativeDialog { get; }

        private ProcedureOwner _procedureOwner;

        private ResourceDownloaderOperation _downloader;

        private int _totalDownloadCount;

        private string _totalSizeMb;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            _procedureOwner = procedureOwner;

            Log.Info("创建补丁下载器");

            LauncherMgr.ShowUI<UILoadUpdate>("创建补丁下载器...");

            CreateDownloader().Forget();
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            _curTryCount = 0;
            _procedureOwner = null;
            _downloader = null;
            _totalDownloadCount = 0;
            _totalSizeMb = null;
        }

        private async UniTaskVoid CreateDownloader()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            var resComponent = LFrameworkEntry.GetModule<IResourceManager>();
            _downloader = resComponent.CreateResourceDownloader();

            if (_downloader.TotalDownloadCount == 0)
            {
                Log.Info("Not found any download files !");
                ChangeState<ProcedureDownloadOver>(_procedureOwner);
            }
            else
            {
                // 找到待更新资源后，计算数量和体积供用户确认。
                Log.Info($"Found total {_downloader.TotalDownloadCount} files that need download ！");

                // 正式下载前先停留在确认界面，避免用户无感知地开始拉取补丁。
                _totalDownloadCount = _downloader.TotalDownloadCount;
                long totalDownloadBytes = _downloader.TotalDownloadBytes;

                float sizeMb = totalDownloadBytes / 1048576f;
                sizeMb = Mathf.Clamp(sizeMb, 0.1f, float.MaxValue);
                _totalSizeMb = sizeMb.ToString("f1");

                LauncherMgr.ShowMessageBox(
                    $"Found update patch files, Total count {_totalDownloadCount} Total size {_totalSizeMb}MB",
                    StartDownFile, Application.Quit);
            }
        }

        /// <summary>
        /// 用户确认后进入实际下载流程。
        /// </summary>
        void StartDownFile()
        {
            ChangeState<ProcedureDownloadFile>(_procedureOwner);
        }
    }
}