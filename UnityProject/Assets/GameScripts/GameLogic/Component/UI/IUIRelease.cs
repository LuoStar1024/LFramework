namespace GameLogic
{
    /// <summary>
    /// 界面资源释放接口。
    /// </summary>
    public interface IUIRelease
    {
        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="uiFormAsset">要释放的界面资源。</param>
        /// <param name="uiFormInstance">要释放的界面实例。</param>
        void ReleaseUIForm(object uiFormAsset, object uiFormInstance);
    }
}