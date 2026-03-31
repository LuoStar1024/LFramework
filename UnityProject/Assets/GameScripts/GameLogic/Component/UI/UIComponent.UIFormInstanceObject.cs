using LFramework;

namespace GameLogic
{
    public sealed partial class UIComponent
    {
        /// <summary>
        /// 界面实例对象。
        /// </summary>
        private sealed class UIFormInstanceObject : ObjectBase
        {
            private object _uiFormAsset;
            private IUIRelease _uiRelease;

            public UIFormInstanceObject()
            {
                _uiFormAsset = null;
                _uiRelease = null;
            }

            public static UIFormInstanceObject Create(string name, object uiFormAsset, object uiFormInstance,
                IUIRelease uiFormHelper)
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
                uiFormInstanceObject._uiRelease = uiFormHelper;
                return uiFormInstanceObject;
            }

            public override void Clear()
            {
                base.Clear();
                _uiFormAsset = null;
                _uiRelease = null;
            }

            protected override void Release(bool isShutdown)
            {
                _uiRelease.ReleaseUIForm(_uiFormAsset, Target);
            }
        }
    }
}