using LFramework;
using UnityEngine;

namespace GameLogic
{
    public class LoginForm : UguiForm
    {
        private ProcedureLogin _procedureLogin = null;

        public void OnLoginBtnClick()
        {
            PlayUISound(1001);
            _procedureLogin.LoginGame();
            Close();
        }
        
        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            _procedureLogin = (ProcedureLogin)userData;
            if (_procedureLogin == null)
            {
                Log.Warning("ProcedureLogin is invalid when open LoginForm.");
                return;
            }
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            _procedureLogin = null;
            
            base.OnClose(isShutdown, userData);
        }
    }
}