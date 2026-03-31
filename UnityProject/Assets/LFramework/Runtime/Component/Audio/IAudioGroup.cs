using UnityEngine.Audio;

namespace LFramework
{
    /// <summary>
    /// 声音组接口。
    /// </summary>
    public interface IAudioGroup
    {
        /// <summary>
        /// 获取声音组名称。
        /// </summary>
        string AudioGroupName { get; }

        /// <summary>
        /// 获取声音代理数。
        /// </summary>
        int AudioAgentCount { get; }

        /// <summary>
        /// 获取或设置声音组中的声音是否避免被同优先级声音替换。
        /// </summary>
        bool AvoidBeingReplacedBySamePriority { get; set; }

        /// <summary>
        /// 获取或设置声音组静音。
        /// </summary>
        bool Mute { get; set; }

        /// <summary>
        /// 获取或设置声音组音量。
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        void StopAllLoadedAudios();

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        void StopAllLoadedAudios(float fadeOutSeconds);
    }
}