using System;
using LFramework;
using UnityEngine;

namespace GameLogic
{
    public class SingletonBehaviour<T> : MonoBehaviour, ISingleton where T : SingletonBehaviour<T>, new()
    {
        private static T _instance;
        private bool _isRelease;

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
                        return null;
                    }

                    _instance.Init();
                }

                return _instance;
            }
        }

        protected virtual void OnDestroy()
        {
            if (!_isRelease && _instance == this && GameEntry.Singleton != null)
            {
                GameEntry.Singleton.ReleaseSingleton(this, this.gameObject);
            }
        }

        private void Init()
        {
            // 注册进入模块
            GameEntry.Singleton.RegisterSingleton(_instance, _instance.gameObject);
            _isRelease = false;
            OnInit();
        }

        protected virtual void OnInit()
        {
        }

        public void Release()
        {
            _isRelease = true;

            if (_instance != null)
            {
                OnRelease();

                _instance = null;
            }
        }

        protected virtual void OnRelease()
        {
        }
    }
}