using System;
using System.Collections.Generic;

namespace LFramework
{
    internal sealed partial class EventPool
    {
        private class EventData : IReference
        {
            private int _eventType = 0;
            private readonly List<Delegate> _existList = new List<Delegate>();
            private readonly List<Delegate> _addList = new List<Delegate>();
            private readonly List<Delegate> _deleteList = new List<Delegate>();
            private bool _isExecute = false;
            private bool _isDirty = false;

            /// <summary>
            /// 订阅事件处理委托。
            /// </summary>
            /// <param name="handler">事件处理回调。</param>
            /// <returns>是否添加回调成功。</returns>
            internal bool Subscribe(Delegate handler)
            {
                if (_existList.Contains(handler))
                {
                    LFrameworkLog.Fatal("Repeated Add Handler");
                    return false;
                }

                if (_isExecute)
                {
                    _isDirty = true;
                    _addList.Add(handler);
                }
                else
                {
                    _existList.Add(handler);
                }

                return true;
            }

            /// <summary>
            /// 取消订阅事件处理委托。
            /// </summary>
            /// <param name="handler">事件处理回调。</param>
            internal void Unsubscribe(Delegate handler)
            {
                if (_isExecute)
                {
                    _isDirty = true;
                    _deleteList.Add(handler);
                }
                else
                {
                    if (!_existList.Remove(handler))
                    {
                        LFrameworkLog.Fatal("Delete handle failed, not exist, EventId: {0}",
                            EventRuntimeId.ToString(_eventType));
                    }
                }
            }

            /// <summary>
            /// 检测脏数据修正。稍后移除或增加数据
            /// </summary>
            private void CheckModify()
            {
                _isExecute = false;
                if (_isDirty)
                {
                    for (int i = 0; i < _addList.Count; i++)
                    {
                        _existList.Add(_addList[i]);
                    }

                    _addList.Clear();

                    for (int i = 0; i < _deleteList.Count; i++)
                    {
                        _existList.Remove(_deleteList[i]);
                    }

                    _deleteList.Clear();
                }
            }

            /// <summary>
            /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
            /// </summary>
            public void FireNow()
            {
                _isExecute = true;
                for (var i = 0; i < _existList.Count; i++)
                {
                    var d = _existList[i];
                    if (d is LFrameworkAction action)
                    {
                        action();
                    }
                }

                CheckModify();
            }

            /// <summary>
            /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
            /// </summary>
            /// <param name="arg1">事件参数1。</param>
            /// <typeparam name="TArg1">事件参数1类型。</typeparam>
            public void FireNow<TArg1>(TArg1 arg1)
            {
                _isExecute = true;
                for (var i = 0; i < _existList.Count; i++)
                {
                    var d = _existList[i];
                    if (d is LFrameworkAction<TArg1> action)
                    {
                        action(arg1);
                    }
                }

                CheckModify();
            }

            /// <summary>
            /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
            /// </summary>
            /// <param name="arg1">事件参数1。</param>
            /// <param name="arg2">事件参数2。</param>
            /// <typeparam name="TArg1">事件参数1类型。</typeparam>
            /// <typeparam name="TArg2">事件参数2类型。</typeparam>
            public void FireNow<TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
            {
                _isExecute = true;
                for (var i = 0; i < _existList.Count; i++)
                {
                    var d = _existList[i];
                    if (d is LFrameworkAction<TArg1, TArg2> action)
                    {
                        action(arg1, arg2);
                    }
                }

                CheckModify();
            }

            /// <summary>
            /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
            /// </summary>
            /// <param name="arg1">事件参数1。</param>
            /// <param name="arg2">事件参数2。</param>
            /// <param name="arg3">事件参数3。</param>
            /// <typeparam name="TArg1">事件参数1类型。</typeparam>
            /// <typeparam name="TArg2">事件参数2类型。</typeparam>
            /// <typeparam name="TArg3">事件参数3类型。</typeparam>
            public void FireNow<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
            {
                _isExecute = true;
                for (var i = 0; i < _existList.Count; i++)
                {
                    var d = _existList[i];
                    if (d is LFrameworkAction<TArg1, TArg2, TArg3> action)
                    {
                        action(arg1, arg2, arg3);
                    }
                }

                CheckModify();
            }

            /// <summary>
            /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
            /// </summary>
            /// <param name="arg1">事件参数1。</param>
            /// <param name="arg2">事件参数2。</param>
            /// <param name="arg3">事件参数3。</param>
            /// <param name="arg4">事件参数4。</param>
            /// <typeparam name="TArg1">事件参数1类型。</typeparam>
            /// <typeparam name="TArg2">事件参数2类型。</typeparam>
            /// <typeparam name="TArg3">事件参数3类型。</typeparam>
            /// <typeparam name="TArg4">事件参数4类型。</typeparam>
            public void FireNow<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
            {
                _isExecute = true;
                for (var i = 0; i < _existList.Count; i++)
                {
                    var d = _existList[i];
                    if (d is LFrameworkAction<TArg1, TArg2, TArg3, TArg4> action)
                    {
                        action(arg1, arg2, arg3, arg4);
                    }
                }

                CheckModify();
            }

            /// <summary>
            /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
            /// </summary>
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
            public void FireNow<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4,
                TArg5 arg5)
            {
                _isExecute = true;
                for (var i = 0; i < _existList.Count; i++)
                {
                    var d = _existList[i];
                    if (d is LFrameworkAction<TArg1, TArg2, TArg3, TArg4, TArg5> action)
                    {
                        action(arg1, arg2, arg3, arg4, arg5);
                    }
                }

                CheckModify();
            }

            /// <summary>
            /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
            /// </summary>
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
            public void FireNow<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TArg1 arg1, TArg2 arg2, TArg3 arg3,
                TArg4 arg4, TArg5 arg5, TArg6 arg6)
            {
                _isExecute = true;
                for (var i = 0; i < _existList.Count; i++)
                {
                    var d = _existList[i];
                    if (d is LFrameworkAction<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> action)
                    {
                        action(arg1, arg2, arg3, arg4, arg5, arg6);
                    }
                }

                CheckModify();
            }

            public static EventData Create(int eventType)
            {
                EventData eventData = ReferencePool.Acquire<EventData>();
                eventData._eventType = eventType;
                return eventData;
            }

            public void Clear()
            {
                _eventType = 0;
                _existList.Clear();
                _addList.Clear();
                _deleteList.Clear();
                _isExecute = false;
                _isDirty = false;
            }
        }
    }
}