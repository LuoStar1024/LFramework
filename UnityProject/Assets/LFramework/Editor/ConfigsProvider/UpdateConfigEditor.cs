using System.Collections.Generic;
using System.Linq;
using HybridCLR.Editor.Settings;
using LFramework;
using UnityEditor;
using UnityEngine;

namespace LFramework.Editor
{
    [CustomEditor(typeof(UpdateConfig), true)]
    public class UpdateConfigEditor : UnityEditor.Editor
    {
#if ENABLE_HYBRIDCLR
        public List<string> HotUpdateAssemblies = new() {};
        public List<string> AOTMetaAssemblies = new() {};
        
        private void OnEnable()
        {
            // 获取当前编辑的 ScriptableObject 实例
            UpdateConfig updateSetting = (UpdateConfig)target;
            if (updateSetting != null)
            {
                HotUpdateAssemblies.AddRange(updateSetting.HotUpdateAssemblies);
                AOTMetaAssemblies.AddRange(updateSetting.AotMetaAssemblies);
            }
        }

        public override void OnInspectorGUI()
        {
            // 记录对象修改前的状态
            EditorGUI.BeginChangeCheck();

            // 绘制默认的 Inspector 界面
            base.OnInspectorGUI();

            // 检测是否有字段被修改
            if (EditorGUI.EndChangeCheck())
            {
                // 获取当前编辑的 ScriptableObject 实例
                UpdateConfig updateSetting = (UpdateConfig)target;

                // 标记对象为“已修改”，确保修改能被保存
                EditorUtility.SetDirty(updateSetting);
                
                bool isHotChanged = !HotUpdateAssemblies.SequenceEqual(updateSetting.HotUpdateAssemblies);
                bool isAOTChanged = !AOTMetaAssemblies.SequenceEqual(updateSetting.AotMetaAssemblies);
                if (isHotChanged)
                {
                    HybridCLRSettings.Instance.hotUpdateAssemblies = updateSetting.HotUpdateAssemblies.ToArray();
                    for (int i = 0; i < updateSetting.HotUpdateAssemblies.Count; i++)
                    {
                        var assemblyName = updateSetting.HotUpdateAssemblies[i];
                        string assemblyNameWithoutExtension = assemblyName.Substring(0, assemblyName.LastIndexOf('.'));
                        HybridCLRSettings.Instance.hotUpdateAssemblies[i] = assemblyNameWithoutExtension;
                    }
                    Debug.Log("HotUpdateAssemblies changed");
                }
                if (isAOTChanged)
                {
                    HybridCLRSettings.Instance.patchAOTAssemblies = updateSetting.AotMetaAssemblies.ToArray();
                    Debug.Log("AOTMetaAssemblies changed");
                }

                if (isAOTChanged || isHotChanged)
                {
                    // 在修改HybridCLRSettings后添加
                    EditorUtility.SetDirty(HybridCLRSettings.Instance);
                    HybridCLRSettings.Save();
                    AssetDatabase.SaveAssets();
                }
            }
        }
#endif

        public static void ForceUpdateAssemblies()
        {
            UpdateConfig updateSetting = null;
            string[] guids = AssetDatabase.FindAssets("t:UpdateConfig");
            if (guids.Length >= 1)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                updateSetting = AssetDatabase.LoadAssetAtPath<UpdateConfig>(path);
            }

            if (updateSetting == null)
            {
                Log.Error("Can not find UpdateConfig");
                return;
            }
            
            HybridCLRSettings.Instance.hotUpdateAssemblies = updateSetting.HotUpdateAssemblies.ToArray();
            for (int i = 0; i < updateSetting.HotUpdateAssemblies.Count; i++)
            {
                var assemblyName = updateSetting.HotUpdateAssemblies[i];
                string assemblyNameWithoutExtension = assemblyName.Substring(0, assemblyName.LastIndexOf('.'));
                HybridCLRSettings.Instance.hotUpdateAssemblies[i] = assemblyNameWithoutExtension;
            }
            
            HybridCLRSettings.Instance.patchAOTAssemblies = updateSetting.AotMetaAssemblies.ToArray();
            HybridCLRSettings.Save();
            EditorUtility.SetDirty(HybridCLRSettings.Instance);
            AssetDatabase.SaveAssets();
            
            Debug.Log("HotUpdateAssemblies changed");
        }
    }
}