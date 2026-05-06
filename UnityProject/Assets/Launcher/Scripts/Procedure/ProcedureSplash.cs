using LFramework;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    /// <summary>
    /// 启动过渡流程，用来在首帧展示启动界面后切到资源包初始化。
    /// </summary>
    public class ProcedureSplash : ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get { return true; }
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // Splash 仅作为短暂过渡，下一步开始初始化资源包。
            ChangeState<ProcedureInitPackage>(procedureOwner);
        }
    }
}