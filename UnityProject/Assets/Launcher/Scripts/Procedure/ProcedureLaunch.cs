using LFramework;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    public class ProcedureLaunch : ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return true;
            }
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Log.Info("进入框架的生命周期");
            
            var eventComponent = LFrameworkEntry.GetModule<IEventManager>();
            eventComponent.Subscribe(ProcedureTest1.EventId, OnTest1);
            
            eventComponent.FireNow(this, ProcedureTest1.Create(1));
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            var eventComponent = LFrameworkEntry.GetModule<IEventManager>();
            eventComponent.Unsubscribe(ProcedureTest1.EventId, OnTest1);
            
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            Log.Info("ProcedureLaunch OnUpdate");
            
            var eventComponent = LFrameworkEntry.GetModule<IEventManager>();
            eventComponent.Fire(this, ProcedureTest1.Create(1));
            
            // 运行一帧即切换到 Splash 展示流程
            ChangeState<ProcedureSplash>(procedureOwner);
        }
        
        private void OnTest1(object sender, GameEventArgs e)
        {
            ProcedureTest1 ne = (ProcedureTest1)e;
            
            Log.Info("Test" + ne.TestId);
        } 
    }
}