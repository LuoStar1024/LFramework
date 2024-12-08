namespace LFramework
{
    /// <summary>
    /// 事件管理器接口。
    /// </summary>
    public interface IEventManager
    {
        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <returns>是否订阅成功。</returns>
        bool Subscribe(int eventType, LFrameworkAction handler);

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        bool Subscribe<TArg1>(int eventType, LFrameworkAction<TArg1> handler);

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数1类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        bool Subscribe<TArg1, TArg2>(int eventType, LFrameworkAction<TArg1, TArg2> handler);

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        bool Subscribe<TArg1, TArg2, TArg3>(int eventType, LFrameworkAction<TArg1, TArg2, TArg3> handler);

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        bool Subscribe<TArg1, TArg2, TArg3, TArg4>(int eventType,
            LFrameworkAction<TArg1, TArg2, TArg3, TArg4> handler);

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        bool Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5>(int eventType,
            LFrameworkAction<TArg1, TArg2, TArg3, TArg4, TArg5> handler);

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        bool Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int eventType,
            LFrameworkAction<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> handler);

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <returns>是否订阅成功。</returns>
        void Unsubscribe(int eventType, LFrameworkAction handler);

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        void Unsubscribe<TArg1>(int eventType, LFrameworkAction<TArg1> handler);

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数1类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        void Unsubscribe<TArg1, TArg2>(int eventType, LFrameworkAction<TArg1, TArg2> handler);

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        void Unsubscribe<TArg1, TArg2, TArg3>(int eventType, LFrameworkAction<TArg1, TArg2, TArg3> handler);

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        void Unsubscribe<TArg1, TArg2, TArg3, TArg4>(int eventType,
            LFrameworkAction<TArg1, TArg2, TArg3, TArg4> handler);

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5>(int eventType,
            LFrameworkAction<TArg1, TArg2, TArg3, TArg4, TArg5> handler);

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <returns>是否订阅成功。</returns>
        void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int eventType,
            LFrameworkAction<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> handler);

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        void FireNow(int eventType);

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1类型。</param>
        void FireNow<TArg1>(int eventType, TArg1 arg1);

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1类型。</param>
        /// <param name="arg2">事件参数2类型。</param>
        void FireNow<TArg1, TArg2>(int eventType, TArg1 arg1, TArg2 arg2);

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1类型。</param>
        /// <param name="arg2">事件参数2类型。</param>
        /// <param name="arg3">事件参数3类型。</param>
        void FireNow<TArg1, TArg2, TArg3>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1类型。</param>
        /// <param name="arg2">事件参数2类型。</param>
        /// <param name="arg3">事件参数3类型。</param>
        /// <param name="arg4">事件参数4类型。</param>
        void FireNow<TArg1, TArg2, TArg3, TArg4>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1类型。</param>
        /// <param name="arg2">事件参数2类型。</param>
        /// <param name="arg3">事件参数3类型。</param>
        /// <param name="arg4">事件参数4类型。</param>
        /// <param name="arg5">事件参数5类型。</param>
        void FireNow<TArg1, TArg2, TArg3, TArg4, TArg5>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4,
            TArg5 arg5);

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1类型。</param>
        /// <param name="arg2">事件参数2类型。</param>
        /// <param name="arg3">事件参数3类型。</param>
        /// <param name="arg4">事件参数4类型。</param>
        /// <param name="arg5">事件参数5类型。</param>
        /// <param name="arg6">事件参数6类型。</param>
        void FireNow<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3,
            TArg4 arg4, TArg5 arg5, TArg6 arg6);
    }
}