using UnityEngine;

namespace GameLogic
{
    public interface ISingleton
    {
        // /// <summary>
        // /// 注册自己进入模块
        // /// </summary>
        // void Init();

        /// <summary>
        /// 释放自己
        /// </summary>
        /// <param name="isSelf">自己主动释放</param>
        void Release(bool isSelf = false);
    }
    
    public interface ISingletonUpdate
    {
        /// <summary>
        /// 游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        void OnUpdate(float elapseSeconds, float realElapseSeconds);
    }
}