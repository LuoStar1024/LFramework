using UnityEngine;

namespace GameLogic
{
    public interface ISingletonManager
    {
        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="singleton">单例</param>
        void RegisterSingleton(ISingleton singleton);
        
        /// <summary>
        /// 释放单例
        /// </summary>
        /// <param name="singleton">单例</param>
        void ReleaseSingleton(ISingleton singleton);
        
        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="singleton">单例</param>
        /// <param name="go">Behaviour单例</param>
        void RegisterSingleton(ISingleton singleton, GameObject go);
        
        /// <summary>
        /// 释放单例
        /// </summary>
        /// <param name="singleton">单例</param>
        /// <param name="go">Behaviour单例</param>
        void ReleaseSingleton(ISingleton singleton, GameObject go);

        /// <summary>
        /// 获取Behaviour单例实体
        /// </summary>
        /// <param name="goName">实体名</param>
        /// <returns>实体</returns>
        GameObject GetGameObject(string goName);
    }
}