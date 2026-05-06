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

        public void Subscribe(int id, Delegate handler)
        {
            if (handler == null)
            {
                throw new LFrameworkException("Event handler is invalid.");
            }

            _eventHandlerDict.Add(id, handler);
            GameEntry.Event.Subscribe(id, handler);
        }

        public void Unsubscribe(int id, Delegate handler)
        {
            if (!_eventHandlerDict.Remove(id, handler))
            {
                throw new LFrameworkException(Utility.Text.Format("Event '{0}' not exists specified handler.",
                    id.ToString()));
            }

            GameEntry.Event.Unsubscribe(id, handler);
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
    }
}