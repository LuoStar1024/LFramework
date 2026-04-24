using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using LFramework;

namespace GameLogic
{
    public class UguiForm : UIFormLogic
    {
        public const int DepthFactor = 10;

        private Canvas _cachedCanvas = null;

        private readonly List<ParticleSystemRenderer> _cachedParticleSystemRenderersContainer = new List<ParticleSystemRenderer>();

        private readonly List<Canvas> _cachedCanvasContainer = new List<Canvas>();

        private UIWidgetContainer _uiWidgetContainer;
        private EventContainer _eventContainer;
        private ResourceContainer _resourceContainer;

        public int OriginalDepth { get; private set; }

        public int Depth => _cachedCanvas.sortingOrder;

        public virtual void Close()
        {
            GameEntry.UI.CloseUIForm(this.UIForm);
        }

        public virtual void PlayUISound(int uiSoundId)
        {
            GameEntry.Audio.PlayUISound(uiSoundId);
        }

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            _cachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            _cachedCanvas.overrideSorting = true;
            OriginalDepth = _cachedCanvas.sortingOrder;
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        protected internal override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            int oldDepth = Depth;
            base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            int deltaDepth = UIGroup.DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup - oldDepth + OriginalDepth;
            GetComponentsInChildren(true, _cachedCanvasContainer);
            for (int i = 0; i < _cachedCanvasContainer.Count; i++)
            {
                _cachedCanvasContainer[i].sortingOrder += deltaDepth;
            }
            _cachedCanvasContainer.Clear();
            GetComponentsInChildren(true, _cachedParticleSystemRenderersContainer);
            foreach (var t in _cachedParticleSystemRenderersContainer)
            {
                t.sortingOrder += deltaDepth;
            }
            _cachedParticleSystemRenderersContainer.Clear();

            _uiWidgetContainer?.OnDepthChanged(uiGroupDepth, depthInUIGroup);
        }

        private void ClearUIForm()
        {
            if (_eventContainer != null)
            {
                ReferencePool.Release(_eventContainer);
                _eventContainer = null;
            }
            if (_uiWidgetContainer != null)
            {
                ReferencePool.Release(_uiWidgetContainer);
                _uiWidgetContainer = null;
            }
            if (_resourceContainer != null)
            {
                ReferencePool.Release(_resourceContainer);
                _resourceContainer = null;
            }
        }

        protected internal override void OnRecycle()
        {
            base.OnRecycle();
            _uiWidgetContainer?.OnRecycle();
        }

        private void OnDestroy()
        {
            RemoveAllUIWidget();
            ClearUIForm();
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            _uiWidgetContainer?.OnClose(isShutdown, userData);
            UnsubscribeAll();
            UnloadAllAssets();
            CloseAllUIWidgets(userData, isShutdown);
            if (isShutdown)
            {
                RemoveAllUIWidget();
                ClearUIForm();
            }
            base.OnClose(isShutdown, userData);
        }

        protected internal override void OnPause()
        {
            base.OnPause();
            _uiWidgetContainer?.OnPause();
        }

        protected internal override void OnResume()
        {
            base.OnResume();
            _uiWidgetContainer?.OnResume();
        }

        protected internal override void OnCover()
        {
            base.OnCover();
            _uiWidgetContainer?.OnCover();
        }

        protected internal override void OnReveal()
        {
            base.OnReveal();
            _uiWidgetContainer?.OnReveal();
        }

        protected internal override void OnRefocus(object userData)
        {
            base.OnRefocus(userData);
            _uiWidgetContainer?.OnRefocus(userData);
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            _uiWidgetContainer?.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        public void AddUIWidget(UIWidget auiWidget, object userData = default)
        {
            if (_uiWidgetContainer == null)
            {
                _uiWidgetContainer = UIWidgetContainer.Create(this);
            }
            _uiWidgetContainer.AddUIWidget(auiWidget, userData);
        }

        public void RemoveUIWidget(UIWidget auiWidget)
        {
            if (_uiWidgetContainer == null)
            {
                throw new LFrameworkException("Container is empty!");
            }
            _uiWidgetContainer.RemoveUIWidget(auiWidget);
        }

        public void RemoveAllUIWidget()
        {
            if (_uiWidgetContainer == null)
                return;
            _uiWidgetContainer.RemoveAllUIWidget();
        }

        /// <summary>
        /// 打开UIWidget，不刷新Depth，一般在UIForm的OnOpen中调用
        /// </summary>
        /// <param name="auiWidget"></param>
        /// <param name="userData"></param>
        /// <exception cref="LFrameworkException"></exception>
        public void OpenUIWidget(UIWidget auiWidget, object userData = default)
        {
            if (_uiWidgetContainer == null)
            {
                throw new LFrameworkException("Container is empty!");
            }
            _uiWidgetContainer.OpenUIWidget(auiWidget, userData);
        }

        /// <summary>
        /// 动态打开UIWidget，刷新Depth
        /// </summary>
        /// <param name="auiWidget"></param>
        /// <param name="userData"></param>
        /// <exception cref="LFrameworkException"></exception>
        public void DynamicOpenUIWidget(UIWidget auiWidget, object userData = default)
        {
            if (_uiWidgetContainer == null)
            {
                throw new LFrameworkException("Container is empty!");
            }
            _uiWidgetContainer.DynamicOpenUIWidget(auiWidget, userData);
        }

        public void CloseUIWidget(UIWidget uiWidget, object userData = default, bool isShutdown = false)
        {
            if (_uiWidgetContainer == null)
            {
                throw new LFrameworkException("Container is empty!");
            }
            _uiWidgetContainer.CloseUIWidget(uiWidget, userData, isShutdown);
        }

        public void CloseAllUIWidgets(object userData = default, bool isShutdown = false)
        {
            if (_uiWidgetContainer == null)
                return;
            _uiWidgetContainer.CloseAllUIWidgets(userData, isShutdown);
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        public void Subscribe(int id, Action handler)
        {
            SubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        public void Subscribe<TArg1>(int id, Action<TArg1> handler)
        {
            SubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        public void Subscribe<TArg1, TArg2>(int id, Action<TArg1, TArg2> handler)
        {
            SubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3>(int id, Action<TArg1, TArg2, TArg3> handler)
        {
            SubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3, TArg4>(int id, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            SubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            SubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> handler)
        {
            SubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handler)
        {
            SubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        /// <typeparam name="TArg8">事件参数8类型。</typeparam>
        public void Subscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> handler)
        {
            SubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        public void Unsubscribe(int id, Action handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        public void Unsubscribe<TArg1>(int id, Action<TArg1> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2>(int id, Action<TArg1, TArg2> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3>(int id, Action<TArg1, TArg2, TArg3> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4>(int id, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <typeparam name="TArg7">事件参数7类型。</typeparam>
        /// <typeparam name="TArg8">事件参数8类型。</typeparam>
        public void Unsubscribe<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(int id, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> handler)
        {
            UnsubscribeDelegate(id, handler);
        }

        public void UnsubscribeAll()
        {
            if (_eventContainer == null)
                return;
            _eventContainer.UnsubscribeAll();
        }

        private void SubscribeDelegate(int id, Delegate handler)
        {
            if (_eventContainer == null)
            {
                _eventContainer = EventContainer.Create(this);
            }
            _eventContainer.Subscribe(id, handler);
        }

        private void UnsubscribeDelegate(int id, Delegate handler)
        {
            if (_eventContainer == null)
                return;
            _eventContainer.Unsubscribe(id, handler);
        }

        public async UniTask<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object
        {
            if (_resourceContainer == null)
            {
                _resourceContainer = ResourceContainer.Create(this);
            }
            return await _resourceContainer.LoadAsset<T>(assetName);
        }

        public void UnloadAsset(UnityEngine.Object asset)
        {
            if (_resourceContainer == null)
                return;
            _resourceContainer.UnloadAsset(asset);
        }

        public void UnloadAllAssets()
        {
            if (_resourceContainer == null)
                return;
            _resourceContainer.UnloadAllAssets();
        }
    }
}