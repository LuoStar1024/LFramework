namespace LFramework
{
    public interface ITimerManager
    {
        /// <summary>
        /// 添加定时器。
        /// </summary>
        /// <param name="time">时间间隔。</param>
        /// <param name="repeatCount">调用次数，小于等于0为无限。</param>
        /// <param name="isUnscaled">是否不受时间缩放影响。</param>
        /// <param name="callback">回调。</param>
        /// <param name="args">传参。（避免闭包）</param>
        /// <returns>计算器ID。</returns>
        int AddTimer(float time, LFrameworkActionArgs callback, int repeatCount = 1, bool isUnscaled = false,
            params object[] args);

        /// <summary>
        /// 暂停计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        void Stop(int timerId);

        /// <summary>
        /// 恢复计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        void Resume(int timerId);

        /// <summary>
        /// 计时器是否在运行中。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        /// <returns>否在运行中。</returns>
        bool IsRunning(int timerId);

        /// <summary>
        /// 获得计时器剩余时间。
        /// </summary>
        float GetLeftTime(int timerId);

        /// <summary>
        /// 重置计时器,恢复到开始状态。
        /// </summary>
        void Restart(int timerId);

        /// <summary>
        /// 重置计时器。
        /// </summary>
        void Reset(int timerId, float time, LFrameworkActionArgs callback, int repeatCount = 1,
            bool isUnscaled = false);

        /// <summary>
        /// 重置计时器。
        /// </summary>
        void Reset(int timerId, float time, int repeatCount = 1, bool isUnscaled = false);

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
