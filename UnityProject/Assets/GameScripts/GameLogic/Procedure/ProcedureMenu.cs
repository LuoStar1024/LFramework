using LFramework;

namespace GameLogic
{
    /// <summary>
    /// 主菜单流程，展示菜单界面并等待玩家开始游戏。
    /// </summary>
    public class ProcedureMenu : ProcedureBase
    {
        private bool _startGame = false;
        
        /// <summary>
        /// 由菜单界面回调触发，通知流程开始进入游戏场景。
        /// </summary>
        public void StartGame()
        {
            _startGame = true;
        }
        
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _startGame = false;
            var id = procedureOwner.GetData<VarInt32>(Constant.Setting.ChangeSceneFormKey);
            // 关闭过场 Loading 界面后，显示主菜单。
            GameEntry.UI.CloseUIForm(id);
            GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset("MenuForm"), Constant.Setting.UIGroupNormal, this);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (_startGame)
            {
                // 菜单确认开始游戏后，切换到正式游戏场景。
                procedureOwner.SetData<VarString>(Constant.Setting.ChangeSceneNameKey, "Game");
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }
    }
}