using System;
using LFramework;

namespace GameLogic
{
    public class EventContainer : IReference
    {
        private readonly LFrameworkMultiDictionary<int, Delegate> _eventHandlerDict =
            new LFrameworkMultiDictionary<int, Delegate>();

        public object Owner { get; private set; }

        public static EventContainer Create(object owner)
        {
            EventContainer eventContainer = ReferencePool.Acquire<EventContainer>();
            eventContainer.Owner = owner;
            return eventContainer;
        }

        public void Clear()
        {
            UnsubscribeAll();
            _eventHandlerDict.Clear();
            Owner = null;
        }

        public void Subscribe(int id, Action handler)
        {
            SubscribeDelegate(id, handler);
        }

        public void Subscribe<TArg1>(int id, Action<TArg1> handler)
        {
            SubscribeDelegate(id, handler);
        }

        public void Subscribe<TArg1, TArg2>(int id, Action<TArg1, TArg2> handler)
        {
            SubscribeDelegate(id, handler);
        }

        public void Subscribe<TArg1, TArg2, TArg3>(int id, Action<TArg1, TArg2, TArg3> handler)
        {
            SubscribeDelegate(id, handler);
        }

        public void Subscribe<TArg1, TArg2, TArg3, TArg4>(int id, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            SubscribeDelegate(id, handler);
        }

        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5>(int id,
            Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            SubscribeDelegate(id, handler);
        }

        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int id,
            Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> handler)
        {
            SubscribeDelegate(id, handler);
        }

        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(int id,
            Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handler)
        {
            SubscribeDelegate(id, handler);
        }

        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(int id,
            Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> handler)
        {
            SubscribeDelegate(id, handler);
        }

        public void Subscribe(int id, Delegate handler)
        {
            SubscribeDelegate(id, handler);
        }

        public void Unsubscribe(int id, Action handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        public void Unsubscribe<TArg1>(int id, Action<TArg1> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        public void Unsubscribe<TArg1, TArg2>(int id, Action<TArg1, TArg2> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        public void Unsubscribe<TArg1, TArg2, TArg3>(int id, Action<TArg1, TArg2, TArg3> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4>(int id, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5>(int id,
            Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int id,
            Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(int id,
            Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(int id,
            Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        public void Unsubscribe(int id, Delegate handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        public void UnsubscribeAll()
        {
            if (_eventHandlerDict.Count > 0)
            {
                foreach (var item in _eventHandlerDict)
                {
                    foreach (var eventHandler in item.Value)
                    {
                        try
                        {
                            GameEntry.Event.Unsubscribe(item.Key, eventHandler);
                        }
                        catch (Exception exception)
                        {
                            Log.Warning("Unsubscribe event '{0}' failure, reason '{1}'.", item.Key.ToString(),
                                exception.Message);
                        }
                    }
                }

                _eventHandlerDict.Clear();
            }
        }

        private void SubscribeDelegate(int id, Delegate handler)
        {
            if (handler == null)
            {
                throw new LFrameworkException("Event handler is invalid.");
            }

            _eventHandlerDict.Add(id, handler);
            GameEntry.Event.Subscribe(id, handler);
        }

        private void UnsubscribeDelegate(int id, Delegate handler)
        {
            if (!_eventHandlerDict.Remove(id, handler))
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not exists specified handler.",
                    id.ToString()));
            }

            GameEntry.Event.Unsubscribe(id, handler);
        }
    }
}
