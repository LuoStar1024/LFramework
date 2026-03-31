using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace LFramework.Editor
{
    [CustomEditor(typeof(ResourceComponent))]
    internal sealed class ResourceComponentInspector : LFrameworkInspector
    {
        private static readonly string[] ResourceModeNames = new string[]
            { "Package", "Updatable", "WebPlayMode" };

        private SerializedProperty _isEditorSimulate = null;
        private SerializedProperty _resourceMode = null;
        private SerializedProperty _updatableWhilePlaying = null;
        private SerializedProperty _loadResourceWayWebGL = null;
        private SerializedProperty _encryptionType = null;
        private SerializedProperty _defaultPackageName = null;
        private SerializedProperty _downloadingMaxNum = null;
        private SerializedProperty _failedTryAgain = null;
        private SerializedProperty _milliseconds = null;
        private SerializedProperty _assetAutoReleaseInterval = null;
        private SerializedProperty _assetCapacity = null;
        private SerializedProperty _assetExpireTime = null;
        private SerializedProperty _assetPriority = null;
        private SerializedProperty _minUnloadUnusedAssetsInterval = null;
        private SerializedProperty _maxUnloadUnusedAssetsInterval = null;
        private SerializedProperty _useSystemUnloadUnusedAssets;

        private int _resourceModeIndex = 0;
        
        private int _packageNameIndex = 0;
        private string[] _packageNames;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ResourceComponent t = (ResourceComponent)target;
            
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(_isEditorSimulate);
                bool isEditorResourceMode = _isEditorSimulate.boolValue;
                if (isEditorResourceMode)
                {
                    EditorGUILayout.HelpBox("Editor resource mode is enabled. Some options are disabled.",
                        MessageType.Warning);
                }
                
                if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
                {
                    EditorGUILayout.EnumPopup("Resource Mode", t.ResourceMode);
                }
                else
                {
                    int selectedIndex = EditorGUILayout.Popup("Resource Mode", _resourceModeIndex, ResourceModeNames);
                    if (selectedIndex != _resourceModeIndex)
                    {
                        _resourceModeIndex = selectedIndex;
                        _resourceMode.enumValueIndex = selectedIndex + 1;
                    }
                }

                if (_resourceMode.enumValueIndex == 2)
                {
                    EditorGUILayout.PropertyField(_updatableWhilePlaying);
                }
                
                if (_resourceMode.enumValueIndex == 3)
                {
                    EditorGUILayout.PropertyField(_loadResourceWayWebGL);
                }
                
                EditorGUILayout.PropertyField(_encryptionType);
            }
            EditorGUI.EndDisabledGroup();
            
            _packageNames = GetBuildPackageNames().ToArray();
            _packageNameIndex = Array.IndexOf(_packageNames, _defaultPackageName.stringValue);
            if (_packageNameIndex < 0)
            {
                _packageNameIndex = 0;
            }

            if (_packageNames.Length > _packageNameIndex)
            {
                _packageNameIndex = EditorGUILayout.Popup("Package Name", _packageNameIndex, _packageNames);
                if (_defaultPackageName.stringValue != _packageNames[_packageNameIndex])
                {
                    _defaultPackageName.stringValue = _packageNames[_packageNameIndex];
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Package Name is null.", MessageType.Error);
            }
            
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(_downloadingMaxNum);
                EditorGUILayout.PropertyField(_failedTryAgain);
                EditorGUILayout.IntSlider("Milliseconds", _milliseconds.intValue, 1, 60);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            {
                float assetAutoReleaseInterval = EditorGUILayout.DelayedFloatField("Asset Auto Release Interval",
                    _assetAutoReleaseInterval.floatValue);
                if (!Mathf.Approximately(assetAutoReleaseInterval, _assetAutoReleaseInterval.floatValue))
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetAutoReleaseInterval = assetAutoReleaseInterval;
                    }
                    else
                    {
                        _assetAutoReleaseInterval.floatValue = assetAutoReleaseInterval;
                    }
                }

                int assetCapacity = EditorGUILayout.DelayedIntField("Asset Capacity", _assetCapacity.intValue);
                if (assetCapacity != _assetCapacity.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetCapacity = assetCapacity;
                    }
                    else
                    {
                        _assetCapacity.intValue = assetCapacity;
                    }
                }

                float assetExpireTime =
                    EditorGUILayout.DelayedFloatField("Asset Expire Time", _assetExpireTime.floatValue);
                if (!Mathf.Approximately(assetExpireTime, _assetExpireTime.floatValue))
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetExpireTime = assetExpireTime;
                    }
                    else
                    {
                        _assetExpireTime.floatValue = assetExpireTime;
                    }
                }

                int assetPriority = EditorGUILayout.DelayedIntField("Asset Priority", _assetPriority.intValue);
                if (assetPriority != _assetPriority.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetPriority = assetPriority;
                    }
                    else
                    {
                        _assetPriority.intValue = assetPriority;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
            
            float minUnloadUnusedAssetsInterval = EditorGUILayout.Slider("Min Unload Unused Assets Interval",
                _minUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (!Mathf.Approximately(minUnloadUnusedAssetsInterval, _minUnloadUnusedAssetsInterval.floatValue))
            {
                if (EditorApplication.isPlaying)
                {
                    t.MinUnloadUnusedAssetsInterval = minUnloadUnusedAssetsInterval;
                }
                else
                {
                    _minUnloadUnusedAssetsInterval.floatValue = minUnloadUnusedAssetsInterval;
                }
            }

            float maxUnloadUnusedAssetsInterval = EditorGUILayout.Slider("Max Unload Unused Assets Interval",
                _maxUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (!Mathf.Approximately(maxUnloadUnusedAssetsInterval, _maxUnloadUnusedAssetsInterval.floatValue))
            {
                if (EditorApplication.isPlaying)
                {
                    t.MaxUnloadUnusedAssetsInterval = maxUnloadUnusedAssetsInterval;
                }
                else
                {
                    _maxUnloadUnusedAssetsInterval.floatValue = maxUnloadUnusedAssetsInterval;
                }
            }
            
            EditorGUILayout.PropertyField(_useSystemUnloadUnusedAssets);

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.BeginVertical("box");
                {
                    List<string> loadAssetInfos = t.GetLoadingAssetInfo();
                    if (loadAssetInfos.Count > 0)
                    {
                        foreach (string loadAssetInfo in loadAssetInfos)
                        {
                            EditorGUILayout.LabelField(loadAssetInfo);
                        }
                    }
                    else
                    {
                        GUILayout.Label("Loading Asset is Empty ...");
                    }
                }
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            _isEditorSimulate = serializedObject.FindProperty("isEditorSimulate");
            _resourceMode = serializedObject.FindProperty("resourceMode");
            _updatableWhilePlaying = serializedObject.FindProperty("updatableWhilePlaying");
            _loadResourceWayWebGL = serializedObject.FindProperty("loadResourceWayWebGL");
            _encryptionType = serializedObject.FindProperty("encryptionType");
            _defaultPackageName = serializedObject.FindProperty("defaultPackageName");
            _downloadingMaxNum = serializedObject.FindProperty("downloadingMaxNum");
            _failedTryAgain = serializedObject.FindProperty("failedTryAgain");
            _milliseconds = serializedObject.FindProperty("milliseconds");
            _assetAutoReleaseInterval = serializedObject.FindProperty("assetAutoReleaseInterval");
            _assetCapacity = serializedObject.FindProperty("assetCapacity");
            _assetExpireTime = serializedObject.FindProperty("assetExpireTime");
            _assetPriority = serializedObject.FindProperty("assetPriority");
            _minUnloadUnusedAssetsInterval = serializedObject.FindProperty("minUnloadUnusedAssetsInterval");
            _maxUnloadUnusedAssetsInterval = serializedObject.FindProperty("maxUnloadUnusedAssetsInterval");
            _useSystemUnloadUnusedAssets = serializedObject.FindProperty("useSystemUnloadUnusedAssets");

            RefreshModes();
            RefreshTypeNames();
        }

        private void RefreshModes()
        {
            _resourceModeIndex = _resourceMode.enumValueIndex > 0 ? _resourceMode.enumValueIndex - 1 : 0;
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// 获取构建包名称列表，用于下拉可选择
        /// </summary>
        /// <returns></returns>
        private List<string> GetBuildPackageNames()
        {
            List<string> result = new List<string>();
            foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
            {
                result.Add(package.PackageName);
            }
            return result;
        }
    }
}