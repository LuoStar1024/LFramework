using UnityEngine;

namespace GameLogic
{
    public static partial class Constant
    {
        /// <summary>
        /// 层。
        /// </summary>
        public static class Layer
        {
            public const string DefaultLayerName = "Default";
            public static readonly int DefaultLayerId = LayerMask.NameToLayer(DefaultLayerName);

            public const string UILayerName = "UI";
            public static readonly int UILayerId = LayerMask.NameToLayer(UILayerName);

            public const string PlayerLayerName = "Player";
            public static readonly int PlayerLayerId = LayerMask.NameToLayer(PlayerLayerName);

            public const string EnemyLayerName = "Enemy";
            public static readonly int EnemyLayerId = LayerMask.NameToLayer(EnemyLayerName);
        }
    }
}