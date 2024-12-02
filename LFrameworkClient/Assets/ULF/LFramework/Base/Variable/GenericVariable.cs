using System;

namespace LFramework
{
    /// <summary>
    /// 变量。
    /// </summary>
    /// <typeparam name="T">变量类型。</typeparam>
    public abstract class Variable<T> : Variable
    {
        private T _value;

        /// <summary>
        /// 初始化变量的新实例。
        /// </summary>
        public Variable()
        {
            _value = default(T);
        }

        /// <summary>
        /// 获取变量类型。
        /// </summary>
        public override Type Type
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// 获取或设置变量值。
        /// </summary>
        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// 获取变量值。
        /// </summary>
        /// <returns>变量值。</returns>
        public override object GetValue()
        {
            return _value;
        }

        /// <summary>
        /// 设置变量值。
        /// </summary>
        /// <param name="value">变量值。</param>
        public override void SetValue(object value)
        {
            _value = (T)value;
        }

        /// <summary>
        /// 清理变量值。
        /// </summary>
        public override void Clear()
        {
            _value = default(T);
        }

        /// <summary>
        /// 获取变量字符串。
        /// </summary>
        /// <returns>变量字符串。</returns>
        public override string ToString()
        {
            return (_value != null) ? _value.ToString() : "<Null>";
        }
    }
}