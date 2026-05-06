using LFramework;

namespace GameLogic
{
    /// <summary>
    /// 热更游戏逻辑入口流程，初始化音频、UI 和本地化后进入登录阶段。
    /// </summary>
    public class ProcedureGameLogicLaunch : ProcedureBase
    {
        private bool _isInitOver = false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _isInitOver = false;

            // 重新建立游戏侧音频组，和启动器阶段的默认音频职责分离。
            InitAudio();

            // 初始化正式游戏所需的 UI 分组。
            InitUI();

            // 加载游戏本地化资源，完成后才允许进入登录界面。
            InitLocalization();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!_isInitOver)
            {
                return;
            }

            // 游戏基础组件就绪后，正式进入登录流程。
            ChangeState<ProcedureLogin>(procedureOwner);
        }

        private void InitAudio()
        {
            foreach (var groupInfo in Constant.Setting.AudioGroupDict)
            {
                GameEntry.Audio.AddAudioGroup(groupInfo.Key, groupInfo.Value);
            }
        }

        private void InitUI()
        {
            for (int i = 0, len = Constant.Setting.UIGroupNames.Length; i < len; i++)
            {
                GameEntry.UI.AddUIGroup(Constant.Setting.UIGroupNames[i], i);
            }
        }

        private async void InitLocalization()
        {
            await GameEntry.Localization.LoadLanguageTotalAsset(LocalizationUtility.LocalizationAssetPath);
            GameEntry.Localization.SetLanguage(GameEntry.Localization.Language);
            _isInitOver = true;
        }
    }
}