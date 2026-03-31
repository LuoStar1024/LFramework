using System.Collections.Generic;
using UnityEditor;

namespace LFramework.Editor
{
    [CustomEditor(typeof(DataNodeComponent))]
    internal sealed class DataNodeComponentInspector : LFrameworkInspector
    {
        private const double RefreshInterval = 0.25d;

        private readonly Dictionary<string, bool> _dataNodeFoldoutStates = new Dictionary<string, bool>();
        private double _lastRefreshTime;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            DataNodeComponent t = (DataNodeComponent)target;

            if (IsPrefabInHierarchy(t.gameObject))
            {
                DrawDataNode(t.Root, 0);
            }

            if (EditorApplication.timeSinceStartup - _lastRefreshTime >= RefreshInterval)
            {
                _lastRefreshTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }

        private void OnEnable()
        {
            _lastRefreshTime = EditorApplication.timeSinceStartup;
        }

        private void DrawDataNode(IDataNode dataNode, int indentLevel)
        {
            int oldIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel;

            if (dataNode.ChildCount > 0)
            {
                bool isExpanded = GetDataNodeFoldoutState(dataNode.FullName, indentLevel == 0);
                isExpanded = EditorGUILayout.Foldout(isExpanded,
                    Utility.Text.Format("{0} {1}", dataNode.FullName, dataNode.ToDataString()), true);
                _dataNodeFoldoutStates[dataNode.FullName] = isExpanded;

                if (isExpanded)
                {
                    for (int i = 0; i < dataNode.ChildCount; i++)
                    {
                        IDataNode child = dataNode.GetChild(i);
                        if (child != null)
                        {
                            DrawDataNode(child, indentLevel + 1);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField(dataNode.FullName, dataNode.ToDataString());
            }

            EditorGUI.indentLevel = oldIndentLevel;
        }

        private bool GetDataNodeFoldoutState(string fullName, bool defaultValue)
        {
            if (_dataNodeFoldoutStates.TryGetValue(fullName, out bool value))
            {
                return value;
            }

            _dataNodeFoldoutStates.Add(fullName, defaultValue);
            return defaultValue;
        }
    }
}