using LFramework;

namespace GameLogic
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        /// <summary>
        /// 获取游戏基础组件。
        /// </summary>
        public static IBaseManager Base
        {
            get;
            private set;
        }

        public static IConfigManager Config
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取数据结点组件。
        /// </summary>
        public static IDataNodeManager DataNode
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取调试组件。
        /// </summary>
        public static IDebuggerManager Debugger
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取事件组件。
        /// </summary>
        public static IEventManager Event
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取有限状态机组件。
        /// </summary>
        public static IFsmManager Fsm
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取本地化组件。
        /// </summary>
        public static ILocalizationManager Localization
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取对象池组件。
        /// </summary>
        public static IObjectPoolManager ObjectPool
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取流程组件。
        /// </summary>
        public static IProcedureManager Procedure
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取资源组件。
        /// </summary>
        public static IResourceManager Resource
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取场景组件。
        /// </summary>
        public static ISceneManager Scene
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取配置组件。
        /// </summary>
        public static ISettingManager Setting
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取声音组件。
        /// </summary>
        public static IAudioManager Audio
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取定时器组件。
        /// </summary>
        public static ITimerManager Timer
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 获取定时器组件。
        /// </summary>
        public static IUnityWrapperManager Unity
        {
            get;
            private set;
        }

        private static void InitBuiltinComponents()
        {
            Base = LFrameworkEntry.GetModule<IBaseManager>();
            Config = LFrameworkEntry.GetModule<IConfigManager>();
            DataNode = LFrameworkEntry.GetModule<IDataNodeManager>();
            Debugger = LFrameworkEntry.GetModule<IDebuggerManager>();
            Event = LFrameworkEntry.GetModule<IEventManager>();
            Fsm = LFrameworkEntry.GetModule<IFsmManager>();
            Localization = LFrameworkEntry.GetModule<ILocalizationManager>();
            ObjectPool = LFrameworkEntry.GetModule<IObjectPoolManager>();
            Procedure = LFrameworkEntry.GetModule<IProcedureManager>();
            Resource = LFrameworkEntry.GetModule<IResourceManager>();
            Scene = LFrameworkEntry.GetModule<ISceneManager>();
            Setting = LFrameworkEntry.GetModule<ISettingManager>();
            Audio = LFrameworkEntry.GetModule<IAudioManager>();
            Timer = LFrameworkEntry.GetModule<ITimerManager>();
            Unity = LFrameworkEntry.GetModule<IUnityWrapperManager>();
        }
    }
}
