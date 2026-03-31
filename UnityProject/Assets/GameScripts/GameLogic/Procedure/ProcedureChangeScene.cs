using System.Threading.Tasks;
using LFramework;

namespace GameLogic
{
    /// <summary>
    /// 场景切换流程，负责卸载旧场景、加载目标场景并在完成后切入对应业务流程。
    /// </summary>
    public class ProcedureChangeScene : ProcedureBase
    {
        private string _changeToSceneName = null;
        private int _loadingId;
        private LoadingForm _loadingForm;
        private bool _isChangeSceneComplete = false;
        
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _changeToSceneName = null;
            _loadingId = 0;
            _loadingForm = null;
            _isChangeSceneComplete = false;
            
            // 切场景前先停止声音，避免旧场景音频残留。
            GameEntry.Audio.StopAllLoadingAudios();
            GameEntry.Audio.StopAllLoadedAudios();
            
            // 卸载当前已加载场景，为目标场景腾出运行环境。
            var loadedSceneAssetNames = GameEntry.Scene.GetLoadedSceneAssetNames();
            for (int i = 0; i < loadedSceneAssetNames.Length; i++)
            {
                GameEntry.Scene.UnloadScene(loadedSceneAssetNames[i]);
            }
            
            // 切场景前恢复时间缩放，避免新场景沿用旧状态。
            GameEntry.Base.ResetNormalGameSpeed();
            
            _changeToSceneName = procedureOwner.GetData<VarString>(Constant.Setting.ChangeSceneNameKey);
            if (_changeToSceneName == null)
            {
                Log.Warning("Can not load scene '{0}' from data table.", _changeToSceneName);
                return;
            }
            
            GameEntry.Scene.LoadScene(AssetUtility.GetSceneAsset(_changeToSceneName), OnLoadSceneProgress, OnLoadSceneSuccess);
            
            // 打开 Loading 界面，用来承接切场景期间的进度展示。
            OpenLoadingForm(procedureOwner);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            
            _loadingId = 0;
            _loadingForm = null;
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!_isChangeSceneComplete)
            {
                return;
            }

            switch (_changeToSceneName)
            {
                case "Menu":
                    // 菜单场景加载完成后进入菜单流程。
                    ChangeState<ProcedureMenu>(procedureOwner);
                    break;
                case "Game":
                    // 游戏场景加载完成后进入游戏流程。
                    ChangeState<ProcedureGame>(procedureOwner);
                    break;
            }
        }

        private async void OpenLoadingForm(IFsm<IProcedureManager> procedureOwner)
        {
            _loadingId = await GameEntry.UI.OpenUIFormAwait(AssetUtility.GetUIFormAsset("LoadingForm"), Constant.Setting.UIGroupTop,
                10, false, null);
            procedureOwner.SetData<VarInt32>(Constant.Setting.ChangeSceneFormKey, _loadingId);
            _loadingForm = (LoadingForm)GameEntry.UI.GetUIForm(_loadingId).Logic;
        }

        private void OnLoadSceneProgress(float progress)
        {
            if (_loadingForm != null)
            {
                _loadingForm.SetProgress(progress);
            }
        }

        private void OnLoadSceneSuccess(bool isSuccess)
        {
            if (isSuccess)
            {
                _isChangeSceneComplete = true;
            }
            else
            {
                Log.Error("Load scene '{0}' failure.", _changeToSceneName);
            }
        }
    }
}