using GameLogic;
using LFramework.Editor;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    [CustomEditor(typeof(UIComponent))]
    internal sealed class UIComponentInspector : LFrameworkInspector
    {
        private SerializedProperty _instanceAutoReleaseInterval = null;
        private SerializedProperty _instanceCapacity = null;
        private SerializedProperty _instanceExpireTime = null;
        private SerializedProperty _instancePriority = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            UIComponent t = (UIComponent)target;

            float instanceAutoReleaseInterval = EditorGUILayout.DelayedFloatField("Instance Auto Release Interval",
                _instanceAutoReleaseInterval.floatValue);
            if (!Mathf.Approximately(instanceAutoReleaseInterval, _instanceAutoReleaseInterval.floatValue))
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceAutoReleaseInterval = instanceAutoReleaseInterval;
                }
                else
                {
                    _instanceAutoReleaseInterval.floatValue = instanceAutoReleaseInterval;
                }
            }

            int instanceCapacity = EditorGUILayout.DelayedIntField("Instance Capacity", _instanceCapacity.intValue);
            if (instanceCapacity != _instanceCapacity.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceCapacity = instanceCapacity;
                }
                else
                {
                    _instanceCapacity.intValue = instanceCapacity;
                }
            }

            float instanceExpireTime =
                EditorGUILayout.DelayedFloatField("Instance Expire Time", _instanceExpireTime.floatValue);
            if (!Mathf.Approximately(instanceExpireTime, _instanceExpireTime.floatValue))
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceExpireTime = instanceExpireTime;
                }
                else
                {
                    _instanceExpireTime.floatValue = instanceExpireTime;
                }
            }

            int instancePriority = EditorGUILayout.DelayedIntField("Instance Priority", _instancePriority.intValue);
            if (instancePriority != _instancePriority.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstancePriority = instancePriority;
                }
                else
                {
                    _instancePriority.intValue = instancePriority;
                }
            }

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("UI Group Count", t.UIGroupCount.ToString());
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
            _instanceAutoReleaseInterval = serializedObject.FindProperty("instanceAutoReleaseInterval");
            _instanceCapacity = serializedObject.FindProperty("instanceCapacity");
            _instanceExpireTime = serializedObject.FindProperty("instanceExpireTime");
            _instancePriority = serializedObject.FindProperty("instancePriority");

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}