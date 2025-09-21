using System;

namespace LFramework
{
    /// <summary>
    /// 定时器管理器接口。
    /// </summary>
    public interface ITimerManager
    {
        /// <summary>
        /// 获取定时器数量。
        /// </summary>
        int TimerCount
        {
            get;
        }
        
        /// <summary>
        /// 获取不受时间缩放定时器数量。
        /// </summary>
        int UnscaledTimerCount
        {
            get;
        }
        
        /// <summary>
        /// 添加定时器。
        /// </summary>
        /// <param name="time">时间间隔。</param>
        /// <param name="callback">回调。</param>
        /// <param name="isUnscaled">是否不受时间缩放影响。</param>
        /// <param name="repeatCount">调用次数，小于等于0为无限。</param>
        /// <returns>定时器Id。</returns>
        int AddTimer(float time, Action callback, bool isUnscaled = false, int repeatCount = 1);
        
        /// <summary>
        /// 添加定时器。
        /// </summary>
        /// <param name="time">时间间隔。</param>
        /// <param name="callback">回调。</param>
        /// <param name="isUnscaled">是否不受时间缩放影响。</param>
        /// <param name="repeatCount">调用次数，小于等于0为无限。</param>
        /// <param name="args">传参。（避免闭包）</param>
        /// <returns>定时器Id。</returns>
        int AddTimer(float time, Action<object[]> callback, bool isUnscaled = false, int repeatCount = 1,  params object[] args);

        /// <summary>
        /// 暂停计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        void StopTimer(int timerId);

        /// <summary>
        /// 恢复计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        void ResumeTimer(int timerId);

        /// <summary>
        /// 计时器是否在运行中。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        /// <returns>否在运行中。</returns>
        bool IsRunningTimer(int timerId);

        /// <summary>
        /// 重置计时器,恢复到开始状态。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        void ResetTimer(int timerId);
        
        /// <summary>
        /// 移除计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        void RemoveTimer(int timerId);

        /// <summary>
        /// 移除所有计时器。
        /// </summary>
        void RemoveAllTimer();
    }
}