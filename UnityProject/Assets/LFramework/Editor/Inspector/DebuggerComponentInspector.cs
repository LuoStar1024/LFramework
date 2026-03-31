using UnityEditor;
using UnityEngine;

namespace LFramework.Editor
{
    [CustomEditor(typeof(DebuggerComponent))]
    internal sealed class DebuggerComponentInspector : LFrameworkInspector
    {
        private SerializedProperty _skin = null;
        private SerializedProperty _activeWindow = null;
        private SerializedProperty _showFullWindow = null;
        private SerializedProperty _consoleWindow = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            DebuggerComponent t = (DebuggerComponent)target;

            EditorGUILayout.PropertyField(_skin);

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                bool activeWindow = EditorGUILayout.Toggle("Active Window", t.ActiveWindow);
                if (activeWindow != t.ActiveWindow)
                {
                    t.ActiveWindow = activeWindow;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(_activeWindow);
            }

            EditorGUILayout.PropertyField(_showFullWindow);

            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Reset Layout"))
                {
                    t.ResetLayout();
                }
            }

            EditorGUILayout.PropertyField(_consoleWindow, true);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _skin = serializedObject.FindProperty("skin");
            _activeWindow = serializedObject.FindProperty("activeWindow");
            _showFullWindow = serializedObject.FindProperty("showFullWindow");
            _consoleWindow = serializedObject.FindProperty("consoleWindow");
        }
    }
}