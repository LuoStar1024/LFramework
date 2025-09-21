using System.Runtime.InteropServices;

namespace LFramework
{
    /// <summary>
    /// 任务信息。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct TaskInfo
    {
        private readonly bool _isValid;
        private readonly int _serialId;
        private readonly string _tag;
        private readonly int _priority;
        private readonly object _userData;
        private readonly TaskStatus _status;
        private readonly string _description;

        /// <summary>
        /// 初始化任务信息的新实例。
        /// </summary>
        /// <param name="serialId">任务的序列编号。</param>
        /// <param name="tag">任务的标签。</param>
        /// <param name="priority">任务的优先级。</param>
        /// <param name="userData">任务的用户自定义数据。</param>
        /// <param name="status">任务状态。</param>
        /// <param name="description">任务描述。</param>
        public TaskInfo(int serialId, string tag, int priority, object userData, TaskStatus status, string description)
        {
            _isValid = true;
            _serialId = serialId;
            _tag = tag;
            _priority = priority;
            _userData = userData;
            _status = status;
            _description = description;
        }

        /// <summary>
        /// 获取任务信息是否有效。
        /// </summary>
        public bool IsValid
        {
            get { return _isValid; }
        }

        /// <summary>
        /// 获取任务的序列编号。
        /// </summary>
        public int SerialId
        {
            get
            {
                if (!_isValid)
                {
                    throw new LFrameworkException("Data is invalid.");
                }

                return _serialId;
            }
        }

        /// <summary>
        /// 获取任务的标签。
        /// </summary>
        public string Tag
        {
            get
            {
                if (!_isValid)
                {
                    throw new LFrameworkException("Data is invalid.");
                }

                return _tag;
            }
        }

        /// <summary>
        /// 获取任务的优先级。
        /// </summary>
        public int Priority
        {
            get
            {
                if (!_isValid)
                {
                    throw new LFrameworkException("Data is invalid.");
                }

                return _priority;
            }
        }

        /// <summary>
        /// 获取任务的用户自定义数据。
        /// </summary>
        public object UserData
        {
            get
            {
                if (!_isValid)
                {
                    throw new LFrameworkException("Data is invalid.");
                }

                return _userData;
            }
        }

        /// <summary>
        /// 获取任务状态。
        /// </summary>
        public TaskStatus Status
        {
            get
            {
                if (!_isValid)
                {
                    throw new LFrameworkException("Data is invalid.");
                }

                return _status;
            }
        }

        /// <summary>
        /// 获取任务描述。
        /// </summary>
        public string Description
        {
            get
            {
                if (!_isValid)
                {
                    throw new LFrameworkException("Data is invalid.");
                }

                return _description;
            }
        }
    }
}