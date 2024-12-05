using System;

namespace LFramework
{
    public sealed partial class TimerPool
    {
        /// <summary>
        /// 普通Timer 类型。
        /// </summary>
        private class Timer : IReference
        {
            /// <summary>
            /// ID
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// 计时结束回调函数。
            /// </summary>
            public LFrameworkActionArgs Callback { get; set; }

            /// <summary>
            /// 回调参数。
            /// </summary>
            public object[] Args { get; private set; }

            /// <summary>
            /// 是否使用非缩放的时间。
            /// </summary>
            public bool IsUnscaled { get; set; }

            /// <summary>
            /// 是否可以移除。
            /// </summary>
            public bool IsNeedRemove { get; set; }

            /// <summary>
            /// 是否计时中。
            /// </summary>
            public bool IsRunning { get; set; }

            /// <summary>
            /// 定时时间。
            /// </summary>
            public float Time { get; set; }

            /// <summary>
            /// 当前时间。
            /// </summary>
            public float CurTime { get; set; }

            /// <summary>
            /// 重复次数。
            /// </summary>
            public int RepeatCount { get; set; }

            /// <summary>
            /// 创建定时器。
            /// </summary>
            /// <param name="id">ID</param>
            /// <param name="time">时间。</param>
            /// <param name="repeatCount">调用次数。</param>
            /// <param name="isUnscaled">是否不受时间缩放影响。</param>
            /// <param name="callback">回调。</param>
            /// <param name="args">回调参数。</param>
            /// <returns>定时器。</returns>
            public static Timer Create(int id, float time, int repeatCount, bool isUnscaled,
                LFrameworkActionArgs callback, params object[] args)
            {
                Timer timer = ReferencePool.Acquire<Timer>();
                timer.ID = id;
                timer.Callback = callback;
                timer.Args = args;
                timer.IsUnscaled = isUnscaled;
                timer.IsRunning = true;
                timer.Time = time;
                timer.CurTime = time;
                timer.Callback = callback;
                timer.RepeatCount = repeatCount;
                return timer;
            }

            /// <summary>
            /// 清理对象。
            /// </summary>
            public void Clear()
            {
                ID = -1;
                Callback = null;
                Args = null;
                IsUnscaled = false;
                IsNeedRemove = false;
                IsRunning = true;
                Time = 0;
                CurTime = 0;
                RepeatCount = 0;
            }
        }
    }
}