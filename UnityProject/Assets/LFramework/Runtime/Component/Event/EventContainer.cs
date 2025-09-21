using System;

namespace LFramework
{
    public sealed class EventContainer : IReference
    {
        private readonly LFrameworkMultiDictionary<int, EventHandler<GameEventArgs>> _eventHandlerDict =
            new LFrameworkMultiDictionary<int, EventHandler<GameEventArgs>>();

        public object Owner
        {
            get;
            private set;
        }

        public static EventContainer Create(object owner)
        {
            EventContainer eventContainer = ReferencePool.Acquire<EventContainer>();
            eventContainer.Owner = owner;
            return eventContainer;
        }

        public void Clear()
        {
            _eventHandlerDict.Clear();
            Owner = null;
        }

        public void Subscribe(int id, EventHandler<GameEventArgs> handler)
        {
            if (handler == null)
            {
                throw new LFrameworkException("Event handler is invalid.");
            }
            _eventHandlerDict.Add(id, handler);
            LFrameworkEntry.GetModule<IEventManager>().Subscribe(id, handler);
        }

        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)
        {
            if (!_eventHandlerDict.Remove(id, handler))
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not exists specified handler.", id.ToString()));
            }
            LFrameworkEntry.GetModule<IEventManager>().Unsubscribe(id, handler);
        }

        public void UnsubscribeAll()
        {
            if (_eventHandlerDict.Count > 0)
            {
                foreach (var item in _eventHandlerDict)
                {
                    foreach (var eventHandler in item.Value)
                    {
                        LFrameworkEntry.GetModule<IEventManager>().Unsubscribe(item.Key, eventHandler);
                    }
                }
                _eventHandlerDict.Clear();
            }
        }
    }
}