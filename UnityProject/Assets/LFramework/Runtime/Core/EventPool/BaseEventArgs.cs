namespace LFramework
{
    /// <summary>
    /// 事件基类。
    /// </summary>
    public abstract class BaseEventArgs : LFrameworkEventArgs
    {
        /// <summary>
        /// 获取类型编号。
        /// </summary>
        public abstract int Id { get; }
    }
}