using System;
using LFramework;
using UnityEngine;

namespace GameLogic
{
    public class SingletonBehaviour<T> : MonoBehaviour, ISingleton where T : SingletonBehaviour<T>, new()
    {
        private static T _instance;
        
        /// <summary>
        /// 实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    System.Type thisType = typeof(T);
                    string instName = thisType.Name;
                    GameObject go = GameEntry.Singleton.GetGameObject(instName);
                    if (go == null)
                    {
                        go = GameObject.Find($"/{instName}");
                        if (go == null)
                        {
                            go = new GameObject(instName)
                            {
                                transform =
                                {
                                    position = Vector3.zero
                                }
                            };
                        }
                    }

                    if (go != null)
                    {
                        _instance = go.GetComponent<T>();
                        if (_instance == null)
                        {
                            _instance = go.AddComponent<T>();
                        }
                    }

                    if (_instance == null)
                    {
                        Debug.LogError($"Can't create SingletonBehaviour<{typeof(T)}>");
                    }
                    
                    _instance.Init();
                }

                return _instance;
            }
        }

        protected virtual void OnDestroy()
        {
            if (this == _instance)
            {
                Release(false);
            }
        }

        private void Init()
        {
            // 注册进入模块
            GameEntry.Singleton.RegisterSingleton(_instance, _instance.gameObject);
            OnInit();
        }
        
        protected virtual void OnInit()
        {
        }
        
        public void Release(bool isSelf = true)
        {
            if (_instance != null)
            {
                OnRelease();
                
                // 主动调用这个，需要销毁游戏对象，否则是直接消耗对象等OnDestroy调用
                if (isSelf)
                {
                    Destroy(_instance.gameObject);
                }
                GameEntry.Singleton.ReleaseSingleton(_instance, _instance.gameObject);
                _instance = null;
            }
        }
        
        protected virtual void OnRelease()
        {
        }
    }
}