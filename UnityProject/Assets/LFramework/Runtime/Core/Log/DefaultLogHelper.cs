using UnityEngine;

namespace LFramework
{
    /// <summary>
    /// 默认游戏框架日志辅助器。
    /// </summary>
    public class DefaultLogHelper : LFrameworkLog.ILogHelper
    {
        /// <summary>
        /// 记录日志。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <param name="message">日志内容。</param>
        public void Log(LFrameworkLogLevel level, object message)
        {
            switch (level)
            {
                case LFrameworkLogLevel.Debug:
                    Debug.Log(Utility.Text.Format("<color=#888888>{0}</color>", message));
                    break;

                case LFrameworkLogLevel.Info:
                    Debug.Log(message.ToString());
                    break;

                case LFrameworkLogLevel.Warning:
                    Debug.LogWarning(message.ToString());
                    break;

                case LFrameworkLogLevel.Error:
                    Debug.LogError(message.ToString());
                    break;

                default:
                    throw new LFrameworkException(message.ToString());
            }
        }
    }
}