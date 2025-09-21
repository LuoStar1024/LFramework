using System;

namespace LFramework
{
    /// <summary>
    /// 定时器管理器。
    /// </summary>
    public sealed partial class TimerComponent
    {
        /// <summary>
        /// 普通Timer 类型。
        /// </summary>
        private sealed class Timer : IReference
        {
            /// <summary>
            /// ID
            /// </summary>
            public int ID { get; private set; }
            
            /// <summary>
            /// 定时时间。
            /// </summary>
            public float Time { get; private set; }

            /// <summary>
            /// 计时结束回调函数。
            /// </summary>
            public Action Callback { get; private set; }
            
            /// <summary>
            /// 计时结束回调函数。
            /// </summary>
            public Action<object[]> CallbackArgs { get; private set; }

            /// <summary>
            /// 是否使用非缩放的时间。
            /// </summary>
            public bool IsUnscaled { get; private set; }
            
            /// <summary>
            /// 重复次数。
            /// </summary>
            public int RepeatCount { get; set; }
            
            /// <summary>
            /// 回调参数。
            /// </summary>
            public object[] Args { get; private set; }

            /// <summary>
            /// 是否需要移除。
            /// </summary>
            public bool IsNeedRemove { get; set; }

            /// <summary>
            /// 是否计时中。
            /// </summary>
            public bool IsRunning { get; set; }

            /// <summary>
            /// 当前时间。
            /// </summary>
            public float CurTime { get; set; }

            /// <summary>
            /// 创建定时器。
            /// </summary>
            /// <param name="id">ID</param>
            /// <param name="time">时间。</param>
            /// <param name="callback">回调。</param>
            /// <param name="isUnscaled">是否不受时间缩放影响。</param>
            /// <param name="repeatCount">调用次数。</param>
            /// <returns>定时器。</returns>
            public static Timer Create(int id, float time, Action callback, bool isUnscaled, int repeatCount)
            {
                Timer timer = ReferencePool.Acquire<Timer>();
                timer.ID = id;
                timer.Time = time;
                timer.Callback = callback;
                timer.CallbackArgs = null;
                timer.IsUnscaled = isUnscaled;
                timer.RepeatCount = repeatCount;
                timer.Args = null;
                timer.IsNeedRemove = false;
                timer.IsRunning = true;
                timer.CurTime = time;
                return timer;
            }
            
            /// <summary>
            /// 创建定时器。
            /// </summary>
            /// <param name="id">ID</param>
            /// <param name="time">时间。</param>
            /// <param name="callback">回调。</param>
            /// <param name="isUnscaled">是否不受时间缩放影响。</param>
            /// <param name="repeatCount">调用次数。</param>
            /// <param name="args">回调参数。</param>
            /// <returns>定时器。</returns>
            public static Timer Create(int id, float time, Action<object[]> callback, bool isUnscaled, int repeatCount, params object[] args)
            {
                Timer timer = ReferencePool.Acquire<Timer>();
                timer.ID = id;
                timer.Time = time;
                timer.Callback = null;
                timer.CallbackArgs = callback;
                timer.IsUnscaled = isUnscaled;
                timer.RepeatCount = repeatCount;
                timer.Args = args;
                timer.IsNeedRemove = false;
                timer.IsRunning = true;
                timer.CurTime = time;
                return timer;
            }

            /// <summary>
            /// 清理对象。
            /// </summary>
            public void Clear()
            {
                ID = -1;
                Time = 0;
                Callback = null;
                CallbackArgs = null;
                IsUnscaled = false;
                RepeatCount = 0;
                Args = null;
                IsNeedRemove = false;
                IsRunning = true;
                CurTime = 0;
            }
        }
    }
}