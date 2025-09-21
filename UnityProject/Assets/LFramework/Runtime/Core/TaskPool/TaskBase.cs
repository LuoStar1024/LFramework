namespace LFramework
{
    /// <summary>
    /// 任务基类。
    /// </summary>
    internal abstract class TaskBase : IReference
    {
        /// <summary>
        /// 任务默认优先级。
        /// </summary>
        public const int DefaultPriority = 0;

        private int _serialId;
        private string _tag;
        private int _priority;
        private object _userData;

        private bool _done;

        /// <summary>
        /// 初始化任务基类的新实例。
        /// </summary>
        public TaskBase()
        {
            _serialId = 0;
            _tag = null;
            _priority = DefaultPriority;
            _done = false;
            _userData = null;
        }

        /// <summary>
        /// 获取任务的序列编号。
        /// </summary>
        public int SerialId
        {
            get { return _serialId; }
        }

        /// <summary>
        /// 获取任务的标签。
        /// </summary>
        public string Tag
        {
            get { return _tag; }
        }

        /// <summary>
        /// 获取任务的优先级。
        /// </summary>
        public int Priority
        {
            get { return _priority; }
        }

        /// <summary>
        /// 获取任务的用户自定义数据。
        /// </summary>
        public object UserData
        {
            get { return _userData; }
        }

        /// <summary>
        /// 获取或设置任务是否完成。
        /// </summary>
        public bool Done
        {
            get { return _done; }
            set { _done = value; }
        }

        /// <summary>
        /// 获取任务描述。
        /// </summary>
        public virtual string Description
        {
            get { return null; }
        }

        /// <summary>
        /// 初始化任务基类。
        /// </summary>
        /// <param name="serialId">任务的序列编号。</param>
        /// <param name="tag">任务的标签。</param>
        /// <param name="priority">任务的优先级。</param>
        /// <param name="userData">任务的用户自定义数据。</param>
        internal void Initialize(int serialId, string tag, int priority, object userData)
        {
            _serialId = serialId;
            _tag = tag;
            _priority = priority;
            _userData = userData;
            _done = false;
        }

        /// <summary>
        /// 清理任务基类。
        /// </summary>
        public virtual void Clear()
        {
            _serialId = 0;
            _tag = null;
            _priority = DefaultPriority;
            _userData = null;
            _done = false;
        }
    }
}