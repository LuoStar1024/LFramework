using GameDataTable;
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
        public static Tables DataTable
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 获取单例组件。
        /// </summary>
        public static ISingletonManager Singleton
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 获取界面组件。
        /// </summary>
        public static IUIManager UI
        {
            get;
            private set;
        }
        
        private static void InitCustomComponents()
        {
            DataTable = LFrameworkEntry.GetModule<IDataTableManager>().Tables;
            UI = LFrameworkEntry.GetModule<IUIManager>();
            Singleton = LFrameworkEntry.GetModule<ISingletonManager>();
        }
    }
}