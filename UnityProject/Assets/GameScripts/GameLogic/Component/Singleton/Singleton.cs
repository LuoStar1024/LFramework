using System.Diagnostics;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 全局对象必须继承于此。
    /// </summary>
    /// <typeparam name="T">子类类型。</typeparam>
    public abstract class Singleton<T> : ISingleton where T : Singleton<T>, new()
    {
        private static T _instance = default(T);

        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new T();
                    _instance.Init();
                }

                return _instance;
            }
        }

        protected Singleton()
        {
#if UNITY_EDITOR
            string st = new StackTrace().ToString();
            // using const string to compare simply
            if (!st.Contains("GameLogic.Singleton`1[T].get_Instance"))
            {
                UnityEngine.Debug.LogError($"请必须通过Instance方法来实例化{typeof(T).FullName}类");
            }
#endif
        }

        private void Init()
        {
            // 注册进入模块
            GameEntry.Singleton.RegisterSingleton(_instance);
            OnInit();
        }

        protected virtual void OnInit()
        {
        }

        public virtual void Release()
        {
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