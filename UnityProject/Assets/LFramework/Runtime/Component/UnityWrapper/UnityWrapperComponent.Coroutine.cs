using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Internal;

namespace LFramework
{
    public sealed partial class UnityWrapperComponent
    {
        #region 控制协程Coroutine

        public Coroutine StartCoroutineWrapper(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return null;
            }

            return StartCoroutine(methodName);
        }

        public Coroutine StartCoroutineWrapper(IEnumerator routine)
        {
            if (routine == null)
            {
                return null;
            }
            
            return StartCoroutine(routine);
        }

        public Coroutine StartCoroutineWrapper(string methodName, [DefaultValue("null")] object value)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return null;
            }

            return StartCoroutine(methodName, value);
        }

        public void StopCoroutineWrapper(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return;
            }

            StopCoroutine(methodName);
        }

        public void StopCoroutineWrapper(IEnumerator routine)
        {
            if (routine == null)
            {
                return;
            }

            StopCoroutine(routine);
        }

        public void StopCoroutineWrapper(Coroutine routine)
        {
            if (routine == null)
            {
                return;
            }

            StopCoroutine(routine);
            routine = null;
        }

        public void StopAllCoroutinesWrapper()
        {
            StopAllCoroutines();
        }

        #endregion
    }
}