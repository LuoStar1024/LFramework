using LFramework;

namespace GameLogic
{
    public class ProcedureGameLogicLaunch : ProcedureBase
    {
        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            
            ChangeState<ProcedureLogin>(procedureOwner);
        }
    }
}