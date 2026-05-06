using System.Runtime.InteropServices;

namespace LFramework
{
    /// <summary>
    /// 定时器信息。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct TimerInfo
    {
        private readonly int _id;
        private readonly string _className;
        private readonly string _methodName;
        private readonly float _time;
        private readonly int _repeatCount;
        private readonly float _curTime;

        /// <summary>
        /// 初始化定时器信息的新实例。
        /// </summary>
        /// <param name="id">定时器Id。</param>
        /// <param name="className">定时器类名。</param>
        /// <param name="methodName">定时器方法名。</param>
        /// <param name="time">定时器时间。</param>
        /// <param name="repeatCount">定时器次数。</param>
        /// <param name="curTime">定时器剩余时间。</param>
        public TimerInfo(int id, string className, string methodName, float time, int repeatCount, float curTime)
        {
            _id = id;
            _className = className;
            _methodName = methodName;
            _time = time;
            _repeatCount = repeatCount;
            _curTime = curTime;
        }

        public int Id
        {
            get { return _id; }
        }

        public string ClassName
        {
            get { return _className; }
        }

        public string MethodName
        {
            get { return _methodName; }
        }

        public float Time
        {
            get { return _time; }
        }

        public int RepeatCount
        {
            get { return _repeatCount; }
        }

        public float CurTime
        {
            get { return _curTime; }
        }
    }
}