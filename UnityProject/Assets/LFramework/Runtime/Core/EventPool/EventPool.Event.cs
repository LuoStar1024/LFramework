using System;

namespace LFramework
{
    internal sealed partial class EventPool
    {
        /// <summary>
        /// 事件结点。
        /// </summary>
        private abstract class Event : IReference
        {
            protected int Id;
            public abstract void HandleEvent();
            public virtual void Clear()
            {
            }
        }
        
        private class EventArgs : Event
        {
            private Action<int> _handleEvent;
            
            public static EventArgs Create(int id, Action<int> handleEvent)
            {
                EventArgs eventArgs = ReferencePool.Acquire<EventArgs>();
                eventArgs.Id = id;
                eventArgs._handleEvent = handleEvent;
                return eventArgs;
            }
    
            public override void HandleEvent()
            {
                _handleEvent(Id);
            }
            
            public override void Clear()
            {
                _handleEvent = null;
            }
        }
        
        private class EventArgs<TArg1> : Event
        {
            private TArg1 _arg1;
            private Action<int, TArg1> _handleEvent;
    
            public static EventArgs<TArg1> Create(int id, TArg1 arg1, Action<int, TArg1> handleEvent)
            {
                EventArgs<TArg1> eventArgs = ReferencePool.Acquire<EventArgs<TArg1>>();
                eventArgs.Id = id;
                eventArgs._arg1 = arg1;
                eventArgs._handleEvent = handleEvent;
                return eventArgs;
            }
            
            public override void HandleEvent()
            {
                _handleEvent(Id, _arg1);
            }
    
            public override void Clear()
            {
                _arg1 = default(TArg1);
            }
        }
        
        private class EventArgs<TArg1, TArg2> : Event
        {
            private TArg1 _arg1;
            private TArg2 _arg2;
            private Action<int, TArg1, TArg2> _handleEvent;
    
            public static EventArgs<TArg1, TArg2> Create(int id, TArg1 arg1, TArg2 arg2, Action<int, TArg1, TArg2> handleEvent)
            {
                EventArgs<TArg1, TArg2> eventArgs = ReferencePool.Acquire<EventArgs<TArg1, TArg2>>();
                eventArgs.Id = id;
                eventArgs._arg1 = arg1;
                eventArgs._arg2 = arg2;
                eventArgs._handleEvent = handleEvent;
                return eventArgs;
            }
            
            public override void HandleEvent()
            {
                _handleEvent(Id, _arg1, _arg2);
            }
    
            public override void Clear()
            {
                _arg1 = default(TArg1);
                _arg2 = default(TArg2);
            }
        }
        
        private class EventArgs<TArg1, TArg2, TArg3> : Event
        {
            private TArg1 _arg1;
            private TArg2 _arg2;
            private TArg3 _arg3;
            private Action<int, TArg1, TArg2, TArg3> _handleEvent;
    
            public static EventArgs<TArg1, TArg2, TArg3> Create(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, Action<int, TArg1, TArg2, TArg3> handleEvent)
            {
                EventArgs<TArg1, TArg2, TArg3> eventArgs = ReferencePool.Acquire<EventArgs<TArg1, TArg2, TArg3>>();
                eventArgs.Id = id;
                eventArgs._arg1 = arg1;
                eventArgs._arg2 = arg2;
                eventArgs._arg3 = arg3;
                eventArgs._handleEvent = handleEvent;
                return eventArgs;
            }
            
            public override void HandleEvent()
            {
                _handleEvent(Id, _arg1, _arg2, _arg3);
            }
    
            public override void Clear()
            {
                _arg1 = default(TArg1);
                _arg2 = default(TArg2);
                _arg3 = default(TArg3);
            }
        }
        
        private class EventArgs<TArg1, TArg2, TArg3, TArg4> : Event
        {
            private TArg1 _arg1;
            private TArg2 _arg2;
            private TArg3 _arg3;
            private TArg4 _arg4;
            private Action<int, TArg1, TArg2, TArg3, TArg4> _handleEvent;
    
            public static EventArgs<TArg1, TArg2, TArg3, TArg4> Create(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, Action<int, TArg1, TArg2, TArg3, TArg4> handleEvent)
            {
                EventArgs<TArg1, TArg2, TArg3, TArg4> eventArgs = ReferencePool.Acquire<EventArgs<TArg1, TArg2, TArg3, TArg4>>();
                eventArgs.Id = id;
                eventArgs._arg1 = arg1;
                eventArgs._arg2 = arg2;
                eventArgs._arg3 = arg3;
                eventArgs._arg4 = arg4;
                eventArgs._handleEvent = handleEvent;
                return eventArgs;
            }
            
            public override void HandleEvent()
            {
                _handleEvent(Id, _arg1, _arg2, _arg3, _arg4);
            }
    
            public override void Clear()
            {
                _arg1 = default(TArg1);
                _arg2 = default(TArg2);
                _arg3 = default(TArg3);
                _arg4 = default(TArg4);
            }
        }
        
        private class EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5> : Event
        {
            private TArg1 _arg1;
            private TArg2 _arg2;
            private TArg3 _arg3;
            private TArg4 _arg4;
            private TArg5 _arg5;
            private Action<int, TArg1, TArg2, TArg3, TArg4, TArg5> _handleEvent;
    
            public static EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5> Create(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, Action<int, TArg1, TArg2, TArg3, TArg4, TArg5> handleEvent)
            {
                EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5> eventArgs = ReferencePool.Acquire<EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5>>();
                eventArgs.Id = id;
                eventArgs._arg1 = arg1;
                eventArgs._arg2 = arg2;
                eventArgs._arg3 = arg3;
                eventArgs._arg4 = arg4;
                eventArgs._arg5 = arg5;
                eventArgs._handleEvent = handleEvent;
                return eventArgs;
            }
            
            public override void HandleEvent()
            {
                _handleEvent(Id, _arg1, _arg2, _arg3, _arg4, _arg5);
            }
    
            public override void Clear()
            {
                _arg1 = default(TArg1);
                _arg2 = default(TArg2);
                _arg3 = default(TArg3);
                _arg4 = default(TArg4);
                _arg5 = default(TArg5);
            }
        }
        
        private class EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> : Event
        {
            private TArg1 _arg1;
            private TArg2 _arg2;
            private TArg3 _arg3;
            private TArg4 _arg4;
            private TArg5 _arg5;
            private TArg6 _arg6;
            private Action<int, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> _handleEvent;
    
            public static EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> Create(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, Action<int, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> handleEvent)
            {
                EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> eventArgs = ReferencePool.Acquire<EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>>();
                eventArgs.Id = id;
                eventArgs._arg1 = arg1;
                eventArgs._arg2 = arg2;
                eventArgs._arg3 = arg3;
                eventArgs._arg4 = arg4;
                eventArgs._arg5 = arg5;
                eventArgs._arg6 = arg6;
                eventArgs._handleEvent = handleEvent;
                return eventArgs;
            }
            
            public override void HandleEvent()
            {
                _handleEvent(Id, _arg1, _arg2, _arg3, _arg4, _arg5, _arg6);
            }
    
            public override void Clear()
            {
                _arg1 = default(TArg1);
                _arg2 = default(TArg2);
                _arg3 = default(TArg3);
                _arg4 = default(TArg4);
                _arg5 = default(TArg5);
                _arg6 = default(TArg6);
            }
        }
        
        private class EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> : Event
        {
            private TArg1 _arg1;
            private TArg2 _arg2;
            private TArg3 _arg3;
            private TArg4 _arg4;
            private TArg5 _arg5;
            private TArg6 _arg6;
            private TArg7 _arg7;
            private Action<int, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> _handleEvent;
    
            public static EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> Create(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, Action<int, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handleEvent)
            {
                EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> eventArgs = ReferencePool.Acquire<EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>>();
                eventArgs.Id = id;
                eventArgs._arg1 = arg1;
                eventArgs._arg2 = arg2;
                eventArgs._arg3 = arg3;
                eventArgs._arg4 = arg4;
                eventArgs._arg5 = arg5;
                eventArgs._arg6 = arg6;
                eventArgs._arg7 = arg7;
                eventArgs._handleEvent = handleEvent;
                return eventArgs;
            }
            
            public override void HandleEvent()
            {
                _handleEvent(Id, _arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7);
            }
    
            public override void Clear()
            {
                _arg1 = default(TArg1);
                _arg2 = default(TArg2);
                _arg3 = default(TArg3);
                _arg4 = default(TArg4);
                _arg5 = default(TArg5);
                _arg6 = default(TArg6);
                _arg7 = default(TArg7);
            }
        }
        
        private class EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> : Event
        {
            private TArg1 _arg1;
            private TArg2 _arg2;
            private TArg3 _arg3;
            private TArg4 _arg4;
            private TArg5 _arg5;
            private TArg6 _arg6;
            private TArg7 _arg7;
            private TArg8 _arg8;
            private Action<int, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> _handleEvent;
    
            public static EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> Create(int id, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, Action<int, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> handleEvent)
            {
                EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> eventArgs = ReferencePool.Acquire<EventArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>>();
                eventArgs.Id = id;
                eventArgs._arg1 = arg1;
                eventArgs._arg2 = arg2;
                eventArgs._arg3 = arg3;
                eventArgs._arg4 = arg4;
                eventArgs._arg5 = arg5;
                eventArgs._arg6 = arg6;
                eventArgs._arg7 = arg7;
                eventArgs._arg8 = arg8;
                eventArgs._handleEvent = handleEvent;
                return eventArgs;
            }
            
            public override void HandleEvent()
            {
                _handleEvent(Id, _arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7, _arg8);
            }
    
            public override void Clear()
            {
                _arg1 = default(TArg1);
                _arg2 = default(TArg2);
                _arg3 = default(TArg3);
                _arg4 = default(TArg4);
                _arg5 = default(TArg5);
                _arg6 = default(TArg6);
                _arg7 = default(TArg7);
                _arg8 = default(TArg8);
            }
        }
    }
}