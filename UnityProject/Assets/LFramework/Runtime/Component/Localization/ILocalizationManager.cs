namespace LFramework
{
    /// <summary>
    /// 本地化管理器接口。
    /// </summary>
    public interface ILocalizationManager
    {
        /// <summary>
        /// 获取或设置本地化语言。
        /// </summary>
        Language Language { get; set; }

        /// <summary>
        /// 获取系统语言。
        /// </summary>
        Language SystemLanguage { get; }
    }
}