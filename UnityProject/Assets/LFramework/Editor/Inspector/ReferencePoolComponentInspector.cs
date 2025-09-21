using LFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LFramework.Editor
{
    [CustomEditor(typeof(ReferencePoolComponent))]
    internal sealed class ReferencePoolComponentInspector : LFrameworkInspector
    {
        private readonly Dictionary<string, List<ReferencePoolInfo>> _referencePoolInfos =
            new Dictionary<string, List<ReferencePoolInfo>>(StringComparer.Ordinal);

        private readonly HashSet<string> _openedItems = new HashSet<string>();

        private SerializedProperty _enableStrictCheck = null;

        private bool _showFullClassName = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ReferencePoolComponent t = (ReferencePoolComponent)target;

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                bool enableStrictCheck = EditorGUILayout.Toggle("Enable Strict Check", t.EnableStrictCheck);
                if (enableStrictCheck != t.EnableStrictCheck)
                {
                    t.EnableStrictCheck = enableStrictCheck;
                }

                EditorGUILayout.LabelField("Reference Pool Count", ReferencePool.Count.ToString());
                _showFullClassName = EditorGUILayout.Toggle("Show Full Class Name", _showFullClassName);
                _referencePoolInfos.Clear();
                ReferencePoolInfo[] referencePoolInfos = ReferencePool.GetAllReferencePoolInfos();
                foreach (ReferencePoolInfo referencePoolInfo in referencePoolInfos)
                {
                    string assemblyName = referencePoolInfo.Type.Assembly.GetName().Name;
                    List<ReferencePoolInfo> results = null;
                    if (!_referencePoolInfos.TryGetValue(assemblyName, out results))
                    {
                        results = new List<ReferencePoolInfo>();
                        _referencePoolInfos.Add(assemblyName, results);
                    }

                    results.Add(referencePoolInfo);
                }

                foreach (KeyValuePair<string, List<ReferencePoolInfo>> assemblyReferencePoolInfo in _referencePoolInfos)
                {
                    bool lastState = _openedItems.Contains(assemblyReferencePoolInfo.Key);
                    bool currentState = EditorGUILayout.Foldout(lastState, assemblyReferencePoolInfo.Key);
                    if (currentState != lastState)
                    {
                        if (currentState)
                        {
                            _openedItems.Add(assemblyReferencePoolInfo.Key);
                        }
                        else
                        {
                            _openedItems.Remove(assemblyReferencePoolInfo.Key);
                        }
                    }

                    if (currentState)
                    {
                        EditorGUILayout.BeginVertical("box");
                        {
                            EditorGUILayout.LabelField(_showFullClassName ? "Full Class Name" : "Class Name",
                                "Unused\tUsing\tAcquire\tRelease\tAdd\tRemove");
                            assemblyReferencePoolInfo.Value.Sort(Comparison);
                            foreach (ReferencePoolInfo referencePoolInfo in assemblyReferencePoolInfo.Value)
                            {
                                DrawReferencePoolInfo(referencePoolInfo);
                            }

                            if (GUILayout.Button("Export CSV Data"))
                            {
                                string exportFileName = EditorUtility.SaveFilePanel("Export CSV Data", string.Empty,
                                    Utility.Text.Format("Reference Pool Data - {0}.csv", assemblyReferencePoolInfo.Key),
                                    string.Empty);
                                if (!string.IsNullOrEmpty(exportFileName))
                                {
                                    try
                                    {
                                        int index = 0;
                                        string[] data = new string[assemblyReferencePoolInfo.Value.Count + 1];
                                        data[index++] =
                                            "Class Name,Full Class Name,Unused,Using,Acquire,Release,Add,Remove";
                                        foreach (ReferencePoolInfo referencePoolInfo in assemblyReferencePoolInfo.Value)
                                        {
                                            data[index++] = Utility.Text.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                                                referencePoolInfo.Type.Name, referencePoolInfo.Type.FullName,
                                                referencePoolInfo.UnusedReferenceCount,
                                                referencePoolInfo.UsingReferenceCount,
                                                referencePoolInfo.AcquireReferenceCount,
                                                referencePoolInfo.ReleaseReferenceCount,
                                                referencePoolInfo.AddReferenceCount,
                                                referencePoolInfo.RemoveReferenceCount);
                                        }

                                        File.WriteAllLines(exportFileName, data, Encoding.UTF8);
                                        Debug.Log(Utility.Text.Format(
                                            "Export reference pool CSV data to '{0}' success.", exportFileName));
                                    }
                                    catch (Exception exception)
                                    {
                                        Debug.LogError(Utility.Text.Format(
                                            "Export reference pool CSV data to '{0}' failure, exception is '{1}'.",
                                            exportFileName, exception));
                                    }
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Separator();
                    }
                }
            }
            else
            {
                EditorGUILayout.PropertyField(_enableStrictCheck);
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        private void OnEnable()
        {
            _enableStrictCheck = serializedObject.FindProperty("enableStrictCheck");
        }

        private void DrawReferencePoolInfo(ReferencePoolInfo referencePoolInfo)
        {
            EditorGUILayout.LabelField(
                _showFullClassName ? referencePoolInfo.Type.FullName : referencePoolInfo.Type.Name,
                Utility.Text.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", referencePoolInfo.UnusedReferenceCount,
                    referencePoolInfo.UsingReferenceCount, referencePoolInfo.AcquireReferenceCount,
                    referencePoolInfo.ReleaseReferenceCount, referencePoolInfo.AddReferenceCount,
                    referencePoolInfo.RemoveReferenceCount));
        }

        private int Comparison(ReferencePoolInfo a, ReferencePoolInfo b)
        {
            if (_showFullClassName)
            {
                return a.Type.FullName.CompareTo(b.Type.FullName);
            }
            else
            {
                return a.Type.Name.CompareTo(b.Type.Name);
            }
        }
    }
}