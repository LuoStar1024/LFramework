using UnityEngine;
using UnityEngine.UI;

namespace Launcher
{
    /// <summary>
    /// UI更新界面。
    /// </summary>
    public class UILoadUpdate : UIBase
    {
        [SerializeField]
        private Scrollbar scrollbarProgress;

        [SerializeField]
        private Text textDesc;

        [SerializeField]
        private Text textAppId;

        [SerializeField]
        private Text textResId;

        public override void OnEnter(object param)
        {
            base.OnEnter(param);
            if (param == null)
            {
                return;
            }

            textDesc.text = param.ToString();
            RefreshProgress(0f);
        }

        internal void OnRefreshVersion(string appId, string resId)
        {
            textAppId.text = string.Format(LoadText.Instance.LabelAppId, appId);
            textResId.text = string.Format(LoadText.Instance.LabelResId, resId);
        }

        /// <summary>
        /// 下载进度更新。
        /// </summary>
        /// <param name="progress">当前进度。</param>
        internal virtual void OnUpdateUIProgress(float progress)
        {
            RefreshProgress(progress);
        }

        internal void RefreshProgress(float progress)
        {
            scrollbarProgress.gameObject.SetActive(true);
            scrollbarProgress.size = progress;
        }
    }
}