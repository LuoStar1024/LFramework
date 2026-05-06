using LFramework;

namespace GameLogic
{
    /// <summary>
    /// 游戏内主流程，显示战斗/玩法界面并监听返回主菜单事件。
    /// </summary>
    public class ProcedureGame : ProcedureBase
    {
        private bool _isReturn = false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _isReturn = false;

            // 进入游戏流程时开始监听返回菜单事件。
            GameEntry.Event.Subscribe(EventGroupUI.ReturnMenuId, OnReturnMenu);

            var id = procedureOwner.GetData<VarInt32>(Constant.Setting.ChangeSceneFormKey);
            // 关闭场景切换 Loading 界面，打开游戏内主界面。
            GameEntry.UI.CloseUIForm(id);
            GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset("GameInfoForm"), Constant.Setting.UIGroupNormal, this);

            GameEntry.Audio.PlayBgm(2);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            // 离开游戏流程时解除事件和界面占用，避免重复订阅或残留 UI。
            GameEntry.Event.Unsubscribe(EventGroupUI.ReturnMenuId, OnReturnMenu);
            var forms = GameEntry.UI.GetUIForms(AssetUtility.GetUIFormAsset("GameInfoForm"));
            if (forms != null && forms.Length > 0)
            {
                GameEntry.UI.CloseUIForm(forms[0]);
            }
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (_isReturn)
            {
                // 游戏中返回菜单时，重新切回菜单场景。
                procedureOwner.SetData<VarString>(Constant.Setting.ChangeSceneNameKey, "Menu");
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }

        private void OnReturnMenu()
        {
            _isReturn = true;
        }
    }
}