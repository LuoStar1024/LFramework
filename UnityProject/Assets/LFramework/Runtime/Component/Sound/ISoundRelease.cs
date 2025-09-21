namespace LFramework
{
    /// <summary>
    /// 声音资源释放接口。
    /// </summary>
    public interface ISoundRelease
    {
        /// <summary>
        /// 释放声音资源。
        /// </summary>
        /// <param name="soundAsset">要释放的声音资源。</param>
        void ReleaseSoundAsset(object soundAsset);
    }
}