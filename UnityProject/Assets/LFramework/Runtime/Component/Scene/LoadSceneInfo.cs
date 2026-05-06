using System;

namespace LFramework
{
    internal sealed class LoadSceneInfo : IReference
    {
        private Action<float> _progressCallback;
        private Action<bool> _loadSuccessCallBack;
        private object _userData;

        public LoadSceneInfo()
        {
            _progressCallback = null;
            _loadSuccessCallBack = null;
            _userData = null;
        }

        public Action<float> ProgressCallback => _progressCallback;

        public Action<bool> LoadSuccessCallBack => _loadSuccessCallBack;

        public object UserData => _userData;

        public static LoadSceneInfo Create(object userData, Action<float> progressCallback,
            Action<bool> loadSuccessCallBack)
        {
            LoadSceneInfo loadSceneInfo = ReferencePool.Acquire<LoadSceneInfo>();
            loadSceneInfo._progressCallback = progressCallback;
            loadSceneInfo._loadSuccessCallBack = loadSuccessCallBack;
            loadSceneInfo._userData = userData;
            return loadSceneInfo;
        }

        public void Clear()
        {
            _progressCallback = null;
            _loadSuccessCallBack = null;
            _userData = null;
        }
    }
}