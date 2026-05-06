using System.Buffers;
using YooAsset;

namespace LFramework
{
    public sealed partial class ResourceComponent
    {
        /// <summary>
        /// 资源对象。
        /// </summary>
        private sealed class AssetObject : ObjectBase
        {
            private AssetHandle _assetHandle = null;

            public static AssetObject Create(string name, object target, object assetHandle)
            {
                if (assetHandle == null)
                {
                    throw new LFrameworkException("Resource is invalid.");
                }

                AssetObject assetObject = ReferencePool.Acquire<AssetObject>();
                assetObject.Initialize(name, target);
                assetObject._assetHandle = (AssetHandle)assetHandle;
                return assetObject;
            }

            public override void Clear()
            {
                base.Clear();
                _assetHandle = null;
            }

            protected internal override void Release(bool isShutdown)
            {
                if (!isShutdown)
                {
                    AssetHandle handle = _assetHandle;
                    if (_assetHandle is { IsValid: true })
                    {
                        handle.Dispose();
                    }

                    handle = null;
                }
            }
        }
    }
}