using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LFramework;

namespace GameLogic
{
    public class ResourceContainer : IReference
    {
        private readonly List<UnityEngine.Object> _assetList = new List<UnityEngine.Object>();
        private CancellationTokenSource _cancellationTokenSource;
        
        public object Owner
        {
            get;
            private set;
        }
        
        public static ResourceContainer Create(object owner)
        {
            ResourceContainer resourceContainer = ReferencePool.Acquire<ResourceContainer>();
            resourceContainer.Owner = owner;
            return resourceContainer;
        }
        
        public void Clear()
        {
            _assetList.Clear();
            _cancellationTokenSource = null;
            Owner = null;
        }
        
        public async UniTask<T> LoadAsset<T>(string assetName, int priority = 0, string packageName = "") where T : UnityEngine.Object
        {
            if (_cancellationTokenSource == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }
            T asset = await GameEntry.Resource.LoadAsset<T>(assetName, priority, _cancellationTokenSource.Token, packageName);
            _assetList.Add(asset);
            return asset;
        }

        public void UnloadAsset(UnityEngine.Object asset)
        {
            _assetList.Remove(asset);
            GameEntry.Resource.UnloadAsset(asset);
        }

        public void UnloadAllAssets()
        {
            if (_assetList.Count > 0)
            {
                foreach (var asset in _assetList)
                {
                    GameEntry.Resource.UnloadAsset(asset);
                }
                _assetList.Clear();
            }
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
        }
    }
}