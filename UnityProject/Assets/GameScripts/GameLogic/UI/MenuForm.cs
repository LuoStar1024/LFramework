using LFramework;
using UnityEngine;

namespace GameLogic
{
    public class MenuForm : UguiForm
    {
        private ProcedureMenu _procedureMenu = null;

        public void OnStartBtnClick()
        {
            PlayUISound(1001);
            _procedureMenu.StartGame();
            Close();
        }

        public void OnSettingBtnClick()
        {
            GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset("SettingForm"), Constant.Setting.UIGroupNormal);
        }

        public void OnQuitBtnClick()
        {
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            _procedureMenu = (ProcedureMenu)userData;
            if (_procedureMenu == null)
            {
                Log.Warning("ProcedureMenu is invalid when open MenuForm.");
                return;
            }
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            _procedureMenu = null;

            base.OnClose(isShutdown, userData);
        }
    }
}