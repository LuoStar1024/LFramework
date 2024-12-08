using System;
using System.Collections.Generic;

namespace LFramework
{
    internal sealed partial class EventPool
    {
        /// <summary>
        /// 事件Table。
        /// </summary>
        private static readonly Dictionary<int, EventData> EventDict = new Dictionary<int, EventData>();

        #region 事件管理接口

        /// <summary>
        /// 订阅事件处理委托。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理委托。</param>
        /// <returns>是否添加成功。</returns>
        public bool Subscribe(int eventType, Delegate handler)
        {
            if (!EventDict.TryGetValue(eventType, out var data))
            {
                data = EventData.Create(eventType);
                EventDict.Add(eventType, data);
            }

            return data.Subscribe(handler);
        }

        /// <summary>
        /// 取消订阅事件处理委托。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理委托。</param>
        public void Unsubscribe(int eventType, Delegate handler)
        {
            if (EventDict.TryGetValue(eventType, out var data))
            {
                data.Unsubscribe(handler);
            }
        }

        #endregion

        #region 事件分发接口

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        public void FireNow(int eventType)
        {
            if (EventDict.TryGetValue(eventType, out var d))
            {
                d.FireNow();
            }
        }

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        public void FireNow<TArg1>(int eventType, TArg1 arg1)
        {
            if (EventDict.TryGetValue(eventType, out var d))
            {
                d.FireNow(arg1);
            }
        }

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        public void FireNow<TArg1, TArg2>(int eventType, TArg1 arg1, TArg2 arg2)
        {
            if (EventDict.TryGetValue(eventType, out var d))
            {
                d.FireNow(arg1, arg2);
            }
        }

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        public void FireNow<TArg1, TArg2, TArg3>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            if (EventDict.TryGetValue(eventType, out var d))
            {
                d.FireNow(arg1, arg2, arg3);
            }
        }

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        public void FireNow<TArg1, TArg2, TArg3, TArg4>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            if (EventDict.TryGetValue(eventType, out var d))
            {
                d.FireNow(arg1, arg2, arg3, arg4);
            }
        }

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <param name="arg5">事件参数5。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        public void FireNow<TArg1, TArg2, TArg3, TArg4, TArg5>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3,
            TArg4 arg4, TArg5 arg5)
        {
            if (EventDict.TryGetValue(eventType, out var d))
            {
                d.FireNow(arg1, arg2, arg3, arg4, arg5);
            }
        }

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <param name="arg5">事件参数5。</param>
        /// <param name="arg6">事件参数6。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        public void FireNow<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3,
            TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            if (EventDict.TryGetValue(eventType, out var d))
            {
                d.FireNow(arg1, arg2, arg3, arg4, arg5, arg6);
            }
        }

        #endregion
    }
}