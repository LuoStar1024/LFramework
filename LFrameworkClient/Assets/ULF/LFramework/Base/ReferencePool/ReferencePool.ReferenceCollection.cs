using System;
using System.Collections.Generic;

namespace LFramework
{
    public static partial class ReferencePool
    {
        private sealed class ReferenceCollection
        {
            private readonly Queue<IReference> _referenceQueue;
            private readonly Type _referenceType;
            private int _usingReferenceCount;
            private int _acquireReferenceCount;
            private int _releaseReferenceCount;
            private int _addReferenceCount;
            private int _removeReferenceCount;

            public ReferenceCollection(Type referenceType)
            {
                _referenceQueue = new Queue<IReference>();
                _referenceType = referenceType;
                _usingReferenceCount = 0;
                _acquireReferenceCount = 0;
                _releaseReferenceCount = 0;
                _addReferenceCount = 0;
                _removeReferenceCount = 0;
            }

            public Type ReferenceType
            {
                get { return _referenceType; }
            }

            public int UnusedReferenceCount
            {
                get { return _referenceQueue.Count; }
            }

            public int UsingReferenceCount
            {
                get { return _usingReferenceCount; }
            }

            public int AcquireReferenceCount
            {
                get { return _acquireReferenceCount; }
            }

            public int ReleaseReferenceCount
            {
                get { return _releaseReferenceCount; }
            }

            public int AddReferenceCount
            {
                get { return _addReferenceCount; }
            }

            public int RemoveReferenceCount
            {
                get { return _removeReferenceCount; }
            }

            public T Acquire<T>() where T : class, IReference, new()
            {
                if (typeof(T) != _referenceType)
                {
                    throw new LFrameworkException("Type is invalid.");
                }

                _usingReferenceCount++;
                _acquireReferenceCount++;
                lock (_referenceQueue)
                {
                    if (_referenceQueue.Count > 0)
                    {
                        return (T)_referenceQueue.Dequeue();
                    }
                }

                _addReferenceCount++;
                return new T();
            }

            public IReference Acquire()
            {
                _usingReferenceCount++;
                _acquireReferenceCount++;
                lock (_referenceQueue)
                {
                    if (_referenceQueue.Count > 0)
                    {
                        return _referenceQueue.Dequeue();
                    }
                }

                _addReferenceCount++;
                return (IReference)Activator.CreateInstance(_referenceType);
            }

            public void Release(IReference reference)
            {
                reference.Clear();
                lock (_referenceQueue)
                {
                    if (_enableStrictCheck && _referenceQueue.Contains(reference))
                    {
                        throw new LFrameworkException("The reference has been released.");
                    }

                    _referenceQueue.Enqueue(reference);
                }

                _releaseReferenceCount++;
                _usingReferenceCount--;
            }

            public void Add<T>(int count) where T : class, IReference, new()
            {
                if (typeof(T) != _referenceType)
                {
                    throw new LFrameworkException("Type is invalid.");
                }

                lock (_referenceQueue)
                {
                    _addReferenceCount += count;
                    while (count-- > 0)
                    {
                        _referenceQueue.Enqueue(new T());
                    }
                }
            }

            public void Add(int count)
            {
                lock (_referenceQueue)
                {
                    _addReferenceCount += count;
                    while (count-- > 0)
                    {
                        _referenceQueue.Enqueue((IReference)Activator.CreateInstance(_referenceType));
                    }
                }
            }

            public void Remove(int count)
            {
                lock (_referenceQueue)
                {
                    if (count > _referenceQueue.Count)
                    {
                        count = _referenceQueue.Count;
                    }

                    _removeReferenceCount += count;
                    while (count-- > 0)
                    {
                        _referenceQueue.Dequeue();
                    }
                }
            }

            public void RemoveAll()
            {
                lock (_referenceQueue)
                {
                    _removeReferenceCount += _referenceQueue.Count;
                    _referenceQueue.Clear();
                }
            }
        }
    }
}