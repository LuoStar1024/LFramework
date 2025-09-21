using LFramework;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    public class ProcedureSplash : ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return true;
            }
        }

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            
            Log.Info("ProcedureSplash OnInit");
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            Log.Info("ProcedureSplash OnEnter");

            var dataNode = LFrameworkEntry.GetModule<IDataNodeManager>();
            dataNode.SetData<VarInt32>("1", 100);
            var x = dataNode.GetData<VarInt32>("1");
            Log.Error(x.Value);
            
            dataNode.SetData<VarInt32>("1", 1001);
            x = dataNode.GetData<VarInt32>("1");
            Log.Error(x.Value);

            var timer = LFrameworkEntry.GetModule<ITimerManager>();
            timer.AddTimer(1, () =>
            {
                Log.Error("Timer1");
            });
            var id2 = timer.AddTimer(2, Timer2, false, 0);
            timer.AddTimer(5, Timer3);
            timer.AddTimer(10, () =>
            {
                Log.Error("Timer4");
                timer.RemoveTimer(id2);
            });
            
            var eventComponent = LFrameworkEntry.GetModule<IEventManager>();
            eventComponent.Subscribe(ProcedureTest2.EventId, OnTest2);
            
            eventComponent.Fire(this, ProcedureTest2.Create("splash"));
            eventComponent.FireNow(this, ProcedureTest2.Create("splash"));
            
            var soundComponent = LFrameworkEntry.GetModule<ISoundManager>();
            soundComponent.AddSoundGroup("testSound", 5);
            soundComponent.AddSoundGroup("UISound", 4);
            soundComponent.AddSoundGroup("BgmSound", 2);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            var eventComponent = LFrameworkEntry.GetModule<IEventManager>();
            eventComponent.Unsubscribe(ProcedureTest2.EventId, OnTest2);
            
            base.OnLeave(procedureOwner, isShutdown);
        }
        
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        }

        private void Timer2()
        {
            Log.Error("Timer2");
        }
        
        private void Timer3()
        {
            Log.Error("Timer3");
        }
        
        private void OnTest2(object sender, GameEventArgs e)
        {
            ProcedureTest2 ne = (ProcedureTest2)e;
            
            Log.Info("Test" + ne.Name);
        } 
    }
}