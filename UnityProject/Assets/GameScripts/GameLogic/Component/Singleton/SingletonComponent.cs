using System;
using System.Collections.Generic;
using LFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameLogic
{
    public class SingletonComponent : MonoBehaviour, ILFrameworkModule, ISingletonManager
    {
        private readonly List<ISingleton> _singletonList = new List<ISingleton>();
        private readonly List<ISingletonUpdate> _singletonUpdateList = new List<ISingletonUpdate>();
        private readonly Dictionary<string, GameObject> _gameObjectDict = new Dictionary<string, GameObject>();
        
        public int Priority
        {
            get
            {
                return 0;
            }
        }

        private void Awake()
        {
            LFrameworkEntry.RegisterModule<ISingletonManager>(this);
        }

        public void OnInit()
        {
            
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (_singletonUpdateList.Count > 0)
            {
                for (int i = 0, len = _singletonUpdateList.Count; i < len; i++)
                {
                    _singletonUpdateList[i].OnUpdate(elapseSeconds, realElapseSeconds);
                }
            }
        }

        public void Shutdown()
        {
            if (_gameObjectDict != null)
            {
                foreach (var item in _gameObjectDict)
                {
                    if(item.Value != null)
                    {
                        bool isMono = item.Value.TryGetComponent<ISingleton>(out var s);
                        if(isMono)
                        {
                            s.Release();
                        }
                        Destroy(item.Value);
                    }
                }
                
                _gameObjectDict.Clear();
            }
            
            if (_singletonList != null)
            {
                for (int i = _singletonList.Count - 1; i >= 0; i--)
                {
                    _singletonList[i].Release();
                }
                
                _singletonList.Clear();
            }
            
            _singletonUpdateList.Clear();
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="singleton">单例</param>
        public void RegisterSingleton(ISingleton singleton)
        {
            _singletonList.Add(singleton);

            RegisterLifeCycle(singleton);
        }

        /// <summary>
        /// 释放单例
        /// </summary>
        /// <param name="singleton">单例</param>
        public void ReleaseSingleton(ISingleton singleton)
        {
            if (singleton != null && _singletonList.Contains(singleton))
            {
                ReleaseLifeCycle(singleton);
                singleton.Release();
                _singletonList.Remove(singleton);
            }
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="singleton">单例</param>
        /// <param name="go">Behaviour单例</param>
        public void RegisterSingleton(ISingleton singleton, GameObject go)
        {
            if (singleton == null)
            {
                throw new LFrameworkException("Singleton is invalid.");
            }

            if (go == null)
            {
                throw new LFrameworkException("Game object is invalid.");
            }

            if (_gameObjectDict.TryAdd(go.name, go))
            {
                RegisterLifeCycle(singleton);
            }
        }

        /// <summary>
        /// 释放单例
        /// </summary>
        /// <param name="singleton">单例</param>
        /// <param name="go">Behaviour单例</param>
        public void ReleaseSingleton(ISingleton singleton, GameObject go)
        {
            if (singleton == null)
            {
                throw new LFrameworkException("Singleton is invalid.");
            }

            if (go == null)
            {
                throw new LFrameworkException("Game object is invalid.");
            }

            if (_gameObjectDict != null && _gameObjectDict.ContainsKey(go.name))
            {
                ReleaseLifeCycle(singleton);
                singleton.Release();
                _gameObjectDict.Remove(go.name);
                Destroy(go);
            }
        }

        /// <summary>
        /// 获取Behaviour单例实体
        /// </summary>
        /// <param name="goName">实体名</param>
        /// <returns>实体</returns>
        public GameObject GetGameObject(string goName)
        {
            GameObject go = null;
            if (_gameObjectDict != null)
            {
                _gameObjectDict.TryGetValue(goName, out go);
            }

            return go;
        }

        private void RegisterLifeCycle(object singleton)
        {
            Type iUpdate = typeof(ISingletonUpdate);
            bool needUpdate = iUpdate.IsInstanceOfType(singleton);
            if (needUpdate && singleton is ISingletonUpdate update)
            {
                _singletonUpdateList.Add(update);
            }
        }

        private void ReleaseLifeCycle(object singleton)
        {
            Type iUpdate = typeof(ISingletonUpdate);
            bool needUpdate = iUpdate.IsInstanceOfType(singleton);
            if (needUpdate && singleton is ISingletonUpdate update)
            {
                if (_singletonUpdateList.Contains(update))
                {
                    _singletonUpdateList.Remove(update);
                }
            }
        }
    }
}