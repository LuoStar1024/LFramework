using System.Collections.Generic;
using UnityEngine;

namespace LFramework
{
    /// <summary>
    /// 声音管理器接口。
    /// </summary>
    public interface IAudioManager
    {
        /// <summary>
        /// 获取声音组数量。
        /// </summary>
        int AudioGroupCount { get; }
        
        /// <summary>
        /// 是否存在指定声音组。
        /// </summary>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <returns>指定声音组是否存在。</returns>
        bool HasAudioGroup(string audioGroupName);

        /// <summary>
        /// 获取指定声音组。
        /// </summary>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <returns>要获取的声音组。</returns>
        IAudioGroup GetAudioGroup(string audioGroupName);

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <returns>所有声音组。</returns>
        IAudioGroup[] GetAllAudioGroups();

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <param name="results">所有声音组。</param>
        void GetAllAudioGroups(List<IAudioGroup> results);

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="audioAgentHelperCount">声音代理辅助器数量。</param>
        /// <returns>是否增加声音组成功。</returns>
        bool AddAudioGroup(string audioGroupName, int audioAgentHelperCount);

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="audioGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换。</param>
        /// <param name="audioGroupMute">声音组是否静音。</param>
        /// <param name="audioGroupVolume">声音组音量。</param>
        /// <param name="audioAgentHelperCount">声音代理辅助器数量。</param>
        /// <returns>是否增加声音组成功。</returns>
        bool AddAudioGroup(string audioGroupName, bool audioGroupAvoidBeingReplacedBySamePriority,
            bool audioGroupMute, float audioGroupVolume, int audioAgentHelperCount);

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <returns>所有正在加载声音的序列编号。</returns>
        int[] GetAllLoadingAudioSerialIds();

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载声音的序列编号。</param>
        void GetAllLoadingAudioSerialIds(List<int> results);

        /// <summary>
        /// 是否正在加载声音。
        /// </summary>
        /// <param name="serialId">声音序列编号。</param>
        /// <returns>是否正在加载声音。</returns>
        bool IsLoadingAudio(int serialId);

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName);

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName, int priority);

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName, PlayAudioParams playAudioParams);

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="bindingTrans">声音绑定的Transform。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName, Transform bindingTrans);

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName, Vector3 worldPosition);
        
        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName, object userData);

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName, int priority, PlayAudioParams playAudioParams);

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName, int priority, PlayAudioParams playAudioParams,
            object userData);

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <param name="bindingTrans">声音绑定的Transform。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName, int priority, PlayAudioParams playAudioParams,
            Transform bindingTrans);

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <param name="bindingTrans">声音绑定的Transform。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName, int priority, PlayAudioParams playAudioParams,
            Transform bindingTrans, object userData);

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName, int priority, PlayAudioParams playAudioParams,
            Vector3 worldPosition);

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        int PlayAudio(string audioAssetName, string audioGroupName, int priority, PlayAudioParams playAudioParams,
            Vector3 worldPosition, object userData);
        
        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <returns>是否停止播放声音成功。</returns>
        bool StopAudio(int serialId);

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否停止播放声音成功。</returns>
        bool StopAudio(int serialId, float fadeOutSeconds);

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        void StopAllLoadedAudios();

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        void StopAllLoadedAudios(float fadeOutSeconds);

        /// <summary>
        /// 停止所有正在加载的声音。
        /// </summary>
        void StopAllLoadingAudios();

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        void PauseAudio(int serialId);

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        void PauseAudio(int serialId, float fadeOutSeconds);

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        void ResumeAudio(int serialId);

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        void ResumeAudio(int serialId, float fadeInSeconds);
    }
}