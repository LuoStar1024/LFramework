namespace LFramework
{
    /// <summary>
    /// 基础管理器接口。
    /// </summary>
    public interface IBaseManager
    {
        /// <summary>
        /// 获取或设置游戏帧率。
        /// </summary>
        int FrameRate
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置游戏速度。
        /// </summary>
        float GameSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// 获取游戏是否暂停。
        /// </summary>
        bool IsGamePaused
        {
            get;
        }

        /// <summary>
        /// 获取是否正常游戏速度。
        /// </summary>
        bool IsNormalGameSpeed
        {
            get;
        }

        /// <summary>
        /// 获取或设置是否允许后台运行。
        /// </summary>
        bool RunInBackground
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置是否禁止休眠。
        /// </summary>
        bool NeverSleep
        {
            get;
            set;
        }

        /// <summary>
        /// 暂停游戏。
        /// </summary>
        void PauseGame();

        /// <summary>
        /// 恢复游戏。
        /// </summary>
        void ResumeGame();

        /// <summary>
        /// 重置为正常游戏速度。
        /// </summary>
        void ResetNormalGameSpeed();
    }
}