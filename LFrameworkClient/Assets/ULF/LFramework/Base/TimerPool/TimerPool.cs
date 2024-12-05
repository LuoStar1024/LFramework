using System.Collections.Generic;

namespace LFramework
{
    /// <summary>
    /// 定时器 池子。
    /// </summary>
    public sealed partial class TimerPool
    {
        private int _serialId = 0;

        private readonly List<Timer> _timerList = new List<Timer>();
        private readonly List<Timer> _unscaledTimerList = new List<Timer>();
        private readonly List<int> _cacheRemoveTimerList = new List<int>();
        private readonly List<int> _cacheRemoveUnscaledTimerList = new List<int>();

        /// <summary>
        /// 定时器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal void Update(float elapseSeconds, float realElapseSeconds)
        {
            UpdateTimer(elapseSeconds);
            UpdateUnscaledTimer(realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理定时器。
        /// </summary>
        internal void Shutdown()
        {
            RemoveAllTimer();
        }

        /// <summary>
        /// 添加定时器。
        /// </summary>
        /// <param name="time">时间间隔。</param>
        /// <param name="repeatCount">调用次数，小于等于0为无限。</param>
        /// <param name="isUnscaled">是否不受时间缩放影响。</param>
        /// <param name="callback">回调。</param>
        /// <param name="args">传参。（避免闭包）</param>
        /// <returns>计算器ID。</returns>
        public int AddTimer(float time, LFrameworkActionArgs callback, int repeatCount = 1, bool isUnscaled = false,
            params object[] args)
        {
            _serialId++;

            Timer timer = Timer.Create(_serialId, time, repeatCount, isUnscaled, callback, args);
            InsertTimer(timer);

            return timer.ID;
        }

        /// <summary>
        /// 暂停计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        public void Stop(int timerId)
        {
            Timer timer = GetTimer(timerId);
            if (timer != null) timer.IsRunning = false;
        }

        /// <summary>
        /// 恢复计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        public void Resume(int timerId)
        {
            Timer timer = GetTimer(timerId);
            if (timer != null) timer.IsRunning = true;
        }

        /// <summary>
        /// 计时器是否在运行中。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        /// <returns>否在运行中。</returns>
        public bool IsRunning(int timerId)
        {
            Timer timer = GetTimer(timerId);
            return timer is { IsRunning: true };
        }

        /// <summary>
        /// 获得计时器剩余时间。
        /// </summary>
        public float GetLeftTime(int timerId)
        {
            Timer timer = GetTimer(timerId);
            if (timer == null) return 0;
            return timer.CurTime;
        }

        /// <summary>
        /// 重置计时器,恢复到开始状态。
        /// </summary>
        public void Restart(int timerId)
        {
            Timer timer = GetTimer(timerId);
            if (timer != null)
            {
                timer.CurTime = timer.Time;
                timer.IsRunning = true;
            }
        }

        /// <summary>
        /// 重置计时器。
        /// </summary>
        public void Reset(int timerId, float time, LFrameworkActionArgs callback, int repeatCount = 1,
            bool isUnscaled = false)
        {
            Timer timer = GetTimer(timerId);
            if (timer != null)
            {
                timer.CurTime = time;
                timer.Time = time;
                timer.Callback = callback;
                timer.RepeatCount = repeatCount;
                timer.IsNeedRemove = false;
                if (timer.IsUnscaled != isUnscaled)
                {
                    RemoveTimerImmediate(timerId);

                    timer.IsUnscaled = isUnscaled;
                    InsertTimer(timer);
                }
            }
        }

        /// <summary>
        /// 重置计时器。
        /// </summary>
        public void Reset(int timerId, float time, int repeatCount = 1, bool isUnscaled= false)
        {
            Timer timer = GetTimer(timerId);
            if (timer != null)
            {
                timer.CurTime = time;
                timer.Time = time;
                timer.RepeatCount = repeatCount;
                timer.IsNeedRemove = false;
                if (timer.IsUnscaled != isUnscaled)
                {
                    RemoveTimerImmediate(timerId);

                    timer.IsUnscaled = isUnscaled;
                    InsertTimer(timer);
                }
            }
        }

        /// <summary>
        /// 移除计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        public void RemoveTimer(int timerId)
        {
            for (int i = 0, len = _timerList.Count; i < len; i++)
            {
                if (_timerList[i].ID == timerId)
                {
                    _timerList[i].IsNeedRemove = true;
                    return;
                }
            }

            for (int i = 0, len = _unscaledTimerList.Count; i < len; i++)
            {
                if (_unscaledTimerList[i].ID == timerId)
                {
                    _unscaledTimerList[i].IsNeedRemove = true;
                    return;
                }
            }
        }

        /// <summary>
        /// 移除所有计时器。
        /// </summary>
        public void RemoveAllTimer()
        {
            for (int i = 0, len = _timerList.Count; i < len; i++)
            {
                ReferencePool.Release(_timerList[i]);
            }

            _timerList.Clear();

            for (int i = 0, len = _unscaledTimerList.Count; i < len; i++)
            {
                ReferencePool.Release(_unscaledTimerList[i]);
            }

            _unscaledTimerList.Clear();
        }

        private void InsertTimer(Timer timer)
        {
            bool isInsert = false;
            if (timer.IsUnscaled)
            {
                for (int i = 0, len = _unscaledTimerList.Count; i < len; i++)
                {
                    if (_unscaledTimerList[i].CurTime > timer.CurTime)
                    {
                        _unscaledTimerList.Insert(i, timer);
                        isInsert = true;
                        break;
                    }
                }

                if (!isInsert)
                {
                    _unscaledTimerList.Add(timer);
                }
            }
            else
            {
                for (int i = 0, len = _timerList.Count; i < len; i++)
                {
                    if (_timerList[i].CurTime > timer.CurTime)
                    {
                        _timerList.Insert(i, timer);
                        isInsert = true;
                        break;
                    }
                }

                if (!isInsert)
                {
                    _timerList.Add(timer);
                }
            }
        }

        /// <summary>
        /// 立即移除。
        /// </summary>
        /// <param name="timerId">ID</param>
        private void RemoveTimerImmediate(int timerId)
        {
            for (int i = 0, len = _timerList.Count; i < len; i++)
            {
                if (_timerList[i].ID == timerId)
                {
                    _timerList.RemoveAt(i);
                    return;
                }
            }

            for (int i = 0, len = _unscaledTimerList.Count; i < len; i++)
            {
                if (_unscaledTimerList[i].ID == timerId)
                {
                    _unscaledTimerList.RemoveAt(i);
                    return;
                }
            }
        }

        private Timer GetTimer(int timerId)
        {
            for (int i = 0, len = _timerList.Count; i < len; i++)
            {
                if (_timerList[i].ID == timerId)
                {
                    return _timerList[i];
                }
            }

            for (int i = 0, len = _unscaledTimerList.Count; i < len; i++)
            {
                if (_unscaledTimerList[i].ID == timerId)
                {
                    return _unscaledTimerList[i];
                }
            }

            return null;
        }

        private void LoopCallInBadFrame()
        {
            bool isLoopCall = false;
            for (int i = 0, len = _timerList.Count; i < len; i++)
            {
                Timer timer = _timerList[i];
                
                if (timer.IsNeedRemove) continue;
                if (timer.CurTime <= 0)
                {
                    timer.Callback?.Invoke(timer.Args);
                    timer.RepeatCount--;
                    
                    if (timer.RepeatCount != 0)
                    {
                        timer.CurTime += timer.Time;
                        if (timer.CurTime <= 0)
                        {
                            isLoopCall = true;
                        }
                    }
                    else
                    {
                        timer.IsNeedRemove = true;
                    }
                }
            }

            if (isLoopCall)
            {
                LoopCallInBadFrame();
            }
        }

        private void LoopCallUnscaledInBadFrame()
        {
            bool isLoopCall = false;
            for (int i = 0, len = _unscaledTimerList.Count; i < len; i++)
            {
                Timer timer = _unscaledTimerList[i];
                
                if (timer.IsNeedRemove) continue;
                if (timer.CurTime <= 0)
                {
                    timer.Callback?.Invoke(timer.Args);
                    timer.RepeatCount--;
                    
                    if (timer.RepeatCount != 0)
                    {
                        timer.CurTime += timer.Time;
                        if (timer.CurTime <= 0)
                        {
                            isLoopCall = true;
                        }
                    }
                    else
                    {
                        timer.IsNeedRemove = true;
                    }
                }
            }

            if (isLoopCall)
            {
                LoopCallUnscaledInBadFrame();
            }
        }

        private void UpdateTimer(float elapseSeconds)
        {
            bool isLoopCall = false;
            for (int i = 0, len = _timerList.Count; i < len; i++)
            {
                Timer timer = _timerList[i];
                if (timer.IsNeedRemove)
                {
                    _cacheRemoveTimerList.Add(i);
                    continue;
                }

                if (!timer.IsRunning) continue;
                timer.CurTime -= elapseSeconds;
                if (timer.CurTime <= 0)
                {
                    timer.Callback?.Invoke(timer.Args);
                    timer.RepeatCount--;

                    if (timer.RepeatCount != 0)
                    {
                        timer.CurTime += timer.Time;
                        if (timer.CurTime <= 0)
                        {
                            isLoopCall = true;
                        }
                    }
                    else
                    {
                        _cacheRemoveTimerList.Add(i);
                    }
                }
            }

            for (int i = _cacheRemoveTimerList.Count - 1; i >= 0; i--)
            {
                Timer timer = _timerList[_cacheRemoveTimerList[i]];
                _timerList.RemoveAt(_cacheRemoveTimerList[i]);
                _cacheRemoveTimerList.RemoveAt(i);
                ReferencePool.Release(timer);
            }

            if (isLoopCall)
            {
                LoopCallInBadFrame();
            }
        }

        private void UpdateUnscaledTimer(float realElapseSeconds)
        {
            bool isLoopCall = false;
            for (int i = 0, len = _unscaledTimerList.Count; i < len; i++)
            {
                Timer timer = _unscaledTimerList[i];
                if (timer.IsNeedRemove)
                {
                    _cacheRemoveUnscaledTimerList.Add(i);
                    continue;
                }

                if (!timer.IsRunning) continue;
                timer.CurTime -= realElapseSeconds;
                if (timer.CurTime <= 0)
                {
                    timer.Callback?.Invoke(timer.Args);
                    timer.RepeatCount--;

                    if (timer.RepeatCount != 0)
                    {
                        timer.CurTime += timer.Time;
                        if (timer.CurTime <= 0)
                        {
                            isLoopCall = true;
                        }
                    }
                    else
                    {
                        _cacheRemoveUnscaledTimerList.Add(i);
                    }
                }
            }

            for (int i = _cacheRemoveUnscaledTimerList.Count - 1; i >= 0; i--)
            {
                Timer timer = _unscaledTimerList[_cacheRemoveUnscaledTimerList[i]];
                _unscaledTimerList.RemoveAt(_cacheRemoveUnscaledTimerList[i]);
                _cacheRemoveUnscaledTimerList.RemoveAt(i);
                ReferencePool.Release(timer);
            }

            if (isLoopCall)
            {
                LoopCallUnscaledInBadFrame();
            }
        }
    }
}