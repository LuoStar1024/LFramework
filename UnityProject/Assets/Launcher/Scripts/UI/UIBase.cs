using UnityEngine;

namespace Launcher
{
    /// <summary>
    /// 热更UI基类。
    /// </summary>
    public class UIBase : MonoBehaviour
    {
        protected object Param;

        /// <summary>
        /// 界面名称。
        /// </summary>
        public virtual string UIName => GetType().Name;

        public virtual void OnEnter(object param)
        {
            Param = param;
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual void Close()
        {
            LauncherMgr.CloseUI(this);
        }
    }
}