using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace LFramework
{
    /// <summary>
    /// 声音组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("LFramework/Audio")]
    public sealed partial class AudioComponent : MonoBehaviour, ILFrameworkModule, IAudioManager, IAudioRelease
    {
        [SerializeField]
        private AudioMixer audioMixer = null;

        private const int DefaultPriority = 0;
        private AudioListener _audioListener = null;
        
        private Dictionary<string, AudioGroup> _audioGroups;
        private List<int> _audiosBeingLoaded;
        private HashSet<int> _audiosToReleaseOnLoad;
        private LoadAssetCallbacks _loadAssetCallbacks;
        private IResourceManager _resourceManager;
        private int _serial;
        
        /// <summary>
        /// 获取声音组数量。
        /// </summary>
        public int AudioGroupCount
        {
            get
            {
                return _audioGroups.Count;
            }
        }

        public AudioMixer AudioMixer
        {
            get
            {
                return audioMixer;
            }
        }

        /// <summary>
        /// 获取正在加载的声音数量。
        /// </summary>
        public int LoadingAudioCount
        {
            get
            {
                return _audiosBeingLoaded.Count;
            }
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }

        private void Awake()
        {
            LFrameworkEntry.RegisterModule<IAudioManager>(this);
        }

        private void Start()
        {
            _resourceManager = LFrameworkEntry.GetModule<IResourceManager>();
        }

        public void OnInit()
        {
            _audioListener = gameObject.GetOrAddComponent<AudioListener>();
            
            _audioGroups = new Dictionary<string, AudioGroup>(StringComparer.Ordinal);
            _audiosBeingLoaded = new List<int>();
            _audiosToReleaseOnLoad = new HashSet<int>();
            _loadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback);
            _resourceManager = null;
            _serial = 0;
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理声音管理器。
        /// </summary>
        public void Shutdown()
        {
            StopAllLoadedAudios();
            _audioGroups.Clear();
            _audiosBeingLoaded.Clear();
            _audiosToReleaseOnLoad.Clear();
        }

        /// <summary>
        /// 是否存在指定声音组。
        /// </summary>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <returns>指定声音组是否存在。</returns>
        public bool HasAudioGroup(string audioGroupName)
        {
            if (string.IsNullOrEmpty(audioGroupName))
            {
                throw new LFrameworkException("Audio group name is invalid.");
            }

            return _audioGroups.ContainsKey(audioGroupName);
        }

        /// <summary>
        /// 获取指定声音组。
        /// </summary>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <returns>要获取的声音组。</returns>
        public IAudioGroup GetAudioGroup(string audioGroupName)
        {
            if (string.IsNullOrEmpty(audioGroupName))
            {
                throw new LFrameworkException("Audio group name is invalid.");
            }

            AudioGroup audioGroup = null;
            if (_audioGroups.TryGetValue(audioGroupName, out audioGroup))
            {
                return audioGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <returns>所有声音组。</returns>
        public IAudioGroup[] GetAllAudioGroups()
        {
            int index = 0;
            IAudioGroup[] results = new IAudioGroup[_audioGroups.Count];
            foreach (KeyValuePair<string, AudioGroup> audioGroup in _audioGroups)
            {
                results[index++] = audioGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <param name="results">所有声音组。</param>
        public void GetAllAudioGroups(List<IAudioGroup> results)
        {
            if (results == null)
            {
                throw new LFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, AudioGroup> audioGroup in _audioGroups)
            {
                results.Add(audioGroup.Value);
            }
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="audioAgentHelperCount">声音代理辅助器数量。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddAudioGroup(string audioGroupName, int audioAgentHelperCount)
        {
            return AddAudioGroup(audioGroupName, false, AudioConstant.DefaultMute, AudioConstant.DefaultVolume,
                audioAgentHelperCount);
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="audioGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换。</param>
        /// <param name="audioGroupMute">声音组是否静音。</param>
        /// <param name="audioGroupVolume">声音组音量。</param>
        /// <param name="audioAgentHelperCount">声音代理辅助器数量。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddAudioGroup(string audioGroupName, bool audioGroupAvoidBeingReplacedBySamePriority,
            bool audioGroupMute, float audioGroupVolume, int audioAgentHelperCount)
        {
            if (string.IsNullOrEmpty(audioGroupName))
            {
                throw new LFrameworkException("Audio group name is invalid.");
            }

            if (HasAudioGroup(audioGroupName))
            {
                return false;
            }

            var audioGroup = new GameObject().AddComponent<AudioGroup>();
            audioGroup.name = Utility.Text.Format("Audio Group - {0}", audioGroupName);
            Transform tempTrans = audioGroup.transform;
            tempTrans.SetParent(gameObject.transform);
            tempTrans.localScale = Vector3.one;
            
            if (audioMixer != null)
            {
                AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}", audioGroupName));
                if (audioMixerGroups.Length > 0)
                {
                    audioGroup.AudioMixerGroup = audioMixerGroups[0];
                }
                else
                {
                    Log.Warning("Can not find audio mixer group 'Master/{0}'.", audioGroupName);
                }
            }
            audioGroup.AudioGroupName = audioGroupName;
            audioGroup.AvoidBeingReplacedBySamePriority = audioGroupAvoidBeingReplacedBySamePriority;
            audioGroup.Mute = audioGroupMute;
            audioGroup.Volume = audioGroupVolume;
            
            for (int i = 0; i < audioAgentHelperCount; i++)
            {
                audioGroup.AddAudioAgentHelper(this, audioMixer, i);
            }

            _audioGroups.Add(audioGroupName, audioGroup);

            return true;
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <returns>所有正在加载声音的序列编号。</returns>
        public int[] GetAllLoadingAudioSerialIds()
        {
            return _audiosBeingLoaded.ToArray();
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载声音的序列编号。</param>
        public void GetAllLoadingAudioSerialIds(List<int> results)
        {
            if (results == null)
            {
                throw new LFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(_audiosBeingLoaded);
        }

        /// <summary>
        /// 是否正在加载声音。
        /// </summary>
        /// <param name="serialId">声音序列编号。</param>
        /// <returns>是否正在加载声音。</returns>
        public bool IsLoadingAudio(int serialId)
        {
            return _audiosBeingLoaded.Contains(serialId);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlayAudio(string audioAssetName, string audioGroupName)
        {
            return PlayAudio(audioAssetName, audioGroupName, ResourceConstant.DefaultPriority, null, null, Vector3.zero,
                null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlayAudio(string audioAssetName, string audioGroupName, int priority)
        {
            return PlayAudio(audioAssetName, audioGroupName, priority, null, null, Vector3.zero, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlayAudio(string audioAssetName, string audioGroupName, PlayAudioParams playAudioParams)
        {
            return PlayAudio(audioAssetName, audioGroupName, ResourceConstant.DefaultPriority, playAudioParams, null,
                Vector3.zero, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="bindingTrans">声音绑定的Transform。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlayAudio(string audioAssetName, string audioGroupName, Transform bindingTrans)
        {
            return PlayAudio(audioAssetName, audioGroupName, ResourceConstant.DefaultPriority, null, bindingTrans,
                Vector3.zero, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlayAudio(string audioAssetName, string audioGroupName, Vector3 worldPosition)
        {
            return PlayAudio(audioAssetName, audioGroupName, ResourceConstant.DefaultPriority, null, null,
                worldPosition, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlayAudio(string audioAssetName, string audioGroupName, object userData)
        {
            return PlayAudio(audioAssetName, audioGroupName, ResourceConstant.DefaultPriority, null, null, Vector3.zero,
                userData);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlayAudio(string audioAssetName, string audioGroupName, int priority,
            PlayAudioParams playAudioParams)
        {
            return PlayAudio(audioAssetName, audioGroupName, priority, playAudioParams, null, Vector3.zero, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlayAudio(string audioAssetName, string audioGroupName, int priority,
            PlayAudioParams playAudioParams, object userData)
        {
            return PlayAudio(audioAssetName, audioGroupName, priority, playAudioParams, null, Vector3.zero, userData);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <param name="bindingTrans">声音绑定的Transform。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlayAudio(string audioAssetName, string audioGroupName, int priority,
            PlayAudioParams playAudioParams, Transform bindingTrans)
        {
            return PlayAudio(audioAssetName, audioGroupName, priority, playAudioParams, bindingTrans, Vector3.zero,
                null);
        }

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
        public int PlayAudio(string audioAssetName, string audioGroupName, int priority,
            PlayAudioParams playAudioParams, Transform bindingTrans, object userData)
        {
            return PlayAudio(audioAssetName, audioGroupName, priority, playAudioParams, bindingTrans, Vector3.zero,
                userData);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlayAudio(string audioAssetName, string audioGroupName, int priority,
            PlayAudioParams playAudioParams, Vector3 worldPosition)
        {
            return PlayAudio(audioAssetName, audioGroupName, priority, playAudioParams, null, worldPosition, null);
        }

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
        public int PlayAudio(string audioAssetName, string audioGroupName, int priority,
            PlayAudioParams playAudioParams, Vector3 worldPosition, object userData)
        {
            return PlayAudio(audioAssetName, audioGroupName, priority, playAudioParams, null, worldPosition, userData);
        }
        
        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopAudio(int serialId)
        {
            return StopAudio(serialId, AudioConstant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopAudio(int serialId, float fadeOutSeconds)
        {
            if (IsLoadingAudio(serialId))
            {
                _audiosToReleaseOnLoad.Add(serialId);
                return true;
            }

            foreach (KeyValuePair<string, AudioGroup> audioGroup in _audioGroups)
            {
                if (audioGroup.Value.StopAudio(serialId, fadeOutSeconds))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        public void StopAllLoadedAudios()
        {
            StopAllLoadedAudios(AudioConstant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void StopAllLoadedAudios(float fadeOutSeconds)
        {
            foreach (KeyValuePair<string, AudioGroup> audioGroup in _audioGroups)
            {
                audioGroup.Value.StopAllLoadedAudios(fadeOutSeconds);
            }
        }

        /// <summary>
        /// 停止所有正在加载的声音。
        /// </summary>
        public void StopAllLoadingAudios()
        {
            foreach (int serialId in _audiosBeingLoaded)
            {
                _audiosToReleaseOnLoad.Add(serialId);
            }
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        public void PauseAudio(int serialId)
        {
            PauseAudio(serialId, AudioConstant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void PauseAudio(int serialId, float fadeOutSeconds)
        {
            if (IsLoadingAudio(serialId))
            {
                return;
            }

            foreach (KeyValuePair<string, AudioGroup> audioGroup in _audioGroups)
            {
                if (audioGroup.Value.PauseAudio(serialId, fadeOutSeconds))
                {
                    return;
                }
            }

            throw new LFrameworkException(Utility.Text.Format("Can not find audio '{0}'.", serialId));
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        public void ResumeAudio(int serialId)
        {
            ResumeAudio(serialId, AudioConstant.DefaultFadeInSeconds);
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void ResumeAudio(int serialId, float fadeInSeconds)
        {
            if (IsLoadingAudio(serialId))
            {
                return;
            }

            foreach (KeyValuePair<string, AudioGroup> audioGroup in _audioGroups)
            {
                if (audioGroup.Value.ResumeAudio(serialId, fadeInSeconds))
                {
                    return;
                }
            }

            throw new LFrameworkException(Utility.Text.Format("Can not find audio '{0}'.", serialId));
        }
        
        /// <summary>
        /// 释放声音资源。
        /// </summary>
        /// <param name="audioAsset">要释放的声音资源。</param>
        public void ReleaseAudioAsset(object audioAsset)
        {
            _resourceManager.UnloadAsset(audioAsset);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="audioAssetName">声音资源名称。</param>
        /// <param name="audioGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <param name="bindingTrans">声音绑定的Transform。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        private int PlayAudio(string audioAssetName, string audioGroupName, int priority,
            PlayAudioParams playAudioParams, Transform bindingTrans, Vector3 worldPosition, object userData)
        {
            if (_resourceManager == null)
            {
                throw new LFrameworkException("You must set resource manager first.");
            }

            if (playAudioParams == null)
            {
                playAudioParams = PlayAudioParams.Create();
            }

            int serialId = ++_serial;
            PlayAudioErrorCode? errorCode = null;
            string errorMessage = null;
            AudioGroup audioGroup = (AudioGroup)GetAudioGroup(audioGroupName);
            if (audioGroup == null)
            {
                errorCode = PlayAudioErrorCode.AudioGroupNotExist;
                errorMessage = Utility.Text.Format("Audio group '{0}' is not exist.", audioGroupName);
            }
            else if (audioGroup.AudioAgentCount <= 0)
            {
                errorCode = PlayAudioErrorCode.AudioGroupHasNoAgent;
                errorMessage = Utility.Text.Format("Audio group '{0}' is have no audio agent.", audioGroupName);
            }

            if (errorCode.HasValue)
            {
                LogPlayAudioFailure(errorCode.Value, audioGroupName, audioAssetName, errorMessage);

                if (playAudioParams.Referenced)
                {
                    ReferencePool.Release(playAudioParams);
                }

                return serialId;
            }

            _audiosBeingLoaded.Add(serialId);
            _resourceManager.LoadAsset(audioAssetName, priority, _loadAssetCallbacks,
                PlayAudioInfo.Create(serialId, audioGroup, playAudioParams, bindingTrans, worldPosition, userData));
            return serialId;
        }

        private void LoadAssetSuccessCallback(string audioAssetName, object audioAsset, float duration, object userData)
        {
            PlayAudioInfo playAudioInfo = (PlayAudioInfo)userData;
            if (playAudioInfo == null)
            {
                throw new LFrameworkException("Play audio info is invalid.");
            }

            if (_audiosToReleaseOnLoad.Contains(playAudioInfo.SerialId))
            {
                RemoveLoadingAudio(playAudioInfo.SerialId);
                ReleasePlayAudioInfo(playAudioInfo);
                ReleaseAudioAsset(audioAsset);
                return;
            }

            RemoveLoadingAudio(playAudioInfo.SerialId);

            PlayAudioErrorCode? errorCode = null;
            IAudioAgent audioAgent = playAudioInfo.AudioGroup.PlayAudio(playAudioInfo.SerialId, audioAsset,
                playAudioInfo.PlayAudioParams, playAudioInfo.BindingTrans, playAudioInfo.WorldPosition, out errorCode);
            if (audioAgent != null)
            {
                ReleasePlayAudioInfo(playAudioInfo);
                return;
            }

            ReleaseAudioAsset(audioAsset);
            LogPlayAudioFailure(errorCode ?? PlayAudioErrorCode.Unknown, playAudioInfo.AudioGroup.AudioGroupName,
                audioAssetName, Utility.Text.Format("Audio group '{0}' play audio '{1}' failure.",
                    playAudioInfo.AudioGroup.AudioGroupName, audioAssetName));
            ReleasePlayAudioInfo(playAudioInfo);
        }

        private void LoadAssetFailureCallback(string audioAssetName, LoadResourceStatus status, string errorMessage,
            object userData)
        {
            PlayAudioInfo playAudioInfo = (PlayAudioInfo)userData;
            if (playAudioInfo == null)
            {
                throw new LFrameworkException("Play audio info is invalid.");
            }

            if (_audiosToReleaseOnLoad.Contains(playAudioInfo.SerialId))
            {
                RemoveLoadingAudio(playAudioInfo.SerialId);
                ReleasePlayAudioInfo(playAudioInfo);
                return;
            }

            RemoveLoadingAudio(playAudioInfo.SerialId);
            string appendErrorMessage =
                Utility.Text.Format("Load audio failure, asset name '{0}', status '{1}', error message '{2}'.",
                    audioAssetName, status, errorMessage);
            LogPlayAudioFailure(PlayAudioErrorCode.LoadAssetFailure, playAudioInfo.AudioGroup.AudioGroupName,
                audioAssetName, appendErrorMessage);
            ReleasePlayAudioInfo(playAudioInfo);
        }

        private void RemoveLoadingAudio(int serialId)
        {
            _audiosBeingLoaded.Remove(serialId);
            _audiosToReleaseOnLoad.Remove(serialId);
        }

        private void ReleasePlayAudioInfo(PlayAudioInfo playAudioInfo)
        {
            if (playAudioInfo.PlayAudioParams != null && playAudioInfo.PlayAudioParams.Referenced)
            {
                ReferencePool.Release(playAudioInfo.PlayAudioParams);
            }

            ReferencePool.Release(playAudioInfo);
        }

        private void LogPlayAudioFailure(PlayAudioErrorCode errorCode, string audioGroupName, string audioAssetName,
            string errorMessage)
        {
            Log.Error("Play audio failure, error code '{0}', audio group '{1}', asset '{2}', detail '{3}'.",
                errorCode.ToString(), audioGroupName, audioAssetName, errorMessage);
        }
    }
}