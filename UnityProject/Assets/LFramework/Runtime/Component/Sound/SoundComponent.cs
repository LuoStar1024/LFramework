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
    [AddComponentMenu("LFramework/Sound")]
    public sealed partial class SoundComponent : MonoBehaviour, ILFrameworkModule, ISoundManager, ISoundRelease
    {
        [SerializeField]
        private AudioMixer audioMixer = null;

        private const int DefaultPriority = 0;
        private AudioListener _audioListener = null;
        
        private Dictionary<string, SoundGroup> _soundGroups;
        private List<int> _soundsBeingLoaded;
        private HashSet<int> _soundsToReleaseOnLoad;
        private LoadAssetCallbacks _loadAssetCallbacks;
        private IResourceManager _resourceManager;
        private int _serial;
        
        /// <summary>
        /// 获取声音组数量。
        /// </summary>
        public int SoundGroupCount
        {
            get
            {
                return _soundGroups.Count;
            }
        }

        public AudioMixer AudioMixer
        {
            get
            {
                return audioMixer;
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
            LFrameworkEntry.RegisterModule<ISoundManager>(this);
        }

        private void Start()
        {
            _resourceManager = LFrameworkEntry.GetModule<IResourceManager>();
        }

        public void OnInit()
        {
            _audioListener = gameObject.GetOrAddComponent<AudioListener>();
            
            _soundGroups = new Dictionary<string, SoundGroup>(StringComparer.Ordinal);
            _soundsBeingLoaded = new List<int>();
            _soundsToReleaseOnLoad = new HashSet<int>();
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
            StopAllLoadedSounds();
            _soundGroups.Clear();
            _soundsBeingLoaded.Clear();
            _soundsToReleaseOnLoad.Clear();
        }

        /// <summary>
        /// 是否存在指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>指定声音组是否存在。</returns>
        public bool HasSoundGroup(string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new LFrameworkException("Sound group name is invalid.");
            }

            return _soundGroups.ContainsKey(soundGroupName);
        }

        /// <summary>
        /// 获取指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>要获取的声音组。</returns>
        public ISoundGroup GetSoundGroup(string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new LFrameworkException("Sound group name is invalid.");
            }

            SoundGroup soundGroup = null;
            if (_soundGroups.TryGetValue(soundGroupName, out soundGroup))
            {
                return soundGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <returns>所有声音组。</returns>
        public ISoundGroup[] GetAllSoundGroups()
        {
            int index = 0;
            ISoundGroup[] results = new ISoundGroup[_soundGroups.Count];
            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                results[index++] = soundGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <param name="results">所有声音组。</param>
        public void GetAllSoundGroups(List<ISoundGroup> results)
        {
            if (results == null)
            {
                throw new LFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                results.Add(soundGroup.Value);
            }
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundAgentHelperCount">声音代理辅助器数量。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup(string soundGroupName, int soundAgentHelperCount)
        {
            return AddSoundGroup(soundGroupName, false, SoundConstant.DefaultMute, SoundConstant.DefaultVolume,
                soundAgentHelperCount);
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换。</param>
        /// <param name="soundGroupMute">声音组是否静音。</param>
        /// <param name="soundGroupVolume">声音组音量。</param>
        /// <param name="soundAgentHelperCount">声音代理辅助器数量。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup(string soundGroupName, bool soundGroupAvoidBeingReplacedBySamePriority,
            bool soundGroupMute, float soundGroupVolume, int soundAgentHelperCount)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new LFrameworkException("Sound group name is invalid.");
            }

            if (HasSoundGroup(soundGroupName))
            {
                return false;
            }

            var soundGroup = new GameObject().AddComponent<SoundGroup>();
            soundGroup.name = Utility.Text.Format("Sound Group - {0}", soundGroupName);
            Transform tempTrans = soundGroup.transform;
            tempTrans.SetParent(gameObject.transform);
            tempTrans.localScale = Vector3.one;
            
            if (audioMixer != null)
            {
                AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}", soundGroupName));
                if (audioMixerGroups.Length > 0)
                {
                    soundGroup.AudioMixerGroup = audioMixerGroups[0];
                }
                else
                {
                    soundGroup.AudioMixerGroup = audioMixer.FindMatchingGroups("Master")[0];
                }
            }
            soundGroup.SoundGroupName = soundGroupName;
            soundGroup.AvoidBeingReplacedBySamePriority = soundGroupAvoidBeingReplacedBySamePriority;
            soundGroup.Mute = soundGroupMute;
            soundGroup.Volume = soundGroupVolume;
            
            for (int i = 0; i < soundAgentHelperCount; i++)
            {
                soundGroup.AddSoundAgentHelper(this, audioMixer, i);
            }

            _soundGroups.Add(soundGroupName, soundGroup);

            return true;
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <returns>所有正在加载声音的序列编号。</returns>
        public int[] GetAllLoadingSoundSerialIds()
        {
            return _soundsBeingLoaded.ToArray();
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载声音的序列编号。</param>
        public void GetAllLoadingSoundSerialIds(List<int> results)
        {
            if (results == null)
            {
                throw new LFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(_soundsBeingLoaded);
        }

        /// <summary>
        /// 是否正在加载声音。
        /// </summary>
        /// <param name="serialId">声音序列编号。</param>
        /// <returns>是否正在加载声音。</returns>
        public bool IsLoadingSound(int serialId)
        {
            return _soundsBeingLoaded.Contains(serialId);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName)
        {
            return PlaySound(soundAssetName, soundGroupName, ResourceConstant.DefaultPriority, null, null, Vector3.zero,
                null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, null, null, Vector3.zero, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams)
        {
            return PlaySound(soundAssetName, soundGroupName, ResourceConstant.DefaultPriority, playSoundParams, null,
                Vector3.zero, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="bindingTrans">声音绑定的Transform。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, Transform bindingTrans)
        {
            return PlaySound(soundAssetName, soundGroupName, ResourceConstant.DefaultPriority, null, bindingTrans,
                Vector3.zero, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, Vector3 worldPosition)
        {
            return PlaySound(soundAssetName, soundGroupName, ResourceConstant.DefaultPriority, null, null,
                worldPosition, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, ResourceConstant.DefaultPriority, null, null, Vector3.zero,
                userData);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority,
            PlaySoundParams playSoundParams)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, null, Vector3.zero, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority,
            PlaySoundParams playSoundParams, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, null, Vector3.zero, userData);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="bindingTrans">声音绑定的Transform。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority,
            PlaySoundParams playSoundParams, Transform bindingTrans)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, bindingTrans, Vector3.zero,
                null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="bindingTrans">声音绑定的Transform。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority,
            PlaySoundParams playSoundParams, Transform bindingTrans, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, bindingTrans, Vector3.zero,
                userData);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority,
            PlaySoundParams playSoundParams, Vector3 worldPosition)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, null, worldPosition, null);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(string soundAssetName, string soundGroupName, int priority,
            PlaySoundParams playSoundParams, Vector3 worldPosition, object userData)
        {
            return PlaySound(soundAssetName, soundGroupName, priority, playSoundParams, null, worldPosition, userData);
        }
        
        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId)
        {
            return StopSound(serialId, SoundConstant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId, float fadeOutSeconds)
        {
            if (IsLoadingSound(serialId))
            {
                _soundsToReleaseOnLoad.Add(serialId);
                _soundsBeingLoaded.Remove(serialId);
                return true;
            }

            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                if (soundGroup.Value.StopSound(serialId, fadeOutSeconds))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        public void StopAllLoadedSounds()
        {
            StopAllLoadedSounds(SoundConstant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void StopAllLoadedSounds(float fadeOutSeconds)
        {
            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                soundGroup.Value.StopAllLoadedSounds(fadeOutSeconds);
            }
        }

        /// <summary>
        /// 停止所有正在加载的声音。
        /// </summary>
        public void StopAllLoadingSounds()
        {
            foreach (int serialId in _soundsBeingLoaded)
            {
                _soundsToReleaseOnLoad.Add(serialId);
            }
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        public void PauseSound(int serialId)
        {
            PauseSound(serialId, SoundConstant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void PauseSound(int serialId, float fadeOutSeconds)
        {
            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                if (soundGroup.Value.PauseSound(serialId, fadeOutSeconds))
                {
                    return;
                }
            }

            throw new LFrameworkException(Utility.Text.Format("Can not find sound '{0}'.", serialId));
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        public void ResumeSound(int serialId)
        {
            ResumeSound(serialId, SoundConstant.DefaultFadeInSeconds);
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void ResumeSound(int serialId, float fadeInSeconds)
        {
            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                if (soundGroup.Value.ResumeSound(serialId, fadeInSeconds))
                {
                    return;
                }
            }

            throw new LFrameworkException(Utility.Text.Format("Can not find sound '{0}'.", serialId));
        }
        
        /// <summary>
        /// 释放声音资源。
        /// </summary>
        /// <param name="soundAsset">要释放的声音资源。</param>
        public void ReleaseSoundAsset(object soundAsset)
        {
            _resourceManager.UnloadAsset(soundAsset);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetName">声音资源名称。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="priority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="bindingTrans">声音绑定的Transform。</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        private int PlaySound(string soundAssetName, string soundGroupName, int priority,
            PlaySoundParams playSoundParams, Transform bindingTrans, Vector3 worldPosition, object userData)
        {
            if (_resourceManager == null)
            {
                throw new LFrameworkException("You must set resource manager first.");
            }

            if (playSoundParams == null)
            {
                playSoundParams = PlaySoundParams.Create();
            }

            int serialId = ++_serial;
            PlaySoundErrorCode? errorCode = null;
            string errorMessage = null;
            SoundGroup soundGroup = (SoundGroup)GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                errorCode = PlaySoundErrorCode.SoundGroupNotExist;
                errorMessage = Utility.Text.Format("Sound group '{0}' is not exist.", soundGroupName);
            }
            else if (soundGroup.SoundAgentCount <= 0)
            {
                errorCode = PlaySoundErrorCode.SoundGroupHasNoAgent;
                errorMessage = Utility.Text.Format("Sound group '{0}' is have no sound agent.", soundGroupName);
            }

            if (errorCode.HasValue)
            {
                Log.Error(errorMessage);

                if (playSoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundParams);
                }

                return serialId;
            }

            _soundsBeingLoaded.Add(serialId);
            _resourceManager.LoadAsset(soundAssetName, priority, _loadAssetCallbacks,
                PlaySoundInfo.Create(serialId, soundGroup, playSoundParams, bindingTrans, worldPosition, userData));
            return serialId;
        }

        private void LoadAssetSuccessCallback(string soundAssetName, object soundAsset, float duration, object userData)
        {
            PlaySoundInfo playSoundInfo = (PlaySoundInfo)userData;
            if (playSoundInfo == null)
            {
                throw new LFrameworkException("Play sound info is invalid.");
            }

            if (_soundsToReleaseOnLoad.Contains(playSoundInfo.SerialId))
            {
                _soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundInfo.PlaySoundParams);
                }

                ReferencePool.Release(playSoundInfo);
                ReleaseSoundAsset(soundAsset);
                return;
            }

            _soundsBeingLoaded.Remove(playSoundInfo.SerialId);

            PlaySoundErrorCode? errorCode = null;
            ISoundAgent soundAgent = playSoundInfo.SoundGroup.PlaySound(playSoundInfo.SerialId, soundAsset,
                playSoundInfo.PlaySoundParams, playSoundInfo.BindingTrans, playSoundInfo.WorldPosition, out errorCode);
            if (soundAgent != null)
            {
                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundInfo.PlaySoundParams);
                }

                ReferencePool.Release(playSoundInfo);
                return;
            }

            _soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
            ReleaseSoundAsset(soundAsset);
            string errorMessage = Utility.Text.Format("Sound group '{0}' play sound '{1}' failure.",
                playSoundInfo.SoundGroup.SoundGroupName, soundAssetName);
            Log.Error(errorMessage);
            
            if (playSoundInfo.PlaySoundParams.Referenced)
            {
                ReferencePool.Release(playSoundInfo.PlaySoundParams);
            }

            ReferencePool.Release(playSoundInfo);
        }

        private void LoadAssetFailureCallback(string soundAssetName, LoadResourceStatus status, string errorMessage,
            object userData)
        {
            PlaySoundInfo playSoundInfo = (PlaySoundInfo)userData;
            if (playSoundInfo == null)
            {
                throw new LFrameworkException("Play sound info is invalid.");
            }

            if (_soundsToReleaseOnLoad.Contains(playSoundInfo.SerialId))
            {
                _soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundInfo.PlaySoundParams);
                }

                return;
            }

            _soundsBeingLoaded.Remove(playSoundInfo.SerialId);
            string appendErrorMessage =
                Utility.Text.Format("Load sound failure, asset name '{0}', status '{1}', error message '{2}'.",
                    soundAssetName, status, errorMessage);
            Log.Error(appendErrorMessage);
                
            if (playSoundInfo.PlaySoundParams.Referenced)
            {
                ReferencePool.Release(playSoundInfo.PlaySoundParams);
            }
        }
    }
}