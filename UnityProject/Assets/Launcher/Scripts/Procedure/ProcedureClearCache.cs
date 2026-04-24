using LFramework;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    /// <summary>
    /// 清理无用缓存文件，避免历史补丁占用磁盘或影响后续资源定位。
    /// </summary>
    public class ProcedureClearCache : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        private ProcedureOwner _procedureOwner;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            _procedureOwner = procedureOwner;
            Log.Info("清理未使用的缓存文件！");

            LauncherMgr.ShowUI<UILoadUpdate>("清理未使用的缓存文件...");

            var resComponent = LFrameworkEntry.GetModule<IResourceManager>();
            // 清理完成后再恢复主流程，避免缓存回收过程中触发后续资源加载。
            var operation = resComponent.ClearCacheFilesAsync();
            operation.Completed += Operation_Completed;
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            _procedureOwner = null;
        }


        /// <summary>
        /// 缓存清理完成后进入预加载阶段。
        /// </summary>
        private void Operation_Completed(YooAsset.AsyncOperationBase obj)
        {
            LauncherMgr.ShowUI<UILoadUpdate>("清理完成 即将进入游戏...");

            ChangeState<ProcedurePreload>(_procedureOwner);
        }
    }
}