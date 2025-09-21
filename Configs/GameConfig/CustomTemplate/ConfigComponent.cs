using System;
using System.Reflection;
using GameConfig;
using LFramework;
using Luban;
using LFramework.Resource;
using SimpleJSON;
using UnityEngine;

/// <summary>
/// 配置加载器。
/// </summary>
public class ConfigComponent
{
    private static ConfigComponent _instance;

    public static ConfigComponent Instance => _instance ??= new ConfigComponent();

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
    
    private IResourceManager _resourceManager;

    /// <summary>
    /// 加载配置。
    /// </summary>
    public void Load()
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
        if (_resourceManager == null)
        {
            _resourceManager = LFrameworkEntry.GetModule<IResourceManager>();
        }

        TextAsset textAsset = null; // _resourceManager.LoadAsset<TextAsset>(file);
        byte[] bytes = textAsset.bytes;
        return new ByteBuf(bytes);
    }

    private JSONNode LoadJsonNode(string file)
    {
        if (_resourceManager == null)
        {
            _resourceManager = LFrameworkEntry.GetModule<IResourceManager>();
        }
        
        TextAsset textAsset = null; // _resourceManager.LoadAsset<TextAsset>(file);
        return JSON.Parse(textAsset.text);
    }
}