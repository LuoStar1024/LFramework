namespace LFramework
{
    /// <summary>
    /// 声音资源释放接口。
    /// </summary>
    public interface IAudioRelease
    {
        /// <summary>
        /// 释放声音资源。
        /// </summary>
        /// <param name="audioAsset">要释放的声音资源。</param>
        void ReleaseAudioAsset(object audioAsset);
    }
}