using System;
using System.Collections.Generic;
using UnityEngine;

namespace LFramework
{
    /// <summary>
    /// 本地化组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("LFramework/Localization")]
    public sealed partial class LocalizationComponent : MonoBehaviour, ILFrameworkModule, ILocalizationManager
    {
        private Language _language;

        /// <summary>
        /// 获取或设置本地化语言。
        /// </summary>
        public Language Language
        {
            get { return _language; }
            set
            {
                if (value == Language.Unspecified)
                {
                    throw new LFrameworkException("Language is invalid.");
                }

                _language = value;
            }
        }

        /// <summary>
        /// 获取系统语言。
        /// </summary>
        public Language SystemLanguage
        {
            get
            {
                return Application.systemLanguage switch
                {
                    UnityEngine.SystemLanguage.Afrikaans => Language.Afrikaans,
                    UnityEngine.SystemLanguage.Arabic => Language.Arabic,
                    UnityEngine.SystemLanguage.Basque => Language.Basque,
                    UnityEngine.SystemLanguage.Belarusian => Language.Belarusian,
                    UnityEngine.SystemLanguage.Bulgarian => Language.Bulgarian,
                    UnityEngine.SystemLanguage.Catalan => Language.Catalan,
                    UnityEngine.SystemLanguage.Chinese => Language.ChineseSimplified,
                    UnityEngine.SystemLanguage.ChineseSimplified => Language.ChineseSimplified,
                    UnityEngine.SystemLanguage.ChineseTraditional => Language.ChineseTraditional,
                    UnityEngine.SystemLanguage.Czech => Language.Czech,
                    UnityEngine.SystemLanguage.Danish => Language.Danish,
                    UnityEngine.SystemLanguage.Dutch => Language.Dutch,
                    UnityEngine.SystemLanguage.English => Language.English,
                    UnityEngine.SystemLanguage.Estonian => Language.Estonian,
                    UnityEngine.SystemLanguage.Faroese => Language.Faroese,
                    UnityEngine.SystemLanguage.Finnish => Language.Finnish,
                    UnityEngine.SystemLanguage.French => Language.French,
                    UnityEngine.SystemLanguage.German => Language.German,
                    UnityEngine.SystemLanguage.Greek => Language.Greek,
                    UnityEngine.SystemLanguage.Hebrew => Language.Hebrew,
                    UnityEngine.SystemLanguage.Hungarian => Language.Hungarian,
                    UnityEngine.SystemLanguage.Icelandic => Language.Icelandic,
                    UnityEngine.SystemLanguage.Indonesian => Language.Indonesian,
                    UnityEngine.SystemLanguage.Italian => Language.Italian,
                    UnityEngine.SystemLanguage.Japanese => Language.Japanese,
                    UnityEngine.SystemLanguage.Korean => Language.Korean,
                    UnityEngine.SystemLanguage.Latvian => Language.Latvian,
                    UnityEngine.SystemLanguage.Lithuanian => Language.Lithuanian,
                    UnityEngine.SystemLanguage.Norwegian => Language.Norwegian,
                    UnityEngine.SystemLanguage.Polish => Language.Polish,
                    UnityEngine.SystemLanguage.Portuguese => Language.PortuguesePortugal,
                    UnityEngine.SystemLanguage.Romanian => Language.Romanian,
                    UnityEngine.SystemLanguage.Russian => Language.Russian,
                    UnityEngine.SystemLanguage.SerboCroatian => Language.SerboCroatian,
                    UnityEngine.SystemLanguage.Slovak => Language.Slovak,
                    UnityEngine.SystemLanguage.Slovenian => Language.Slovenian,
                    UnityEngine.SystemLanguage.Spanish => Language.Spanish,
                    UnityEngine.SystemLanguage.Swedish => Language.Swedish,
                    UnityEngine.SystemLanguage.Thai => Language.Thai,
                    UnityEngine.SystemLanguage.Turkish => Language.Turkish,
                    UnityEngine.SystemLanguage.Ukrainian => Language.Ukrainian,
                    UnityEngine.SystemLanguage.Unknown => Language.Unspecified,
                    UnityEngine.SystemLanguage.Vietnamese => Language.Vietnamese,
                    _ => Language.Unspecified
                };
            }
        }
        
        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public int Priority
        {
            get { return 0; }
        }
        
        private void Awake()
        {
            LFrameworkEntry.RegisterModule<ILocalizationManager>(this);
        }

        private void Start()
        {
        }

        public void OnInit()
        {
            _language = Language.Unspecified;
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理本地化管理器。
        /// </summary>
        public void Shutdown()
        {
        }
    }
}