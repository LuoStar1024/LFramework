using System;
using System.Reflection;
using GameDataTable;
using LFramework;
using Luban;
using SimpleJSON;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 配置表组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("LFramework/DataTable")]
    public class DataTableComponent : MonoBehaviour, ILFrameworkModule, IDataTableManager
    {
        private bool _init = false;

        private Tables _tables;

        public Tables Tables
        {
            get
            {
                if (!_init)
                {
                    Load();
                }

                return _tables;
            }
        }

        public int Priority => 0;

        private void Awake()
        {
            LFrameworkEntry.RegisterModule<IDataTableManager>(this);
        }

        public void OnInit()
        {
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Shutdown()
        {
        }

        /// <summary>
        /// 加载配置。
        /// </summary>
        private void Load()
        {
            Type tablesType = typeof(Tables);
            MethodInfo loadMethodInfo = tablesType.GetMethod("SetDefaultLoader");
            if (loadMethodInfo == null)
            {
                return;
            }

            Type loaderReturnType = loadMethodInfo.GetParameters()[0].ParameterType.GetGenericArguments()[1];
            if (loaderReturnType == typeof(ByteBuf))
            {
                _tables = new Tables(LoadByteBuf);
                _init = true;
            }
            else
            {
                _tables = new Tables(LoadJsonNode);
                _init = true;
            }
        }

        /// <summary>
        /// 加载二进制配置。
        /// </summary>
        /// <param name="file">FileName</param>
        /// <returns>ByteBuf</returns>
        private ByteBuf LoadByteBuf(string file)
        {
            string assetPath = AssetUtility.GetDataTableAsset(file);
            TextAsset textAsset = GameEntry.Resource.LoadExistAsset<TextAsset>(assetPath);
            if (textAsset == null)
            {
                throw new LFrameworkException($"Data table asset is not loaded: {assetPath}");
            }

            byte[] bytes = textAsset.bytes;
            return new ByteBuf(bytes);
        }

        private JSONNode LoadJsonNode(string file)
        {
            string assetPath = AssetUtility.GetDataTableAsset(file);
            TextAsset textAsset = GameEntry.Resource.LoadExistAsset<TextAsset>(assetPath);
            if (textAsset == null)
            {
                throw new LFrameworkException($"Data table asset is not loaded: {assetPath}");
            }

            return JSON.Parse(textAsset.text);
        }
    }
}