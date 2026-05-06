using System;
using Cysharp.Threading.Tasks;
using LFramework;
using UnityEngine;
using YooAsset;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    /// <summary>
    /// 执行补丁下载，并实时刷新下载进度、速度和剩余时间。
    /// </summary>
    public class ProcedureDownloadFile : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        private ProcedureOwner _procedureOwner;

        private float _lastUpdateDownloadedSize;
        private float _totalSpeed;
        private int _speedSampleCount;

        private IResourceManager _resComponent;

        private float CurrentSpeed
        {
            get
            {
                float interval = Math.Max(Time.deltaTime, 0.01f); // 防止deltaTime过小
                var sizeDiff = _resComponent.Downloader.CurrentDownloadBytes - _lastUpdateDownloadedSize;
                _lastUpdateDownloadedSize = _resComponent.Downloader.CurrentDownloadBytes;
                var speed = sizeDiff / interval;

                // 使用滑动窗口计算平均速度
                _totalSpeed += speed;
                _speedSampleCount++;
                return _totalSpeed / _speedSampleCount;
            }
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _procedureOwner = procedureOwner;
            _resComponent = LFrameworkEntry.GetModule<IResourceManager>();

            Log.Info("开始下载更新文件！");

            LauncherMgr.ShowUI<UILoadUpdate>("开始下载更新文件...");

            BeginDownload().Forget();
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            _lastUpdateDownloadedSize = 0f;
            _totalSpeed = 0f;
            _speedSampleCount = 0;
            _procedureOwner = null;
            _resComponent = null;
        }

        private async UniTaskVoid BeginDownload()
        {
            var downloader = _resComponent.Downloader;

            // 下载过程中的错误、进度都统一回流到当前流程刷新 UI。
            downloader.DownloadErrorCallback = OnDownloadErrorCallback;
            downloader.DownloadUpdateCallback = OnDownloadProgressCallback;
            downloader.BeginDownload();
            await downloader;

            // 只有全部下载成功后，才进入下载完成收尾阶段。
            if (downloader.Status != EOperationStatus.Succeed)
                return;

            ChangeState<ProcedureDownloadOver>(_procedureOwner);
        }

        private void OnDownloadErrorCallback(DownloadErrorData downloadErrorData)
        {
            LauncherMgr.ShowMessageBox($"Failed to download file : {downloadErrorData.FileName}",
                () => { ChangeState<ProcedureCreateDownloader>(_procedureOwner); }, UnityEngine.Application.Quit);
        }

        private void OnDownloadProgressCallback(DownloadUpdateData downloadUpdateData)
        {
            // 将下载器状态转换为玩家可理解的进度、体积和网速信息。
            string currentSizeMb = (downloadUpdateData.CurrentDownloadBytes / 1048576f).ToString("f1");
            string totalSizeMb = (downloadUpdateData.TotalDownloadBytes / 1048576f).ToString("f1");
            float progressPercentage = _resComponent.Downloader.Progress * 100;
            string speed = Utility.File.GetLengthString((int)CurrentSpeed);

            string line1 = Utility.Text.Format("正在更新，已更新 {0}/{1} ({2:F2}%)", downloadUpdateData.CurrentDownloadCount,
                downloadUpdateData.TotalDownloadCount, progressPercentage);
            string line2 = Utility.Text.Format("已更新大小 {0}MB/{1}MB", currentSizeMb, totalSizeMb);
            string line3 = Utility.Text.Format("当前网速 {0}/s，剩余时间 {1}", speed,
                GetRemainingTime(downloadUpdateData.TotalDownloadBytes, downloadUpdateData.CurrentDownloadBytes,
                    CurrentSpeed));

            LauncherMgr.UpdateUIProgress(_resComponent.Downloader.Progress);
            LauncherMgr.ShowUI<UILoadUpdate>($"{line1}\n{line2}\n{line3}");

            Log.Info($"{line1} {line2} {line3}");
        }

        private string GetRemainingTime(long totalBytes, long currentBytes, float speed)
        {
            int needTime = 0;
            if (speed > 0)
            {
                needTime = (int)((totalBytes - currentBytes) / speed);
            }

            TimeSpan ts = new TimeSpan(0, 0, needTime);
            return ts.ToString(@"mm\:ss");
        }
    }
}