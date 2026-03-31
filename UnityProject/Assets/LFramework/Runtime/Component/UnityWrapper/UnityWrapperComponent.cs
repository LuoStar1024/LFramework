using UnityEngine;
using UnityEngine.Scripting;

namespace LFramework
{
    /// <summary>
    /// Unity组件。负责提供一些依赖Unity的桥接接口。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("LFramework/UnityWrapper")]
    [Preserve]
    public sealed partial class UnityWrapperComponent : MonoBehaviour, ILFrameworkModule, IUnityWrapperManager
    {
        public int Priority
        {
            get
            {
                return 0;
            }
        }

        private void Awake()
        {
            LFrameworkEntry.RegisterModule<IUnityWrapperManager>(this);
            
            // 防止裁剪引用。
            // 如果在主工程无引用，link.xml的防裁剪也无效。
            // 最好是AOT显示保留引用，Preserve有可能还会裁成员变量。
            //UnityEngine.Physics
            RegisterType<Collider>();
            RegisterType<Collider2D>();
            RegisterType<Collision>();
            RegisterType<Collision2D>();
            RegisterType<CapsuleCollider2D>();

            RegisterType<Rigidbody>();
            RegisterType<Rigidbody2D>();
        
            RegisterType<Ray>();
            RegisterType<Ray2D>();

            //UnityEngine.Graphics
            RegisterType<Mesh>();
            RegisterType<MeshRenderer>();

            //UnityEngine.Animation
            RegisterType<AnimationClip>();
            RegisterType<AnimationCurve>();
            RegisterType<AnimationEvent>();
            RegisterType<AnimationState>();
            RegisterType<Animator>();
            RegisterType<Animation>();

#if UNITY_IOS || PLATFORM_IOS
        /* 
        // IOSCamera ios下相机权限的问题，用这种方法就可以解决了 问题防裁剪。
        foreach (var _ in WebCamTexture.devices)
        {
        } 
        */ 
#endif
        }

        public void OnInit()
        {
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理声音管理器。
        /// </summary>
        public void Shutdown()
        {
        }
        
        private void RegisterType<T>()
        {
#if UNITY_EDITOR && false
      Debug.Log($"DisStripCode RegisterType :{typeof(T)}");
#endif
        }
    }
}