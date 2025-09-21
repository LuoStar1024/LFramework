namespace LFramework
{
    /// <summary>
    /// 加载二进制资源回调函数集。
    /// </summary>
    public sealed class LoadBinaryCallbacks
    {
        private readonly LoadBinarySuccessCallback _loadBinarySuccessCallback;
        private readonly LoadBinaryFailureCallback _loadBinaryFailureCallback;

        /// <summary>
        /// 初始化加载二进制资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadBinarySuccessCallback">加载二进制资源成功回调函数。</param>
        public LoadBinaryCallbacks(LoadBinarySuccessCallback loadBinarySuccessCallback)
            : this(loadBinarySuccessCallback, null)
        {
        }

        /// <summary>
        /// 初始化加载二进制资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadBinarySuccessCallback">加载二进制资源成功回调函数。</param>
        /// <param name="loadBinaryFailureCallback">加载二进制资源失败回调函数。</param>
        public LoadBinaryCallbacks(LoadBinarySuccessCallback loadBinarySuccessCallback, LoadBinaryFailureCallback loadBinaryFailureCallback)
        {
            if (loadBinarySuccessCallback == null)
            {
                throw new LFrameworkException("Load binary success callback is invalid.");
            }

            _loadBinarySuccessCallback = loadBinarySuccessCallback;
            _loadBinaryFailureCallback = loadBinaryFailureCallback;
        }

        /// <summary>
        /// 获取加载二进制资源成功回调函数。
        /// </summary>
        public LoadBinarySuccessCallback LoadBinarySuccessCallback
        {
            get
            {
                return _loadBinarySuccessCallback;
            }
        }

        /// <summary>
        /// 获取加载二进制资源失败回调函数。
        /// </summary>
        public LoadBinaryFailureCallback LoadBinaryFailureCallback
        {
            get
            {
                return _loadBinaryFailureCallback;
            }
        }
    }
}