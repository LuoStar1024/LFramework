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

        /// <summary>
        /// 获取数据结点组件。
        /// </summary>
        public static IDataNodeManager DataNode
        {
            get;
            private set;
        }

        // /// <summary>
        // /// 获取调试组件。
        // /// </summary>
        // public static DebuggerComponent Debugger
        // {
        //     get;
        //     private set;
        // }

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

        // /// <summary>
        // /// 获取本地化组件。
        // /// </summary>
        // public static LocalizationComponent Localization
        // {
        //     get;
        //     private set;
        // }

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

        // /// <summary>
        // /// 获取场景组件。
        // /// </summary>
        // public static SceneComponent Scene
        // {
        //     get;
        //     private set;
        // }

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
        public static ISoundManager Sound
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
        
        // /// <summary>
        // /// 获取界面组件。
        // /// </summary>
        // public static UIComponent UI
        // {
        //     get;
        //     private set;
        // }

        private static void InitBuiltinComponents()
        {
            Base = LFrameworkEntry.GetModule<IBaseManager>();
            DataNode = LFrameworkEntry.GetModule<IDataNodeManager>();
            // Debugger = UnityLFrameworkEntry.GetComponent<DebuggerComponent>();
            Event = LFrameworkEntry.GetModule<IEventManager>();
            Fsm = LFrameworkEntry.GetModule<IFsmManager>();
            // Localization = UnityLFrameworkEntry.GetComponent<LocalizationComponent>();
            ObjectPool = LFrameworkEntry.GetModule<IObjectPoolManager>();
            Procedure = LFrameworkEntry.GetModule<IProcedureManager>();
            Resource = LFrameworkEntry.GetModule<IResourceManager>();
            // Scene = UnityLFrameworkEntry.GetComponent<SceneComponent>();
            Setting = LFrameworkEntry.GetModule<ISettingManager>();
            Sound = LFrameworkEntry.GetModule<ISoundManager>();
            Timer = LFrameworkEntry.GetModule<ITimerManager>();
            // UI = UnityLFrameworkEntry.GetComponent<UIComponent>();
        }
    }
}