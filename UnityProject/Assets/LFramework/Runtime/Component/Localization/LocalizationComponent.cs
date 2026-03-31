using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using LFramework.Localization;
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
        [SerializeField]
        private List<string> allLanguage = new List<string>();
        
        private Language _language;
        private LanguageSource _languageSource;
        private IResourceManager _resourceManager;

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

                if (_language == Language.Unspecified)
                {
                    _language = value;
                }
                else
                {
                    SetLanguage(value);
                }
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
        
        private LanguageSourceData SourceData
        {
            get
            {
                if (_languageSource == null)
                {
                    _languageSource = gameObject.AddComponent<LanguageSource>();
                }

                return _languageSource.SourceData;
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
            _resourceManager = LFrameworkEntry.GetModule<IResourceManager>();
        }

        public void OnInit()
        {
            _language = Language.Unspecified;
            _resourceManager = null;
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
        
        /// <summary>
        /// 加载完整的语言资源包。
        /// </summary>
        /// <param name="assetName">要加载的资源包名称</param>
        public async UniTask LoadLanguageTotalAsset(string assetName)
        {
#if UNITY_EDITOR
            if (_resourceManager.ResourceMode == ResourceMode.EditorSimulate)
            {
                Localization.LocalizationManager.RegisterSourceInEditor();
                UpdateAllLanguages();
                SourceData.Awake();
                return;
            }
#endif
            
            SourceData.Awake();
            TextAsset assetTextAsset = await _resourceManager.LoadAsset<TextAsset>(assetName, 10);

            if (assetTextAsset == null)
            {
                Log.Warning($"没有加载到语言总表");
                return;
            }

            Log.Info($"加载语言总表成功");

            UseLocalizationCsv(assetTextAsset.text, true);
        }

        // /// <summary>
        // /// 加载语言分表。
        // /// </summary>
        // /// <param name="language">语言类型。</param>
        // /// <param name="setCurrent">是否立刻设置成当前语言。</param>
        // /// <param name="fromInit">是否初始化Inner语言。</param>
        // public async UniTask LoadLanguage(string language, bool setCurrent = false, bool fromInit = false)
        // {
        //     TextAsset assetTextAsset = await _resourceManager.LoadAsset<TextAsset>(language, 10);
        //
        //     if (assetTextAsset == null)
        //     {
        //         Log.Warning($"没有加载到语言总表");
        //         return;
        //     }
        //
        //     Log.Info($"加载语言总表成功");
        //
        //     UseLocalizationCSV(assetTextAsset.text, true);
        // }
        
        /// <summary>
        /// 检查指定语言是否可用。
        /// </summary>
        /// <param name="language">要检查的语言名称。</param>
        /// <returns>如果语言可用返回true，否则false。</returns>
        public bool CheckLanguage(Language language)
        {
            return allLanguage.Contains(language.ToString());
        }

        /// <summary>
        /// 设置当前语言（通过枚举值）。
        /// </summary>
        /// <param name="language">要设置的语言枚举值。</param>
        /// <param name="load">是否立即加载语言资源。</param>
        /// <returns>设置是否成功。</returns>
        public bool SetLanguage(Language language, bool load = false)
        {
            if (!CheckLanguage(language))
            {
                Log.Warning($"当前没有这个语言无法切换到此语言 {language}");
                return false;
            }

            // if (_language == language)
            // {
            //     return true;
            // }

            Log.Info($"设置当前语言 = {language}");
            Localization.LocalizationManager.CurrentLanguage = language.ToString();
            _language = language;
            return true;
        }
        
        private void UseLocalizationCsv(string text, bool isLocalizeAll = false)
        {
            SourceData.Import_CSV(string.Empty, text, eSpreadsheetUpdateMode.Merge, ',');
            if (isLocalizeAll)
            {
                Localization.LocalizationManager.LocalizeAll();
            }

            UpdateAllLanguages();
        }
        
        /// <summary>
        /// 检查并初始化所有语言的Id。
        /// </summary>
        private void UpdateAllLanguages()
        {
            allLanguage.Clear();
            List<string> allLanguages = Localization.LocalizationManager.GetAllLanguages();
            foreach (var language in allLanguages)
            {
                var newLanguage = Regex.Replace(language, @"[\r\n]", "");
                allLanguage.Add(newLanguage);
            }
        }
    }
}