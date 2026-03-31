using LFramework;

namespace GameLogic
{
    /// <summary>
    /// 登录流程，打开登录界面并等待玩家确认进入主菜单。
    /// </summary>
    public class ProcedureLogin : ProcedureBase
    {
        private bool _loginSuccess = false;
        
        /// <summary>
        /// 登录成功后由界面回调触发，通知流程进入下一阶段。
        /// </summary>
        public void LoginGame()
        {
            _loginSuccess = true;
        }
        
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _loginSuccess = false;
            // 打开登录界面，界面逻辑会在成功时回调当前流程。
            GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset("LoginForm"), Constant.Setting.UIGroupNormal, this);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (_loginSuccess)
            {
                // 登录通过后，先切到菜单场景，再进入菜单流程。
                procedureOwner.SetData<VarString>(Constant.Setting.ChangeSceneNameKey, "Menu");
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }
    }
}