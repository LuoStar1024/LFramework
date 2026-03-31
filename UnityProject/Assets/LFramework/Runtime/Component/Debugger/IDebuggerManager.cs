using UnityEngine;

namespace LFramework
{
    /// <summary>
    /// 调试器管理器接口。
    /// </summary>
    public interface IDebuggerManager
    {
        /// <summary>
        /// 获取或设置调试器窗口是否激活。
        /// </summary>
        bool ActiveWindow { get; set; }

        /// <summary>
        /// 获取或设置调试器漂浮框大小。
        /// </summary>
        Rect IconRect { get; set; }
        
        /// <summary>
        /// 获取或设置调试器窗口大小。
        /// </summary>
        Rect WindowRect { get; set; }
        
        /// <summary>
        /// 获取或设置调试器窗口缩放比例。
        /// </summary>
        float WindowScale { get; set; }
        
        /// <summary>
        /// 调试器窗口根结点。
        /// </summary>
        IDebuggerWindowGroup DebuggerWindowRoot { get; }

        /// <summary>
        /// 注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <param name="debuggerWindow">要注册的调试器窗口。</param>
        /// <param name="args">初始化调试器窗口参数。</param>
        void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow, params object[] args);

        /// <summary>
        /// 解除注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否解除注册调试器窗口成功。</returns>
        bool UnregisterDebuggerWindow(string path);

        /// <summary>
        /// 获取调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>要获取的调试器窗口。</returns>
        IDebuggerWindow GetDebuggerWindow(string path);

        /// <summary>
        /// 选中调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否成功选中调试器窗口。</returns>
        bool SelectDebuggerWindow(string path);

        /// <summary>
        /// 还原调试器窗口布局。
        /// </summary>
        void ResetLayout();
    }
}