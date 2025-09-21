using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace LFramework
{
    /// <summary>
    /// 声音组。
    /// </summary>
    public sealed class SoundGroup : MonoBehaviour, ISoundGroup
    {
        [SerializeField]
        private AudioMixerGroup audioMixerGroup = null;
        
        [SerializeField]
        private string soundGroupName;
        
        [SerializeField]
        private bool avoidBeingReplacedBySamePriority;
        
        [SerializeField]
        private bool mute;
        
        [SerializeField]
        private float volume;
        
        [SerializeField]
        private int soundAgentCount = 1;
        
        private List<SoundAgent> _soundAgents;
        
        /// <summary>
        /// 获取或设置声音组辅助器所在的混音组。
        /// </summary>
        public AudioMixerGroup AudioMixerGroup
        {
            get { return audioMixerGroup; }
            set { audioMixerGroup = value; }
        }

        /// <summary>
        /// 获取声音组名称。
        /// </summary>
        public string SoundGroupName
        {
            get { return soundGroupName; }
            internal set { soundGroupName = value; }
        }

        /// <summary>
        /// 获取声音代理数。
        /// </summary>
        public int SoundAgentCount
        {
            get { return _soundAgents.Count; }
        }

        /// <summary>
        /// 获取或设置声音组中的声音是否避免被同优先级声音替换。
        /// </summary>
        public bool AvoidBeingReplacedBySamePriority
        {
            get { return avoidBeingReplacedBySamePriority; }
            set { avoidBeingReplacedBySamePriority = value; }
        }

        /// <summary>
        /// 获取或设置声音组静音。
        /// </summary>
        public bool Mute
        {
            get { return mute; }
            set
            {
                mute = value;
                foreach (SoundAgent soundAgent in _soundAgents)
                {
                    soundAgent.RefreshMute();
                }
            }
        }

        /// <summary>
        /// 获取或设置声音组音量。
        /// </summary>
        public float Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                foreach (SoundAgent soundAgent in _soundAgents)
                {
                    soundAgent.RefreshVolume();
                }
            }
        }

        /// <summary>
        /// 增加声音代理辅助器。
        /// </summary>
        public void AddSoundAgentHelper(ISoundRelease soundRelease, AudioMixer audioMixer, int index)
        {
            var soundAgent = new GameObject().AddComponent<SoundAgent>();
            soundAgent.name = Utility.Text.Format("Sound Agent - {0} - {1}", soundGroupName, index);
            Transform tempTrans = soundAgent.transform;
            tempTrans.SetParent(gameObject.transform);
            tempTrans.localScale = Vector3.one;

            soundAgent.SetSoundRelease(soundRelease);
            if (audioMixer != null)
            {
                AudioMixerGroup[] audioMixerGroups =
                    audioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}/{1}", soundGroupName, index));
                if (audioMixerGroups.Length > 0)
                {
                    soundAgent.AudioMixerGroup = audioMixerGroups[0];
                }
                else
                {
                    soundAgent.AudioMixerGroup = AudioMixerGroup;
                }
            }
            soundAgent.SoundGroup = this;
            soundAgent.SerialId = 0;
            soundAgent.Reset();
            
            _soundAgents.Add(soundAgent);
            soundAgentCount = _soundAgents.Count;
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="serialId">声音的序列编号。</param>
        /// <param name="soundAsset">声音资源。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="errorCode">错误码。</param>
        /// <returns>用于播放的声音代理。</returns>
        public ISoundAgent PlaySound(int serialId, object soundAsset, PlaySoundParams playSoundParams,
            Transform bindingTrans, Vector3 worldPosition, out PlaySoundErrorCode? errorCode)
        {
            errorCode = null;
            SoundAgent candidateAgent = null;
            foreach (SoundAgent soundAgent in _soundAgents)
            {
                if (!soundAgent.IsPlaying)
                {
                    candidateAgent = soundAgent;
                    break;
                }

                if (soundAgent.Priority < playSoundParams.Priority)
                {
                    if (candidateAgent == null || soundAgent.Priority < candidateAgent.Priority)
                    {
                        candidateAgent = soundAgent;
                    }
                }
                else if (!avoidBeingReplacedBySamePriority && soundAgent.Priority == playSoundParams.Priority)
                {
                    if (candidateAgent == null || soundAgent.SetSoundAssetTime < candidateAgent.SetSoundAssetTime)
                    {
                        candidateAgent = soundAgent;
                    }
                }
            }

            if (candidateAgent == null)
            {
                errorCode = PlaySoundErrorCode.IgnoredDueToLowPriority;
                return null;
            }
            
            if (!candidateAgent.SetSoundAsset(soundAsset))
            {
                errorCode = PlaySoundErrorCode.SetSoundAssetFailure;
                return null;
            }

            if (bindingTrans != null)
            {
                candidateAgent.SetBindingEntity(bindingTrans);
            }
            else
            {
                candidateAgent.SetWorldPosition(worldPosition);
            }

            candidateAgent.SerialId = serialId;
            candidateAgent.Time = playSoundParams.Time;
            candidateAgent.MuteInSoundGroup = playSoundParams.MuteInSoundGroup;
            candidateAgent.Loop = playSoundParams.Loop;
            candidateAgent.Priority = playSoundParams.Priority;
            candidateAgent.VolumeInSoundGroup = playSoundParams.VolumeInSoundGroup;
            candidateAgent.Pitch = playSoundParams.Pitch;
            candidateAgent.PanStereo = playSoundParams.PanStereo;
            candidateAgent.SpatialBlend = playSoundParams.SpatialBlend;
            candidateAgent.MaxDistance = playSoundParams.MaxDistance;
            candidateAgent.DopplerLevel = playSoundParams.DopplerLevel;
            candidateAgent.Play(playSoundParams.FadeInSeconds);
            return candidateAgent;
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId, float fadeOutSeconds)
        {
            foreach (SoundAgent soundAgent in _soundAgents)
            {
                if (soundAgent.SerialId != serialId)
                {
                    continue;
                }

                soundAgent.Stop(fadeOutSeconds);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否暂停播放声音成功。</returns>
        public bool PauseSound(int serialId, float fadeOutSeconds)
        {
            foreach (SoundAgent soundAgent in _soundAgents)
            {
                if (soundAgent.SerialId != serialId)
                {
                    continue;
                }

                soundAgent.Pause(fadeOutSeconds);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        /// <returns>是否恢复播放声音成功。</returns>
        public bool ResumeSound(int serialId, float fadeInSeconds)
        {
            foreach (SoundAgent soundAgent in _soundAgents)
            {
                if (soundAgent.SerialId != serialId)
                {
                    continue;
                }

                soundAgent.Resume(fadeInSeconds);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        public void StopAllLoadedSounds()
        {
            foreach (SoundAgent soundAgent in _soundAgents)
            {
                if (soundAgent.IsPlaying)
                {
                    soundAgent.Stop();
                }
            }
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void StopAllLoadedSounds(float fadeOutSeconds)
        {
            foreach (SoundAgent soundAgent in _soundAgents)
            {
                if (soundAgent.IsPlaying)
                {
                    soundAgent.Stop(fadeOutSeconds);
                }
            }
        }

        private void Awake()
        {
            _soundAgents = new List<SoundAgent>();
        }
    }
}