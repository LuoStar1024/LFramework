using System;
using System.Collections.Generic;

namespace LFramework
{
    /// <summary>
    /// 事件池。
    /// </summary>
    /// <typeparam name="T">事件类型。</typeparam>
    internal sealed partial class EventPool
    {
        private readonly LFrameworkMultiDictionary<int, Delegate> _eventHandlers;
        private readonly Queue<Event> _events;
        private readonly Dictionary<int, LinkedListNode<Delegate>> _cachedNodes;
        private readonly Dictionary<int, LinkedListNode<Delegate>> _tempNodes;
        private readonly EventPoolMode _eventPoolMode;
        private Action<int> _defaultHandler;
        private readonly Dictionary<string, object> _eventGroupDict;

        /// <summary>
        /// 初始化事件池的新实例。
        /// </summary>
        /// <param name="mode">事件池模式。</param>
        public EventPool(EventPoolMode mode)
        {
            _eventHandlers = new LFrameworkMultiDictionary<int, Delegate>();
            _events = new Queue<Event>();
            _cachedNodes = new Dictionary<int, LinkedListNode<Delegate>>();
            _tempNodes = new Dictionary<int, LinkedListNode<Delegate>>();
            _eventPoolMode = mode;
            _defaultHandler = null;
            _eventGroupDict = new Dictionary<string, object>();
        }

        /// <summary>
        /// 获取事件处理函数的数量。
        /// </summary>
        public int EventHandlerCount
        {
            get { return _eventHandlers.Count; }
        }

        /// <summary>
        /// 获取事件数量。
        /// </summary>
        public int EventCount
        {
            get { return _events.Count; }
        }

        /// <summary>
        /// 事件池轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            lock (_events)
            {
                while (_events.Count > 0)
                {
                    Event eventNode = _events.Dequeue();
                    eventNode.HandleEvent();
                    ReferencePool.Release(eventNode);
                }
            }
        }

        /// <summary>
        /// 关闭并清理事件池。
        /// </summary>
        public void Shutdown()
        {
            Clear();
            _eventHandlers.Clear();
            _cachedNodes.Clear();
            _tempNodes.Clear();
            _defaultHandler = null;
            _eventGroupDict.Clear();
        }

        /// <summary>
        /// 清理事件。
        /// </summary>
        public void Clear()
        {
            lock (_events)
            {
                while (_events.Count > 0)
                {
                    Event eventNode = _events.Dequeue();
                    ReferencePool.Release(eventNode);
                }
            }
        }

        /// <summary>
        /// 获取事件处理函数的数量。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <returns>事件处理函数的数量。</returns>
        public int Count(int id)
        {
            LFrameworkLinkedListRange<Delegate> range = default(LFrameworkLinkedListRange<Delegate>);
            if (_eventHandlers.TryGetValue(id, out range))
            {
                return range.Count;
            }

            return 0;
        }

        /// <summary>
        /// 检查是否存在事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要检查的事件处理函数。</param>
        /// <returns>是否存在事件处理函数。</returns>
        public bool Check(int id, Delegate handler)
        {
            if (handler == null)
            {
                throw new LFrameworkException("Event handler is invalid.");
            }

            return _eventHandlers.Contains(id, handler);
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        public void Subscribe(int id, Action handler)
        {
            SubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        public void Subscribe<TArg1>(int id, Action<TArg1> handler)
        {
            SubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        public void Subscribe<TArg1, TArg2>(int id, Action<TArg1, TArg2> handler)
        {
            SubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3>(int id, Action<TArg1, TArg2, TArg3> handler)
        {
            SubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3, TArg4>(int id, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            SubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            SubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> handler)
        {
            SubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handler)
        {
            SubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        /// <typeparam name="TArg8">事件参数8类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> handler)
        {
            SubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        public void SubscribeDelegate(int id, Delegate handler)
        {
            if (handler == null)
            {
                throw new LFrameworkException("Event handler is invalid.");
            }

            if (!_eventHandlers.Contains(id))
            {
                _eventHandlers.Add(id, handler);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowMultiHandler) != EventPoolMode.AllowMultiHandler)
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not allow multi handler.", id));
            }
            else if ((_eventPoolMode & EventPoolMode.AllowDuplicateHandler) != EventPoolMode.AllowDuplicateHandler &&
                     Check(id, handler))
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not allow duplicate handler.", id));
            }
            else
            {
                _eventHandlers.Add(id, handler);
            }
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        public void Unsubscribe(int id, Action handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        public void Unsubscribe<TArg1>(int id, Action<TArg1> handler)
        {
            UnsubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2>(int id, Action<TArg1, TArg2> handler)
        {
            UnsubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3>(int id, Action<TArg1, TArg2, TArg3> handler)
        {
            UnsubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4>(int id, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            UnsubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            UnsubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> handler)
        {
            UnsubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handler)
        {
            UnsubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        /// <typeparam name="TArg8">事件参数8类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> handler)
        {
            UnsubscribeDelegate(id, handler);
        }
        
        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        public void UnsubscribeDelegate(int id, Delegate handler)
        {
            if (handler == null)
            {
                throw new LFrameworkException("Event handler is invalid.");
            }

            if (_cachedNodes.Count > 0)
            {
                foreach (KeyValuePair<int, LinkedListNode<Delegate>> cachedNode in _cachedNodes)
                {
                    if (cachedNode.Key == id && cachedNode.Value != null && cachedNode.Value.Value == handler)
                    {
                        _tempNodes.Add(cachedNode.Key, cachedNode.Value.Next);
                    }
                }

                if (_tempNodes.Count > 0)
                {
                    foreach (KeyValuePair<int, LinkedListNode<Delegate>> cachedNode in _tempNodes)
                    {
                        _cachedNodes[cachedNode.Key] = cachedNode.Value;
                    }

                    _tempNodes.Clear();
                }
            }

            if (!_eventHandlers.Remove(id, handler))
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not exists specified handler.", id));
            }
        }
        
        /// <summary>
        /// 设置默认事件处理函数。
        /// </summary>
        /// <param name="handler">要设置的默认事件处理函数。</param>
        public void SetDefaultHandler(Action<int> handler)
        {
            _defaultHandler = handler;
        }

        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        public void Fire(int id)
        {
            Event eventNode = EventArgs.Create(id, HandleEvent);
            lock (_events)
            {
                _events.Enqueue(eventNode);
            }
        }
        
        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        public void Fire<TArg1>(int id, TArg1 arg1)
        {
            Event eventNode = EventArgs<TArg1>.Create(id, arg1, HandleEvent);
            lock (_events)
            {
                _events.Enqueue(eventNode);
            }
        }
        
        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        public void Fire<TArg1, TArg2>(int id, TArg1 arg1, TArg2 arg2)
        {
            Event eventNode = EventArgs<TArg1, TArg2>.Create(id, arg1, arg2, HandleEvent);
            lock (_events)
            {
                _events.Enqueue(eventNode);
            }
        }
        
        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        public void Fire<TArg1, TArg2, TArg3>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            Event eventNode = EventArgs<TArg1, TArg2, TArg3>.Create(id, arg1, arg2, arg3, HandleEvent);
            lock (_events)
            {
                _events.Enqueue(eventNode);
            }
        }
        
        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        public void Fire<TArg1, TArg2, TArg3, TArg4>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            Event eventNode = EventArgs<TArg1, TArg2, TArg3, TArg4>.Create(id, arg1, arg2, arg3, arg4, HandleEvent);
            lock (_events)
            {
                _events.Enqueue(eventNode);
            }
        }
        
        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
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
        public void Fire<TArg1, TArg2, TArg3, TArg4, TArg5>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            Event eventNode = EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5>.Create(id, arg1, arg2, arg3, arg4, arg5, HandleEvent);
            lock (_events)
            {
                _events.Enqueue(eventNode);
            }
        }
        
        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
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
        public void Fire<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            Event eventNode = EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>.Create(id, arg1, arg2, arg3, arg4, arg5, arg6, HandleEvent);
            lock (_events)
            {
                _events.Enqueue(eventNode);
            }
        }
        
        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <param name="arg5">事件参数5。</param>
        /// <param name="arg6">事件参数6。</param>
        /// <param name="arg7">事件参数7。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        public void Fire<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            Event eventNode = EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>.Create(id, arg1, arg2, arg3, arg4, arg5, arg6, arg7, HandleEvent);
            lock (_events)
            {
                _events.Enqueue(eventNode);
            }
        }
        
        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <param name="arg5">事件参数5。</param>
        /// <param name="arg6">事件参数6。</param>
        /// <param name="arg7">事件参数7。</param>
        /// <param name="arg8">事件参数8。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        /// <typeparam name="TArg8">事件参数8类型。</typeparam>
        public void Fire<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            Event eventNode = EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>.Create(id, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, HandleEvent);
            lock (_events)
            {
                _events.Enqueue(eventNode);
            }
        }

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        public void FireNow(int id)
        {
            HandleEvent(id);
        }
        
        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        public void FireNow<TArg1>(int id, TArg1 arg1)
        {
            HandleEvent(id, arg1);
        }
        
        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        public void FireNow<TArg1, TArg2>(int id, TArg1 arg1, TArg2 arg2)
        {
            HandleEvent(id, arg1, arg2);
        }
        
        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        public void FireNow<TArg1, TArg2, TArg3>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            HandleEvent(id, arg1, arg2, arg3);
        }
        
        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        public void FireNow<TArg1, TArg2, TArg3, TArg4>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            HandleEvent(id, arg1, arg2, arg3, arg4);
        }
        
        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
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
        public void FireNow<TArg1, TArg2, TArg3, TArg4, TArg5>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            HandleEvent(id, arg1, arg2, arg3, arg4, arg5);
        }
        
        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
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
        public void FireNow<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            HandleEvent(id, arg1, arg2, arg3, arg4, arg5, arg6);
        }
        
        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <param name="arg5">事件参数5。</param>
        /// <param name="arg6">事件参数6。</param>
        /// <param name="arg7">事件参数7。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        public void FireNow<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            HandleEvent(id, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
        
        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <param name="arg5">事件参数5。</param>
        /// <param name="arg6">事件参数6。</param>
        /// <param name="arg7">事件参数7。</param>
        /// <param name="arg8">事件参数8。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        /// <typeparam name="TArg8">事件参数8类型。</typeparam>
        public void FireNow<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            HandleEvent(id, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        /// <summary>
        /// 注册事件组。
        /// </summary>
        /// <param name="group">事件组实例。</param>
        /// <typeparam name="T">事件组接口类型。</typeparam>
        public void RegisterGroup<T>(T group)
        {
            string groupName = typeof(T).FullName;
            if (groupName != null)
            {
                _eventGroupDict[groupName] = group;
            }
        }

        /// <summary>
        /// 获取事件组。
        /// </summary>
        /// <typeparam name="T">事件组接口类型。</typeparam>
        /// <returns>事件组实例。</returns>
        public T FireGroup<T>()
        {
            string groupName = typeof(T).FullName;
            if (groupName != null && _eventGroupDict.TryGetValue(groupName, out var group))
            {
                return (T)group;
            }

            throw new LFrameworkException(Utility.Text.Format("Event group '{0}' is not exist.",
                typeof(T).FullName));
        }
        
        /// <summary>
        /// 处理事件结点。
        /// </summary>
        /// <param name="id">事件Id。</param>
        private void HandleEvent(int id)
        {
            bool noHandlerException = false;
            LFrameworkLinkedListRange<Delegate> range = default(LFrameworkLinkedListRange<Delegate>);
            if (_eventHandlers.TryGetValue(id, out range))
            {
                LinkedListNode<Delegate> current = range.First;
                while (current != null && current != range.Terminal)
                {
                    _cachedNodes[id] = current.Next != range.Terminal ? current.Next : null;
                    if (current.Value is Action action)
                    {
                        action();
                    }
                    current = _cachedNodes[id];
                }

                _cachedNodes.Remove(id);
            }
            else if (_defaultHandler != null)
            {
                _defaultHandler(id);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }

            if (noHandlerException)
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not allow no handler.", id));
            }
        }
        
        /// <summary>
        /// 处理事件结点。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        private void HandleEvent<TArg1>(int id, TArg1 arg1)
        {
            bool noHandlerException = false;
            LFrameworkLinkedListRange<Delegate> range = default(LFrameworkLinkedListRange<Delegate>);
            if (_eventHandlers.TryGetValue(id, out range))
            {
                LinkedListNode<Delegate> current = range.First;
                while (current != null && current != range.Terminal)
                {
                    _cachedNodes[id] = current.Next != range.Terminal ? current.Next : null;
                    if (current.Value is Action<TArg1> action)
                    {
                        action(arg1);
                    }
                    current = _cachedNodes[id];
                }

                _cachedNodes.Remove(id);
            }
            else if (_defaultHandler != null)
            {
                _defaultHandler(id);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }

            if (noHandlerException)
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not allow no handler.", id));
            }
        }
        
        /// <summary>
        /// 处理事件结点。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        private void HandleEvent<TArg1, TArg2>(int id, TArg1 arg1, TArg2 arg2)
        {
            bool noHandlerException = false;
            LFrameworkLinkedListRange<Delegate> range = default(LFrameworkLinkedListRange<Delegate>);
            if (_eventHandlers.TryGetValue(id, out range))
            {
                LinkedListNode<Delegate> current = range.First;
                while (current != null && current != range.Terminal)
                {
                    _cachedNodes[id] = current.Next != range.Terminal ? current.Next : null;
                    if (current.Value is Action<TArg1, TArg2> action)
                    {
                        action(arg1, arg2);
                    }
                    current = _cachedNodes[id];
                }

                _cachedNodes.Remove(id);
            }
            else if (_defaultHandler != null)
            {
                _defaultHandler(id);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }

            if (noHandlerException)
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not allow no handler.", id));
            }
        }
        
        /// <summary>
        /// 处理事件结点。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        private void HandleEvent<TArg1, TArg2, TArg3>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            bool noHandlerException = false;
            LFrameworkLinkedListRange<Delegate> range = default(LFrameworkLinkedListRange<Delegate>);
            if (_eventHandlers.TryGetValue(id, out range))
            {
                LinkedListNode<Delegate> current = range.First;
                while (current != null && current != range.Terminal)
                {
                    _cachedNodes[id] = current.Next != range.Terminal ? current.Next : null;
                    if (current.Value is Action<TArg1, TArg2, TArg3> action)
                    {
                        action(arg1, arg2, arg3);
                    }
                    current = _cachedNodes[id];
                }

                _cachedNodes.Remove(id);
            }
            else if (_defaultHandler != null)
            {
                _defaultHandler(id);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }

            if (noHandlerException)
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not allow no handler.", id));
            }
        }
        
        /// <summary>
        /// 处理事件结点。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        private void HandleEvent<TArg1, TArg2, TArg3, TArg4>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            bool noHandlerException = false;
            LFrameworkLinkedListRange<Delegate> range = default(LFrameworkLinkedListRange<Delegate>);
            if (_eventHandlers.TryGetValue(id, out range))
            {
                LinkedListNode<Delegate> current = range.First;
                while (current != null && current != range.Terminal)
                {
                    _cachedNodes[id] = current.Next != range.Terminal ? current.Next : null;
                    if (current.Value is Action<TArg1, TArg2, TArg3, TArg4> action)
                    {
                        action(arg1, arg2, arg3, arg4);
                    }
                    current = _cachedNodes[id];
                }

                _cachedNodes.Remove(id);
            }
            else if (_defaultHandler != null)
            {
                _defaultHandler(id);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }

            if (noHandlerException)
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not allow no handler.", id));
            }
        }
        
        /// <summary>
        /// 处理事件结点。
        /// </summary>
        /// <param name="id">事件Id。</param>
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
        private void HandleEvent<TArg1, TArg2, TArg3, TArg4, TArg5>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            bool noHandlerException = false;
            LFrameworkLinkedListRange<Delegate> range = default(LFrameworkLinkedListRange<Delegate>);
            if (_eventHandlers.TryGetValue(id, out range))
            {
                LinkedListNode<Delegate> current = range.First;
                while (current != null && current != range.Terminal)
                {
                    _cachedNodes[id] = current.Next != range.Terminal ? current.Next : null;
                    if (current.Value is Action<TArg1, TArg2, TArg3, TArg4, TArg5> action)
                    {
                        action(arg1, arg2, arg3, arg4, arg5);
                    }
                    current = _cachedNodes[id];
                }

                _cachedNodes.Remove(id);
            }
            else if (_defaultHandler != null)
            {
                _defaultHandler(id);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }

            if (noHandlerException)
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not allow no handler.", id));
            }
        }
        
        /// <summary>
        /// 处理事件结点。
        /// </summary>
        /// <param name="id">事件Id。</param>
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
        private void HandleEvent<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            bool noHandlerException = false;
            LFrameworkLinkedListRange<Delegate> range = default(LFrameworkLinkedListRange<Delegate>);
            if (_eventHandlers.TryGetValue(id, out range))
            {
                LinkedListNode<Delegate> current = range.First;
                while (current != null && current != range.Terminal)
                {
                    _cachedNodes[id] = current.Next != range.Terminal ? current.Next : null;
                    if (current.Value is Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> action)
                    {
                        action(arg1, arg2, arg3, arg4, arg5, arg6);
                    }
                    current = _cachedNodes[id];
                }

                _cachedNodes.Remove(id);
            }
            else if (_defaultHandler != null)
            {
                _defaultHandler(id);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }

            if (noHandlerException)
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not allow no handler.", id));
            }
        }
        
        /// <summary>
        /// 处理事件结点。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <param name="arg5">事件参数5。</param>
        /// <param name="arg6">事件参数6。</param>
        /// <param name="arg7">事件参数7。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        private void HandleEvent<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            bool noHandlerException = false;
            LFrameworkLinkedListRange<Delegate> range = default(LFrameworkLinkedListRange<Delegate>);
            if (_eventHandlers.TryGetValue(id, out range))
            {
                LinkedListNode<Delegate> current = range.First;
                while (current != null && current != range.Terminal)
                {
                    _cachedNodes[id] = current.Next != range.Terminal ? current.Next : null;
                    if (current.Value is Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> action)
                    {
                        action(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                    }
                    current = _cachedNodes[id];
                }

                _cachedNodes.Remove(id);
            }
            else if (_defaultHandler != null)
            {
                _defaultHandler(id);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }

            if (noHandlerException)
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not allow no handler.", id));
            }
        }
        
        /// <summary>
        /// 处理事件结点。
        /// </summary>
        /// <param name="id">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <param name="arg5">事件参数5。</param>
        /// <param name="arg6">事件参数6。</param>
        /// <param name="arg7">事件参数7。</param>
        /// <param name="arg8">事件参数8。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        /// <typeparam name="TArg8">事件参数8类型。</typeparam>
        private void HandleEvent<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8)
        {
            bool noHandlerException = false;
            LFrameworkLinkedListRange<Delegate> range = default(LFrameworkLinkedListRange<Delegate>);
            if (_eventHandlers.TryGetValue(id, out range))
            {
                LinkedListNode<Delegate> current = range.First;
                while (current != null && current != range.Terminal)
                {
                    _cachedNodes[id] = current.Next != range.Terminal ? current.Next : null;
                    if (current.Value is Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> action)
                    {
                        action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                    }
                    current = _cachedNodes[id];
                }

                _cachedNodes.Remove(id);
            }
            else if (_defaultHandler != null)
            {
                _defaultHandler(id);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }

            if (noHandlerException)
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not allow no handler.", id));
            }
        }
    }
}