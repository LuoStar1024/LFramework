namespace LFramework
{
    /// <summary>
    /// WebGL平台下，
    /// StreamingAssets：跳过远程下载资源直接访问StreamingAssets
    /// Remote：访问远程资源
    /// </summary>
    public enum LoadResourceWayWebGL
    {
        Remote,
        StreamingAssets,
    }
}