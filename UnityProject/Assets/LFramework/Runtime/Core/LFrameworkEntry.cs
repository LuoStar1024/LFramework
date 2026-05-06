using System;
using System.Collections.Generic;

namespace LFramework
{
    /// <summary>
    /// 游戏框架入口。
    /// </summary>
    public static class LFrameworkEntry
    {
        /// <summary>
        /// 默认设计的模块数量，初始分配内存
        /// </summary>
        private const int DesignModuleCount = 16;

        private static readonly Dictionary<Type, ILFrameworkModule> ModuleDict =
            new Dictionary<Type, ILFrameworkModule>(DesignModuleCount);

        private static readonly LFrameworkLinkedList<ILFrameworkModule> ModuleLinkedList =
            new LFrameworkLinkedList<ILFrameworkModule>();

        private static readonly List<ILFrameworkModule> UpdateModuleExecuteList =
            new List<ILFrameworkModule>(DesignModuleCount);

        private static bool _isExecuteListDirty;

        /// <summary>
        /// 所有游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public static void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (_isExecuteListDirty)
            {
                _isExecuteListDirty = false;
                BuildExecuteList();
            }

            for (int i = 0, count = UpdateModuleExecuteList.Count; i < count; i++)
            {
                UpdateModuleExecuteList[i].OnUpdate(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        public static void Shutdown()
        {
            for (LinkedListNode<ILFrameworkModule> current = ModuleLinkedList.Last;
                 current != null;
                 current = current.Previous)
            {
                current.Value.Shutdown();
            }

            ModuleLinkedList.Clear();
            UpdateModuleExecuteList.Clear();
            ReferencePool.ClearAll();
            Utility.Marshal.FreeCachedHGlobal();
            LFrameworkLog.SetLogHelper(null);
        }

        /// <summary>
        /// 获取已注册游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        public static T GetModule<T>() where T : class
        {
            Type interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                throw new LFrameworkException(Utility.Text.Format("You must get module by interface, but '{0}' is not.",
                    interfaceType.FullName));
            }

            if (ModuleDict.TryGetValue(interfaceType, out var module))
            {
                return module as T;
            }

            throw new LFrameworkException(Utility.Text.Format("Can not find LFramework module type '{0}'.",
                interfaceType.FullName));
        }

        /// <summary>
        /// 注册游戏框架模块。
        /// </summary>
        /// <param name="module">要注册的游戏框架模块。</param>
        /// <typeparam name="T">模块接口类型。</typeparam>
        public static void RegisterModule<T>(ILFrameworkModule module) where T : class
        {
            Type interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                throw new LFrameworkException(Utility.Text.Format("You must get module by interface, but '{0}' is not.",
                    interfaceType.FullName));
            }

            if (!ModuleDict.TryAdd(interfaceType, module))
            {
                throw new LFrameworkException(Utility.Text.Format("module '{0}' is already exist.",
                    interfaceType.FullName));
            }

            LinkedListNode<ILFrameworkModule> current = ModuleLinkedList.First;
            while (current != null)
            {
                if (module.Priority > current.Value.Priority)
                {
                    break;
                }

                current = current.Next;
            }

            if (current != null)
            {
                ModuleLinkedList.AddBefore(current, module);
            }
            else
            {
                ModuleLinkedList.AddLast(module);
            }

            _isExecuteListDirty = true;
            module.OnInit();
        }

        /// <summary>
        /// 构造执行队列。
        /// </summary>
        private static void BuildExecuteList()
        {
            UpdateModuleExecuteList.Clear();
            foreach (var updateModule in ModuleLinkedList)
            {
                UpdateModuleExecuteList.Add(updateModule);
            }
        }
    }
}