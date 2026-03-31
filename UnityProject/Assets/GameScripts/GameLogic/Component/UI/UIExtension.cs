using JetBrains.Annotations;
using LFramework;

namespace GameLogic
{
    public static partial class UIExtension
    {
        public static bool HasUIForm(this IUIManager uiComponent, int uiFormId, string uiGroupName = null)
        {
            var cfgUIForm = GameEntry.DataTable.TbUIForm.Get(uiFormId);
            if (cfgUIForm == null)
            {
                return false;
            }
            
            string assetName = AssetUtility.GetUIFormAsset(cfgUIForm.AssetName);
            if (string.IsNullOrEmpty(uiGroupName))
            {
                return uiComponent.HasUIForm(assetName);
            }
            
            IUIGroup uiGroup = uiComponent.GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                return false;
            }
            
            return uiGroup.HasUIForm(assetName);
        }
        
        [CanBeNull]
        public static UIForm GetUIForm(this IUIManager uiComponent, int uiFormId, string uiGroupName = null)
        {
            var cfgUIForm = GameEntry.DataTable.TbUIForm.Get(uiFormId);
            if (cfgUIForm == null)
            {
                return null;
            }
            
            string assetName = AssetUtility.GetUIFormAsset(cfgUIForm.AssetName);
            UIForm uiForm = null;
            if (string.IsNullOrEmpty(uiGroupName))
            {
                uiForm = uiComponent.GetUIForm(assetName);
                if (uiForm == null)
                {
                    return null;
                }
            
                return uiForm;
            }
            
            IUIGroup uiGroup = uiComponent.GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                return null;
            }
            
            uiForm = (UIForm)uiGroup.GetUIForm(assetName);
            if (uiForm == null)
            {
                return null;
            }
            
            return uiForm;
        }
        
        public static void CloseUIForm(this UIComponent uiComponent, UguiForm uiForm)
        {
            uiComponent.CloseUIForm(uiForm.UIForm);
        }
        
        public static int? OpenUIForm(this IUIManager uiComponent, int uiFormId, object userData = null)
        {
            var drUIForm = GameEntry.DataTable.TbUIForm.Get(uiFormId);
            if (drUIForm == null)
            {
                Log.Warning("Can not load UI form '{0}' from data table.", uiFormId.ToString());
                return null;
            }
            
            string assetName = AssetUtility.GetUIFormAsset(drUIForm.AssetName);
            if (!drUIForm.AllowMultiInstance)
            {
                if (uiComponent.IsLoadingUIForm(assetName))
                {
                    return null;
                }
            
                if (uiComponent.HasUIForm(assetName))
                {
                    return null;
                }
            }

            return uiComponent.OpenUIForm(assetName, drUIForm.GroupName.ToString(), Constant.AssetPriority.UIFormAsset,
                drUIForm.PauseCoveredForm, userData);
        }
    }
}