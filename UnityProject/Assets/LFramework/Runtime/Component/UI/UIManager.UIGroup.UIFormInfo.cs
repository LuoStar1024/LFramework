namespace LFramework
{
    internal sealed partial class UIManager
    {
        private sealed partial class UIGroup : IUIGroup
        {
            /// <summary>
            /// 界面组界面信息。
            /// </summary>
            private sealed class UIFormInfo : IReference
            {
                private IUIForm _uiForm;
                private bool _paused;
                private bool _covered;

                public UIFormInfo()
                {
                    _uiForm = null;
                    _paused = false;
                    _covered = false;
                }

                public IUIForm UIForm
                {
                    get { return _uiForm; }
                }

                public bool Paused
                {
                    get { return _paused; }
                    set { _paused = value; }
                }

                public bool Covered
                {
                    get { return _covered; }
                    set { _covered = value; }
                }

                public static UIFormInfo Create(IUIForm uiForm)
                {
                    if (uiForm == null)
                    {
                        throw new LFrameworkException("UI form is invalid.");
                    }

                    UIFormInfo uiFormInfo = ReferencePool.Acquire<UIFormInfo>();
                    uiFormInfo._uiForm = uiForm;
                    uiFormInfo._paused = true;
                    uiFormInfo._covered = true;
                    return uiFormInfo;
                }

                public void Clear()
                {
                    _uiForm = null;
                    _paused = false;
                    _covered = false;
                }
            }
        }
    }
}