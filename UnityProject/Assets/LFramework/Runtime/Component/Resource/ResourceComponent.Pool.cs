namespace LFramework
{
    public sealed partial class ResourceComponent
    {
        private IObjectPool<AssetObject> _assetPool;

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float AssetAutoReleaseInterval
        {
            get => _assetPool.AutoReleaseInterval;
            set => _assetPool.AutoReleaseInterval = value;
        }

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        public int AssetCapacity
        {
            get => _assetPool.Capacity;
            set => _assetPool.Capacity = value;
        }

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        public float AssetExpireTime
        {
            get => _assetPool.ExpireTime;
            set => _assetPool.ExpireTime = value;
        }

        /// <summary>
        /// 获取或设置资源对象池的优先级。
        /// </summary>
        public int AssetPriority
        {
            get => _assetPool.Priority;
            set => _assetPool.Priority = value;
        }

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
        {
            if (objectPoolManager == null)
            {
                throw new LFrameworkException("Object pool manager is invalid.");
            }

            _assetPool = objectPoolManager.CreateMultiSpawnObjectPool<AssetObject>("Asset Pool");
            AssetAutoReleaseInterval = assetAutoReleaseInterval;
            AssetCapacity = assetCapacity;
            AssetExpireTime = assetExpireTime;
            AssetPriority = assetPriority;
        }
    }
}