namespace LFramework
{
    /// <summary>
    /// 播放声音参数。
    /// </summary>
    public sealed class PlayAudioParams : IReference
    {
        private bool _referenced;
        private float _time;
        private bool _muteInAudioGroup;
        private bool _loop;
        private int _priority;
        private float _volumeInAudioGroup;
        private float _fadeInSeconds;
        private float _pitch;
        private float _panStereo;
        private float _spatialBlend;
        private float _maxDistance;
        private float _dopplerLevel;

        /// <summary>
        /// 初始化播放声音参数的新实例。
        /// </summary>
        public PlayAudioParams()
        {
            _referenced = false;
            _time = AudioConstant.DefaultTime;
            _muteInAudioGroup = AudioConstant.DefaultMute;
            _loop = AudioConstant.DefaultLoop;
            _priority = AudioConstant.DefaultPriority;
            _volumeInAudioGroup = AudioConstant.DefaultVolume;
            _fadeInSeconds = AudioConstant.DefaultFadeInSeconds;
            _pitch = AudioConstant.DefaultPitch;
            _panStereo = AudioConstant.DefaultPanStereo;
            _spatialBlend = AudioConstant.DefaultSpatialBlend;
            _maxDistance = AudioConstant.DefaultMaxDistance;
            _dopplerLevel = AudioConstant.DefaultDopplerLevel;
        }

        /// <summary>
        /// 获取或设置播放位置。
        /// </summary>
        public float Time
        {
            get { return _time; }
            set { _time = value; }
        }

        /// <summary>
        /// 获取或设置在声音组内是否静音。
        /// </summary>
        public bool MuteInAudioGroup
        {
            get { return _muteInAudioGroup; }
            set { _muteInAudioGroup = value; }
        }

        /// <summary>
        /// 获取或设置是否循环播放。
        /// </summary>
        public bool Loop
        {
            get { return _loop; }
            set { _loop = value; }
        }

        /// <summary>
        /// 获取或设置声音优先级。
        /// </summary>
        public int Priority
        {
            get { return _priority; }
            set { _priority = UnityEngine.Mathf.Clamp(value, AudioConstant.MinPriority, AudioConstant.MaxPriority); }
        }

        /// <summary>
        /// 获取或设置在声音组内音量大小。
        /// </summary>
        public float VolumeInAudioGroup
        {
            get { return _volumeInAudioGroup; }
            set { _volumeInAudioGroup = value; }
        }

        /// <summary>
        /// 获取或设置声音淡入时间，以秒为单位。
        /// </summary>
        public float FadeInSeconds
        {
            get { return _fadeInSeconds; }
            set { _fadeInSeconds = value; }
        }

        /// <summary>
        /// 获取或设置声音音调。
        /// </summary>
        public float Pitch
        {
            get { return _pitch; }
            set { _pitch = value; }
        }

        /// <summary>
        /// 获取或设置声音立体声声相。
        /// </summary>
        public float PanStereo
        {
            get { return _panStereo; }
            set { _panStereo = value; }
        }

        /// <summary>
        /// 获取或设置声音空间混合量。
        /// </summary>
        public float SpatialBlend
        {
            get { return _spatialBlend; }
            set { _spatialBlend = value; }
        }

        /// <summary>
        /// 获取或设置声音最大距离。
        /// </summary>
        public float MaxDistance
        {
            get { return _maxDistance; }
            set { _maxDistance = value; }
        }

        /// <summary>
        /// 获取或设置声音多普勒等级。
        /// </summary>
        public float DopplerLevel
        {
            get { return _dopplerLevel; }
            set { _dopplerLevel = value; }
        }

        internal bool Referenced
        {
            get { return _referenced; }
        }

        /// <summary>
        /// 创建播放声音参数。
        /// </summary>
        /// <returns>创建的播放声音参数。</returns>
        public static PlayAudioParams Create()
        {
            PlayAudioParams playAudioParams = ReferencePool.Acquire<PlayAudioParams>();
            playAudioParams._referenced = true;
            return playAudioParams;
        }

        /// <summary>
        /// 清理播放声音参数。
        /// </summary>
        public void Clear()
        {
            _referenced = false;
            _time = AudioConstant.DefaultTime;
            _muteInAudioGroup = AudioConstant.DefaultMute;
            _loop = AudioConstant.DefaultLoop;
            _priority = AudioConstant.DefaultPriority;
            _volumeInAudioGroup = AudioConstant.DefaultVolume;
            _fadeInSeconds = AudioConstant.DefaultFadeInSeconds;
            _pitch = AudioConstant.DefaultPitch;
            _panStereo = AudioConstant.DefaultPanStereo;
            _spatialBlend = AudioConstant.DefaultSpatialBlend;
            _maxDistance = AudioConstant.DefaultMaxDistance;
            _dopplerLevel = AudioConstant.DefaultDopplerLevel;
        }
    }
}