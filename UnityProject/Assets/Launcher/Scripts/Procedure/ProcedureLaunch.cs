using System;
using LFramework;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    /// <summary>
    /// 启动器入口流程，负责初始化热更界面、语言和基础音频，然后进入资源启动链路。
    /// </summary>
    public class ProcedureLaunch : ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return true;
            }
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Log.Info("框架流程启动！");
            
            // 初始化启动器界面，后续资源更新、下载、提示都依赖这里的 UI。
            LauncherMgr.Initialize();

            // 初始化启动阶段语言，保证更新文案能按当前语言显示。
            InitLocalization();

            // 初始化默认音频组，给启动阶段和后续流程提供基础音频通道。
            InitSound();
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            
            // 启动器入口只做一次性准备，下一帧立刻交给启动过渡流程。
            ChangeState<ProcedureSplash>(procedureOwner);
        }

        private void InitLocalization()
        {
            var resComponent = LFrameworkEntry.GetModule<IResourceManager>();
            var playMode = resComponent.ResourceMode;
            var localizationComponent = LFrameworkEntry.GetModule<ILocalizationManager>();
            if (playMode == ResourceMode.EditorSimulate)
            {
                if (localizationComponent.Language == Language.Unspecified)
                {
                    localizationComponent.Language = localizationComponent.SystemLanguage;
                }
                return;
            }
            
            Language language = localizationComponent.Language;
            var settingComponent = LFrameworkEntry.GetModule<ISettingManager>();
            if (settingComponent.HasSetting("Language"))
            {
                try
                {
                    string languageString = settingComponent.GetString("Language");
                    language = (Language)System.Enum.Parse(typeof(Language), languageString);
                }
                catch (Exception e)
                {
                    Log.Error("Init language error, reason {0}",e.ToString());
                }
            }
            
            if (language != Language.English
                && language != Language.ChineseSimplified
                && language != Language.ChineseTraditional)
            {
                // 若是暂不支持的语言，则使用英语
                language = Language.English;
            
                settingComponent.SetString("Language", language.ToString());
                settingComponent.Save();
            }
            
            localizationComponent.Language = language;
            Log.Info("Init language settings complete, current language is '{0}'.", language.ToString());
        }
        
        private void InitSound()
        {
            var soundComponent = LFrameworkEntry.GetModule<IAudioManager>();
            soundComponent.AddAudioGroup("Default", 3);
        }
    }
}