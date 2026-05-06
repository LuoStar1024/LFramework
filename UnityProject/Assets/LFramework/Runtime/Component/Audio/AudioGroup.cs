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
    public sealed class AudioGroup : MonoBehaviour, IAudioGroup
    {
        [SerializeField] private AudioMixerGroup audioMixerGroup = null;

        [SerializeField] private string audioGroupName;

        [SerializeField] private bool avoidBeingReplacedBySamePriority;

        [SerializeField] private bool mute;

        [SerializeField] private float volume;

        [SerializeField] private int audioAgentCount = 1;

        private List<AudioAgent> _audioAgents;

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
        public string AudioGroupName
        {
            get { return audioGroupName; }
            internal set { audioGroupName = value; }
        }

        /// <summary>
        /// 获取声音代理数。
        /// </summary>
        public int AudioAgentCount
        {
            get { return _audioAgents.Count; }
        }

        /// <summary>
        /// 获取正在播放的声音代理数。
        /// </summary>
        public int PlayingAudioAgentCount
        {
            get
            {
                int count = 0;
                foreach (AudioAgent audioAgent in _audioAgents)
                {
                    if (audioAgent.IsPlaying)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// 获取空闲的声音代理数。
        /// </summary>
        public int FreeAudioAgentCount
        {
            get { return AudioAgentCount - PlayingAudioAgentCount; }
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
                foreach (AudioAgent audioAgent in _audioAgents)
                {
                    audioAgent.RefreshMute();
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
                foreach (AudioAgent audioAgent in _audioAgents)
                {
                    audioAgent.RefreshVolume();
                }
            }
        }

        /// <summary>
        /// 增加声音代理辅助器。
        /// </summary>
        public void AddAudioAgentHelper(IAudioRelease audioRelease, AudioMixer audioMixer, int index)
        {
            var audioAgent = new GameObject().AddComponent<AudioAgent>();
            audioAgent.name = Utility.Text.Format("Audio Agent - {0} - {1}", audioGroupName, index);
            Transform tempTrans = audioAgent.transform;
            tempTrans.SetParent(gameObject.transform);
            tempTrans.localScale = Vector3.one;

            audioAgent.SetAudioRelease(audioRelease);
            if (audioMixer != null)
            {
                AudioMixerGroup[] audioMixerGroups =
                    audioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}/{1}", audioGroupName, index));
                if (audioMixerGroups.Length > 0)
                {
                    audioAgent.AudioMixerGroup = audioMixerGroups[0];
                }
                else
                {
                    audioAgent.AudioMixerGroup = AudioMixerGroup;
                }
            }

            audioAgent.AudioGroup = this;
            audioAgent.SerialId = 0;
            audioAgent.Reset();

            _audioAgents.Add(audioAgent);
            audioAgentCount = _audioAgents.Count;
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="serialId">声音的序列编号。</param>
        /// <param name="audioAsset">声音资源。</param>
        /// <param name="playAudioParams">播放声音参数。</param>
        /// <param name="errorCode">错误码。</param>
        /// <returns>用于播放的声音代理。</returns>
        public IAudioAgent PlayAudio(int serialId, object audioAsset, PlayAudioParams playAudioParams,
            Transform bindingTrans, Vector3 worldPosition, out PlayAudioErrorCode? errorCode)
        {
            errorCode = null;
            AudioAgent candidateAgent = null;
            foreach (AudioAgent audioAgent in _audioAgents)
            {
                if (!audioAgent.IsPlaying)
                {
                    candidateAgent = audioAgent;
                    break;
                }

                if (audioAgent.Priority < playAudioParams.Priority)
                {
                    if (candidateAgent == null || audioAgent.Priority < candidateAgent.Priority)
                    {
                        candidateAgent = audioAgent;
                    }
                }
                else if (!avoidBeingReplacedBySamePriority && audioAgent.Priority == playAudioParams.Priority)
                {
                    if (candidateAgent == null || audioAgent.SetAudioAssetTime < candidateAgent.SetAudioAssetTime)
                    {
                        candidateAgent = audioAgent;
                    }
                }
            }

            if (candidateAgent == null)
            {
                errorCode = PlayAudioErrorCode.IgnoredDueToLowPriority;
                return null;
            }

            if (!candidateAgent.SetAudioAsset(audioAsset))
            {
                errorCode = PlayAudioErrorCode.SetAudioAssetFailure;
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
            candidateAgent.Time = playAudioParams.Time;
            candidateAgent.MuteInAudioGroup = playAudioParams.MuteInAudioGroup;
            candidateAgent.Loop = playAudioParams.Loop;
            candidateAgent.Priority = playAudioParams.Priority;
            candidateAgent.VolumeInAudioGroup = playAudioParams.VolumeInAudioGroup;
            candidateAgent.Pitch = playAudioParams.Pitch;
            candidateAgent.PanStereo = playAudioParams.PanStereo;
            candidateAgent.SpatialBlend = playAudioParams.SpatialBlend;
            candidateAgent.MaxDistance = playAudioParams.MaxDistance;
            candidateAgent.DopplerLevel = playAudioParams.DopplerLevel;
            candidateAgent.Play(playAudioParams.FadeInSeconds);
            return candidateAgent;
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopAudio(int serialId, float fadeOutSeconds)
        {
            foreach (AudioAgent audioAgent in _audioAgents)
            {
                if (audioAgent.SerialId != serialId)
                {
                    continue;
                }

                audioAgent.Stop(fadeOutSeconds);
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
        public bool PauseAudio(int serialId, float fadeOutSeconds)
        {
            foreach (AudioAgent audioAgent in _audioAgents)
            {
                if (audioAgent.SerialId != serialId)
                {
                    continue;
                }

                audioAgent.Pause(fadeOutSeconds);
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
        public bool ResumeAudio(int serialId, float fadeInSeconds)
        {
            foreach (AudioAgent audioAgent in _audioAgents)
            {
                if (audioAgent.SerialId != serialId)
                {
                    continue;
                }

                audioAgent.Resume(fadeInSeconds);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        public void StopAllLoadedAudios()
        {
            foreach (AudioAgent audioAgent in _audioAgents)
            {
                if (audioAgent.IsPlaying)
                {
                    audioAgent.Stop();
                }
            }
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void StopAllLoadedAudios(float fadeOutSeconds)
        {
            foreach (AudioAgent audioAgent in _audioAgents)
            {
                if (audioAgent.IsPlaying)
                {
                    audioAgent.Stop(fadeOutSeconds);
                }
            }
        }

        /// <summary>
        /// 获取所有声音代理。
        /// </summary>
        /// <returns>所有声音代理。</returns>
        public AudioAgent[] GetAllAudioAgents()
        {
            return _audioAgents.ToArray();
        }

        private void Awake()
        {
            _audioAgents = new List<AudioAgent>();
        }
    }
}