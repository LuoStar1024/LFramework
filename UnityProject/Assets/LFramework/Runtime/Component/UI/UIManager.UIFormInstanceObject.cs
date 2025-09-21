namespace LFramework
{
    internal sealed partial class UIManager
    {
        /// <summary>
        /// 界面实例对象。
        /// </summary>
        private sealed class UIFormInstanceObject : ObjectBase
        {
            private object _uiFormAsset;
            private IUIFormHelper _uiFormHelper;

            public UIFormInstanceObject()
            {
                _uiFormAsset = null;
                _uiFormHelper = null;
            }

            public static UIFormInstanceObject Create(string name, object uiFormAsset, object uiFormInstance,
                IUIFormHelper uiFormHelper)
            {
                if (uiFormAsset == null)
                {
                    throw new LFrameworkException("UI form asset is invalid.");
                }

                if (uiFormHelper == null)
                {
                    throw new LFrameworkException("UI form helper is invalid.");
                }

                UIFormInstanceObject uiFormInstanceObject = ReferencePool.Acquire<UIFormInstanceObject>();
                uiFormInstanceObject.Initialize(name, uiFormInstance);
                uiFormInstanceObject._uiFormAsset = uiFormAsset;
                uiFormInstanceObject._uiFormHelper = uiFormHelper;
                return uiFormInstanceObject;
            }

            public override void Clear()
            {
                base.Clear();
                _uiFormAsset = null;
                _uiFormHelper = null;
            }

            protected internal override void Release(bool isShutdown)
            {
                _uiFormHelper.ReleaseUIForm(_uiFormAsset, Target);
            }
        }
    }
}