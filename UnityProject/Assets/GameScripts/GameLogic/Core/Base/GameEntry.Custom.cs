using GameConfig;
using LFramework;

namespace GameLogic
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        /// <summary>
        /// 获取配置表组件
        /// </summary>
        public static IConfigManager Config
        {
            get;
            private set;
        }
        
        private static void InitCustomComponents()
        {
            Config = LFrameworkEntry.GetModule<IConfigManager>();
        }
    }
}