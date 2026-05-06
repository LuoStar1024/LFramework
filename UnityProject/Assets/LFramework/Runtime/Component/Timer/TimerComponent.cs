using System;
using System.Collections.Generic;
using UnityEngine;

namespace LFramework
{
    /// <summary>
    /// 定时器组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("LFramework/Timer")]
    public sealed partial class TimerComponent : MonoBehaviour, ILFrameworkModule, ITimerManager
    {
        private int _serialId = 0;
        private readonly List<Timer> _timerList = new List<Timer>();
        private readonly List<Timer> _unscaledTimerList = new List<Timer>();
        private readonly List<Timer> _cacheAddTimerList = new List<Timer>();
        private readonly List<int> _cacheRemoveTimerList = new List<int>();
        private readonly List<int> _cacheRemoveUnscaledTimerList = new List<int>();

        /// <summary>
        /// 获取定时器数量。
        /// </summary>
        public int TimerCount
        {
            get { return _timerList.Count; }
        }

        /// <summary>
        /// 获取不受时间缩放定时器数量。
        /// </summary>
        public int UnscaledTimerCount
        {
            get { return _unscaledTimerList.Count; }
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public int Priority
        {
            get { return 0; }
        }

        private void Awake()
        {
            LFrameworkEntry.RegisterModule<ITimerManager>(this);
        }

        public void OnInit()
        {
        }

        /// <summary>
        /// 定时器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            for (int i = 0, len = _cacheAddTimerList.Count; i < len; i++)
            {
                InsertTimer(_cacheAddTimerList[i]);
            }

            _cacheAddTimerList.Clear();

            UpdateTimer(elapseSeconds);
            UpdateUnscaledTimer(realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理定时器。
        /// </summary>
        public void Shutdown()
        {
            _cacheRemoveTimerList.Clear();
            _cacheRemoveUnscaledTimerList.Clear();

            RemoveAllTimer();
        }

        /// <summary>
        /// 添加定时器。
        /// </summary>
        /// <param name="time">时间间隔。</param>
        /// <param name="callback">回调。</param>
        /// <param name="isUnscaled">是否不受时间缩放影响。</param>
        /// <param name="repeatCount">调用次数，小于等于0为无限。</param>
        /// <returns>定时器Id。</returns>
        public int AddTimer(float time, Action callback, bool isUnscaled = false, int repeatCount = 1)
        {
            if (time <= 0f)
            {
                throw new LFrameworkException("Time is invalid.");
            }

            if (callback == null)
            {
                throw new LFrameworkException("Callback is invalid.");
            }

            _serialId++;

            Timer timer = Timer.Create(_serialId, time, callback, isUnscaled, repeatCount);
            _cacheAddTimerList.Add(timer);

            return timer.ID;
        }

        /// <summary>
        /// 添加定时器。
        /// </summary>
        /// <param name="time">时间间隔。</param>
        /// <param name="callback">回调。</param>
        /// <param name="isUnscaled">是否不受时间缩放影响。</param>
        /// <param name="repeatCount">调用次数，小于等于0为无限。</param>
        /// <param name="args">传参。（避免闭包）</param>
        /// <returns>定时器Id。</returns>
        public int AddTimer(float time, Action<object[]> callback, bool isUnscaled = false, int repeatCount = 1,
            params object[] args)
        {
            if (time <= 0f)
            {
                throw new LFrameworkException("Time is invalid.");
            }

            if (callback == null)
            {
                throw new LFrameworkException("Callback is invalid.");
            }

            _serialId++;

            Timer timer = Timer.Create(_serialId, time, callback, isUnscaled, repeatCount, args);
            _cacheAddTimerList.Add(timer);

            return timer.ID;
        }

        /// <summary>
        /// 暂停计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        public void StopTimer(int timerId)
        {
            Timer timer = GetTimer(timerId);
            if (timer != null)
            {
                timer.IsRunning = false;
            }
        }

        /// <summary>
        /// 恢复计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        public void ResumeTimer(int timerId)
        {
            Timer timer = GetTimer(timerId);
            if (timer != null)
            {
                timer.IsRunning = true;
            }
        }

        /// <summary>
        /// 计时器是否在运行中。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        /// <returns>否在运行中。</returns>
        public bool IsRunningTimer(int timerId)
        {
            Timer timer = GetTimer(timerId);
            return timer is { IsRunning: true, IsNeedRemove: false };
        }

        /// <summary>
        /// 重置计时器,恢复到开始状态。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        public void ResetTimer(int timerId)
        {
            Timer timer = GetTimer(timerId);
            if (timer != null)
            {
                timer.IsRunning = true;
                timer.CurTime = timer.Time;
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

            for (int i = 0, len = _cacheAddTimerList.Count; i < len; i++)
            {
                if (_cacheAddTimerList[i].ID == timerId)
                {
                    _cacheAddTimerList[i].IsNeedRemove = true;
                    return;
                }
            }
        }

        /// <summary>
        /// 移除所有计时器。
        /// </summary>
        public void RemoveAllTimer()
        {
            _cacheRemoveTimerList.Clear();
            _cacheRemoveUnscaledTimerList.Clear();

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

            for (int i = 0, len = _cacheAddTimerList.Count; i < len; i++)
            {
                ReferencePool.Release(_cacheAddTimerList[i]);
            }

            _cacheAddTimerList.Clear();
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

            for (int i = 0, len = _cacheAddTimerList.Count; i < len; i++)
            {
                if (_cacheAddTimerList[i].ID == timerId)
                {
                    return _cacheAddTimerList[i];
                }
            }

            return null;
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

                if (!timer.IsRunning)
                {
                    continue;
                }

                timer.CurTime -= elapseSeconds;
                if (timer.CurTime <= 0)
                {
                    if (timer.Callback != null)
                    {
                        timer.Callback?.Invoke();
                    }
                    else
                    {
                        timer.CallbackArgs?.Invoke(timer.Args);
                    }

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

        private void LoopCallInBadFrame()
        {
            bool isLoopCall = false;
            for (int i = 0, len = _timerList.Count; i < len; i++)
            {
                Timer timer = _timerList[i];

                if (timer.IsNeedRemove)
                {
                    continue;
                }

                if (timer.CurTime <= 0)
                {
                    if (timer.Callback != null)
                    {
                        timer.Callback?.Invoke();
                    }
                    else
                    {
                        timer.CallbackArgs?.Invoke(timer.Args);
                    }

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

                if (!timer.IsRunning)
                {
                    continue;
                }

                timer.CurTime -= realElapseSeconds;
                if (timer.CurTime <= 0)
                {
                    if (timer.Callback != null)
                    {
                        timer.Callback?.Invoke();
                    }
                    else
                    {
                        timer.CallbackArgs?.Invoke(timer.Args);
                    }

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

        private void LoopCallUnscaledInBadFrame()
        {
            bool isLoopCall = false;
            for (int i = 0, len = _unscaledTimerList.Count; i < len; i++)
            {
                Timer timer = _unscaledTimerList[i];

                if (timer.IsNeedRemove)
                {
                    continue;
                }

                if (timer.CurTime <= 0)
                {
                    if (timer.Callback != null)
                    {
                        timer.Callback?.Invoke();
                    }
                    else
                    {
                        timer.CallbackArgs?.Invoke(timer.Args);
                    }

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

        public TimerInfo[] GetTimersInfo()
        {
            List<TimerInfo> results = new List<TimerInfo>(_timerList.Count);

            foreach (var timer in _timerList)
            {
                if (timer.Callback != null)
                {
                    results.Add(new TimerInfo(timer.ID,
                        timer.Callback.Method.DeclaringType?.Name, timer.Callback.Method.Name, timer.Time,
                        timer.RepeatCount, timer.CurTime));
                }
                else
                {
                    results.Add(new TimerInfo(timer.ID,
                        timer.CallbackArgs.Method.DeclaringType?.Name, timer.CallbackArgs.Method.Name, timer.Time,
                        timer.RepeatCount, timer.CurTime));
                }
            }

            return results.ToArray();
        }

        public TimerInfo[] GetUnscaledTimersInfo()
        {
            List<TimerInfo> results = new List<TimerInfo>(_unscaledTimerList.Count);

            foreach (var timer in _unscaledTimerList)
            {
                if (timer.Callback != null)
                {
                    results.Add(new TimerInfo(timer.ID,
                        timer.Callback.Method.DeclaringType?.Name, timer.Callback.Method.Name, timer.Time,
                        timer.RepeatCount, timer.CurTime));
                }
                else
                {
                    results.Add(new TimerInfo(timer.ID,
                        timer.CallbackArgs.Method.DeclaringType?.Name, timer.CallbackArgs.Method.Name, timer.Time,
                        timer.RepeatCount, timer.CurTime));
                }
            }

            return results.ToArray();
        }
    }
}