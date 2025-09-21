using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace LFramework
{
    /// <summary>
    /// 声音代理。
    /// </summary>
    public sealed class SoundAgent : MonoBehaviour, ISoundAgent
    {
        private Transform _cachedTransform = null;
        private AudioSource _audioSource = null;
        private Transform _bindingTrans;
        private float _volumeWhenPause = 0f;
        private bool _applicationPauseFlag = false;
        private bool _isPaused;
        
        private SoundGroup _soundGroup;
        private ISoundRelease _soundRelease;
        private int _serialId;
        private object _soundAsset;
        private DateTime _setSoundAssetTime;
        private bool _muteInSoundGroup;
        private float _volumeInSoundGroup;

        /// <summary>
        /// 获取所在的声音组。
        /// </summary>
        public ISoundGroup SoundGroup
        {
            get { return _soundGroup; }
            internal set { _soundGroup = value as SoundGroup; }
        }

        /// <summary>
        /// 获取或设置声音的序列编号。
        /// </summary>
        public int SerialId
        {
            get { return _serialId; }
            set { _serialId = value; }
        }

        /// <summary>
        /// 获取当前是否正在播放。
        /// </summary>
        public bool IsPlaying
        {
            get { return _audioSource.isPlaying || _isPaused; }
        }

        /// <summary>
        /// 获取声音长度。
        /// </summary>
        public float Length
        {
            get { return _audioSource.clip != null ? _audioSource.clip.length : 0f; }
        }

        /// <summary>
        /// 获取或设置播放位置。
        /// </summary>
        public float Time
        {
            get { return _audioSource.time; }
            set { _audioSource.time = value; }
        }

        /// <summary>
        /// 获取是否静音。
        /// </summary>
        public bool Mute
        {
            get { return _audioSource.mute; }
            set { _audioSource.mute = value; }
        }

        /// <summary>
        /// 获取或设置在声音组内是否静音。
        /// </summary>
        public bool MuteInSoundGroup
        {
            get { return _muteInSoundGroup; }
            set
            {
                _muteInSoundGroup = value;
                RefreshMute();
            }
        }

        /// <summary>
        /// 获取或设置是否循环播放。
        /// </summary>
        public bool Loop
        {
            get { return _audioSource.loop; }
            set { _audioSource.loop = value; }
        }

        /// <summary>
        /// 获取或设置声音优先级。
        /// </summary>
        public int Priority
        {
            get { return 128 - _audioSource.priority; }
            set { _audioSource.priority = 128 - value; }
        }

        /// <summary>
        /// 获取音量大小。
        /// </summary>
        public float Volume
        {
            get { return _audioSource.volume; }
            set { _audioSource.volume = value; }
        }

        /// <summary>
        /// 获取或设置在声音组内音量大小。
        /// </summary>
        public float VolumeInSoundGroup
        {
            get { return _volumeInSoundGroup; }
            set
            {
                _volumeInSoundGroup = value;
                RefreshVolume();
            }
        }

        /// <summary>
        /// 获取或设置声音音调。
        /// </summary>
        public float Pitch
        {
            get { return _audioSource.pitch; }
            set { _audioSource.pitch = value; }
        }

        /// <summary>
        /// 获取或设置声音立体声声相。
        /// </summary>
        public float PanStereo
        {
            get { return _audioSource.panStereo; }
            set { _audioSource.panStereo = value; }
        }

        /// <summary>
        /// 获取或设置声音空间混合量。
        /// </summary>
        public float SpatialBlend
        {
            get { return _audioSource.spatialBlend; }
            set { _audioSource.spatialBlend = value; }
        }

        /// <summary>
        /// 获取或设置声音最大距离。
        /// </summary>
        public float MaxDistance
        {
            get { return _audioSource.maxDistance; }
            set { _audioSource.maxDistance = value; }
        }

        /// <summary>
        /// 获取或设置声音多普勒等级。
        /// </summary>
        public float DopplerLevel
        {
            get { return _audioSource.dopplerLevel; }
            set { _audioSource.dopplerLevel = value; }
        }
        
        /// <summary>
        /// 获取或设置声音代理辅助器所在的混音组。
        /// </summary>
        public AudioMixerGroup AudioMixerGroup
        {
            get { return _audioSource.outputAudioMixerGroup; }
            set { _audioSource.outputAudioMixerGroup = value; }
        }

        /// <summary>
        /// 获取声音创建时间。
        /// </summary>
        internal DateTime SetSoundAssetTime
        {
            get { return _setSoundAssetTime; }
        }

        internal void SetSoundRelease(ISoundRelease soundRelease)
        {
            if (soundRelease != null)
            {
                _soundRelease = soundRelease;
            }
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        public void Play()
        {
            Play(SoundConstant.DefaultFadeInSeconds);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void Play(float fadeInSeconds)
        {
            StopAllCoroutines();

            _audioSource.Play();
            if (fadeInSeconds > 0f)
            {
                float volume = _audioSource.volume;
                _audioSource.volume = 0f;
                StartCoroutine(FadeToVolume(_audioSource, volume, fadeInSeconds));
            }
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        public void Stop()
        {
            Stop(SoundConstant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void Stop(float fadeOutSeconds)
        {
            StopAllCoroutines();

            if (fadeOutSeconds > 0f && gameObject.activeInHierarchy)
            {
                StartCoroutine(StopCo(fadeOutSeconds));
            }
            else
            {
                _audioSource.Stop();
            }
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        public void Pause()
        {
            Pause(SoundConstant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void Pause(float fadeOutSeconds)
        {
            StopAllCoroutines();

            _volumeWhenPause = _audioSource.volume;
            if (fadeOutSeconds > 0f && gameObject.activeInHierarchy)
            {
                StartCoroutine(PauseCo(fadeOutSeconds));
            }
            else
            {
                _isPaused = true;
                _audioSource.Pause();
            }
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        public void Resume()
        {
            Resume(SoundConstant.DefaultFadeInSeconds);
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void Resume(float fadeInSeconds)
        {
            StopAllCoroutines();
            _isPaused = false;
            _audioSource.UnPause();
            if (fadeInSeconds > 0f)
            {
                StartCoroutine(FadeToVolume(_audioSource, _volumeWhenPause, fadeInSeconds));
            }
            else
            {
                _audioSource.volume = _volumeWhenPause;
            }
        }

        /// <summary>
        /// 重置声音代理。
        /// </summary>
        public void Reset()
        {
            if (_soundAsset != null)
            {
                _soundRelease.ReleaseSoundAsset(_soundAsset);
                _soundAsset = null;
            }

            _setSoundAssetTime = DateTime.MinValue;
            Time = SoundConstant.DefaultTime;
            MuteInSoundGroup = SoundConstant.DefaultMute;
            Loop = SoundConstant.DefaultLoop;
            Priority = SoundConstant.DefaultPriority;
            VolumeInSoundGroup = SoundConstant.DefaultVolume;
            Pitch = SoundConstant.DefaultPitch;
            PanStereo = SoundConstant.DefaultPanStereo;
            SpatialBlend = SoundConstant.DefaultSpatialBlend;
            MaxDistance = SoundConstant.DefaultMaxDistance;
            DopplerLevel = SoundConstant.DefaultDopplerLevel;
            
            _cachedTransform.localPosition = Vector3.zero;
            _audioSource.clip = null;
            _bindingTrans = null;
            _volumeWhenPause = 0f;
            _isPaused = false;
        }

        public bool SetSoundAsset(object soundAsset)
        {
            Reset();
            _soundAsset = soundAsset;
            _setSoundAssetTime = DateTime.UtcNow;
            
            AudioClip audioClip = soundAsset as AudioClip;
            if (audioClip == null)
            {
                return false;
            }

            _audioSource.clip = audioClip;
            return true;
        }

        /// <summary>
        /// 设置声音绑定的实体。
        /// </summary>
        /// <param name="bindingTrans">声音绑定的实体。</param>
        public void SetBindingEntity(Transform bindingTrans)
        {
            _bindingTrans = bindingTrans;
            if (_bindingTrans != null)
            {
                UpdateAgentPosition();
                return;
            }
            
            Reset();
        }

        /// <summary>
        /// 设置声音所在的世界坐标。
        /// </summary>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        public void SetWorldPosition(Vector3 worldPosition)
        {
            _cachedTransform.position = worldPosition;
        }

        internal void RefreshMute()
        {
            Mute = _soundGroup.Mute || _muteInSoundGroup;
        }

        internal void RefreshVolume()
        {
            Volume = _soundGroup.Volume * _volumeInSoundGroup;
        }
        
        private void Awake()
        {
            _cachedTransform = transform;
            _audioSource = gameObject.GetOrAddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.rolloffMode = AudioRolloffMode.Custom;
        }

        private void Update()
        {
            if (!_applicationPauseFlag && !IsPlaying && _audioSource.clip != null)
            {
                Reset();
                return;
            }

            if (_bindingTrans != null)
            {
                UpdateAgentPosition();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            _applicationPauseFlag = pause;
        }
        
        private void UpdateAgentPosition()
        {
            if (_bindingTrans.gameObject.activeInHierarchy)
            {
                _cachedTransform.position = _bindingTrans.position;
                return;
            }

            Reset();
        }

        private IEnumerator StopCo(float fadeOutSeconds)
        {
            yield return FadeToVolume(_audioSource, 0f, fadeOutSeconds);
            _audioSource.Stop();
        }

        private IEnumerator PauseCo(float fadeOutSeconds)
        {
            yield return FadeToVolume(_audioSource, 0f, fadeOutSeconds);
            _isPaused = true;
            _audioSource.Pause();
        }

        private IEnumerator FadeToVolume(AudioSource audioSource, float volume, float duration)
        {
            float time = 0f;
            float originalVolume = audioSource.volume;
            while (time < duration)
            {
                time += UnityEngine.Time.deltaTime;
                audioSource.volume = Mathf.Lerp(originalVolume, volume, time / duration);
                yield return new WaitForEndOfFrame();
            }

            audioSource.volume = volume;
        }
    }
}