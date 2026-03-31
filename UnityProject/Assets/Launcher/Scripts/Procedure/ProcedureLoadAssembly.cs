using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if ENABLE_HYBRIDCLR
using HybridCLR;
#endif
using UnityEngine;
using System.Reflection;
using YooAsset;
using Cysharp.Threading.Tasks;
using LFramework;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    /// <summary>
    /// 加载 HybridCLR 热更程序集与 AOT 补充元数据，完成启动器到游戏逻辑的最后准备。
    /// </summary>
    public class ProcedureLoadAssembly : ProcedureBase
    {
        private bool _enableAddressable = true;
        public override bool UseNativeDialog => true;
        private int _loadAssetCount;
        private int _loadMetadataAssetCount;
        private int _failureAssetCount;
        private int _failureMetadataAssetCount;
        private bool _loadAssemblyComplete;
        private bool _loadMetadataAssemblyComplete;
        private bool _loadAssemblyWait;
        private bool _loadMetadataAssemblyWait;
        private Assembly _mainLogicAssembly;
        private List<Assembly> _hotfixAssemblyList;
        private ProcedureOwner _procedureOwner;
        private UpdateConfig _setting;
        
        private IResourceManager _resComponent;
        private IConfigManager _configComponent;
        private ISettingManager _settingComponent;

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            _resComponent = LFrameworkEntry.GetModule<IResourceManager>();
            _configComponent = LFrameworkEntry.GetModule<IConfigManager>();
            _settingComponent = LFrameworkEntry.GetModule<ISettingManager>();
            
            _setting = _configComponent.UpdateConfig;
            
            Log.Debug("HybridCLR ProcedureLoadAssembly OnEnter");
            _procedureOwner = procedureOwner;
            // 程序集加载量较大，使用异步流程避免阻塞启动器界面刷新。
            LoadAssembly().Forget();
        }

        private async UniTaskVoid LoadAssembly()
        {
            _loadAssemblyComplete = false;
            _hotfixAssemblyList = new List<Assembly>();

            // 先补充 AOT 元数据，避免热更代码触发泛型/反射时缺少运行时信息。
            if (_setting.Enable)
            {
#if !UNITY_EDITOR
                _loadMetadataAssemblyComplete = false;
                LoadMetadataForAOTAssembly();
#else
                _loadMetadataAssemblyComplete = true;
#endif
            }
            else
            {
                _loadMetadataAssemblyComplete = true;
            }

            if (!_setting.Enable || _resComponent.ResourceMode == ResourceMode.EditorSimulate)
            {
                _mainLogicAssembly = GetMainLogicAssembly();
            }
            else
            {
                if (_setting.Enable)
                {
                    foreach (string hotUpdateDllName in _setting.HotUpdateAssemblies)
                    {
                        var assetLocation = hotUpdateDllName;
                        if (!_enableAddressable)
                        {
                            assetLocation = Utility.Path.GetRegularPath(
                                Path.Combine(
                                    "Assets",
                                    _setting.AssemblyTextAssetPath,
                                    $"{hotUpdateDllName}{_setting.AssemblyTextAssetExtension}"));
                        }

                        // 逐个加载热更程序集文本资源，再通过 Assembly.Load 注入运行时。
                        Log.Debug($"LoadAsset: [ {assetLocation} ]");
                        _loadAssetCount++;
                        var result = await _resComponent.LoadAsset<TextAsset>(assetLocation, 10);
                        LoadAssetSuccess(result);
                    }

                    _loadAssemblyWait = true;
                }
                else
                {
                    _mainLogicAssembly = GetMainLogicAssembly();
                }
            }

            if (_loadAssetCount == 0)
            {
                _loadAssemblyComplete = true;
            }
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!_loadAssemblyComplete)
            {
                return;
            }
            if (!_loadMetadataAssemblyComplete)
            {
                return;
            }
            AllAssemblyLoadComplete();
        }

        private void AllAssemblyLoadComplete()
        {
            // 当前项目通过实例化 GameEntry 预制体来接管后续游戏逻辑流程。
            ChangeState<ProcedureStartGame>(_procedureOwner);
// #if UNITY_EDITOR
//             _mainLogicAssembly = GetMainLogicAssembly();
// #endif
//             if (_mainLogicAssembly == null)
//             {
//                 Log.Fatal($"Main logic assembly missing. Please check \'ENABLE_HYBRIDCLR\' is defined in Player Settings And check the file of {_setting.LogicMainDllName}.bytes is exits.");
//                 return;
//             }
//             
//             var appType = _mainLogicAssembly.GetType("GameApp");
//             if (appType == null)
//             {
//                 Log.Fatal($"Main logic type 'GameMain' missing.");
//                 return;
//             }
//             var entryMethod = appType.GetMethod("Entrance");
//             if (entryMethod == null)
//             {
//                 Log.Fatal($"Main logic entry method 'Entrance' missing.");
//                 return;
//             }
//             object[] objects = new object[] { new object[] { _hotfixAssemblyList } };
//             entryMethod.Invoke(appType, objects);
        }

        private Assembly GetMainLogicAssembly()
        {
            // 编辑器模拟模式不走远端 DLL 加载，直接从当前 AppDomain 中查找程序集。
            _hotfixAssemblyList.Clear();
            Assembly mainLogicAssembly = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Compare(_setting.LogicMainDllName, $"{assembly.GetName().Name}.dll",
                        StringComparison.Ordinal) == 0)
                {
                    mainLogicAssembly = assembly;
                }

                foreach (var hotUpdateDllName in _setting.HotUpdateAssemblies)
                {
                    if (hotUpdateDllName == $"{assembly.GetName().Name}.dll")
                    {
                        _hotfixAssemblyList.Add(assembly);
                    }
                }

                if (mainLogicAssembly != null && _hotfixAssemblyList.Count == _setting.HotUpdateAssemblies.Count)
                {
                    break;
                }
            }

            return mainLogicAssembly;
        }

        /// <summary>
        /// 加载代码资源成功回调。
        /// </summary>
        /// <param name="textAsset">代码资产。</param>
        private void LoadAssetSuccess(TextAsset textAsset)
        {
            _loadAssetCount--;
            if (textAsset == null)
            {
                Log.Warning($"Load Assembly failed.");
                return;
            }

            var assetName = textAsset.name;
            Log.Debug($"LoadAssetSuccess, assetName: [ {assetName} ]");

            try
            {
                var assembly = Assembly.Load(textAsset.bytes);
                if (string.Compare(_setting.LogicMainDllName, assetName, StringComparison.Ordinal) == 0)
                {
                    _mainLogicAssembly = assembly;
                }
                _hotfixAssemblyList.Add(assembly);
                Log.Debug($"Assembly [ {assembly.GetName().Name} ] loaded");
            }
            catch (Exception e)
            {
                _failureAssetCount++;
                Log.Fatal(e);
                throw;
            }
            finally
            {
                _loadAssemblyComplete = _loadAssemblyWait && 0 == _loadAssetCount;
            }
            _resComponent.UnloadAsset(textAsset);
        }

        /// <summary>
        /// 为Aot Assembly加载原始metadata， 这个代码放Aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行。
        /// </summary>
        public void LoadMetadataForAOTAssembly()
        {
            // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
            // 我们在BuildProcessor_xxx里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。

            // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            if (_setting.AotMetaAssemblies.Count == 0)
            {
                _loadMetadataAssemblyComplete = true;
                return;
            }
            foreach (string aotDllName in _setting.AotMetaAssemblies)
            {
                var assetLocation = aotDllName;
                if (!_enableAddressable)
                {
                    assetLocation = Utility.Path.GetRegularPath(
                        Path.Combine(
                            "Assets",
                            _setting.AssemblyTextAssetPath,
                            $"{aotDllName}{_setting.AssemblyTextAssetExtension}"));
                }


                Log.Debug($"LoadMetadataAsset: [ {assetLocation} ]");
                _loadMetadataAssetCount++;
                _resComponent.LoadAsset<TextAsset>(assetLocation, LoadMetadataAssetSuccess);
            }
            _loadMetadataAssemblyWait = true;
        }

        /// <summary>
        /// 加载元数据资源成功回调。
        /// </summary>
        /// <param name="textAsset">代码资产。</param>
        private void LoadMetadataAssetSuccess(TextAsset textAsset)
        {
            _loadMetadataAssetCount--;
            if (null == textAsset)
            {
                Log.Debug($"LoadMetadataAssetSuccess:Load Metadata failed.");
                return;
            }

            string assetName = textAsset.name;
            Log.Debug($"LoadMetadataAssetSuccess, assetName: [ {assetName} ]");
            try
            {
                byte[] dllBytes = textAsset.bytes;
#if ENABLE_HYBRIDCLR
                    // 加载assembly对应的dll，会自动为它hook。一旦Aot泛型函数的native函数不存在，用解释器版本代码
                    HomologousImageMode mode = HomologousImageMode.SuperSet;
                    LoadImageErrorCode err = (LoadImageErrorCode)HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(dllBytes,mode); 
                    Log.Warning($"LoadMetadataForAOTAssembly:{assetName}. mode:{mode} ret:{err}");
#endif
            }
            catch (Exception e)
            {
                _failureMetadataAssetCount++;
                Log.Fatal(e.Message);
                throw;
            }
            finally
            {
                _loadMetadataAssemblyComplete = _loadMetadataAssemblyWait && 0 == _loadMetadataAssetCount;
            }
            _resComponent.UnloadAsset(textAsset);
        }
    }
}