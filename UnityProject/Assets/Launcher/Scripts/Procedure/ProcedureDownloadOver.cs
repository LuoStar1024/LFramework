using LFramework;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    /// <summary>
    /// 下载完成后的收尾流程，记录当前版本并决定是否执行缓存清理。
    /// </summary>
    public class ProcedureDownloadOver : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        private bool _needClearCache;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            Log.Info("下载完成!!!");

            LauncherMgr.Show(UIDefine.UILoadUpdate, $"下载完成...");

            // 下载完成后将最新版本写入本地，供离线回退或下次启动使用。
            var resComponent = LFrameworkEntry.GetModule<IResourceManager>();
            var settingComponent = LFrameworkEntry.GetModule<ISettingManager>();
            settingComponent.SetString("GAME_VERSION", resComponent.PackageVersion);
            settingComponent.Save();
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            if (_needClearCache)
            {
                // 若需要回收旧版本缓存，则先清理后再继续进入游戏。
                ChangeState<ProcedureClearCache>(procedureOwner);
            }
            else
            {
                // 默认直接进入预加载阶段，准备启动热更逻辑。
                ChangeState<ProcedurePreload>(procedureOwner);
            }
        }
    }
}