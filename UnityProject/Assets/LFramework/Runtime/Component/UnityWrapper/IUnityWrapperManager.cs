using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Internal;

namespace LFramework
{
    /// <summary>
    /// 声音管理器接口。
    /// </summary>
    public interface IUnityWrapperManager
    {
        #region 控制协程Coroutine

        public Coroutine StartCoroutineWrapper(string methodName);

        public Coroutine StartCoroutineWrapper(IEnumerator routine);

        public Coroutine StartCoroutineWrapper(string methodName, [DefaultValue("null")] object value);

        public void StopCoroutineWrapper(string methodName);

        public void StopCoroutineWrapper(IEnumerator routine);

        public void StopCoroutineWrapper(Coroutine routine);

        public void StopAllCoroutinesWrapper();

        #endregion
    }
}