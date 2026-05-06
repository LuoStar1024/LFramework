using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public partial class ToolbarExtender
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
            // Left
            InitSwitchScene();
            InitLauncherScene();

            // Right
            InitOpenCsProject();

            UnityEditorToolbar.LeftToolbarGUI.Add(OnLeftToolbarGUI);
            UnityEditorToolbar.RightToolbarGUI.Add(OnRightToolbarGUI);
        }

        private static void OnLeftToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            // Function

            // EditorGUILayout.Space(10);
            // Function

            ToolbarGUISwitchScene();

            EditorGUILayout.Space(10);
            ToolbarGUILauncherScene();
        }

        private static void OnRightToolbarGUI()
        {
            ToolbarGUIOpenCsProject();

            // Function
            // EditorGUILayout.Space(10);

            // Function
            GUILayout.FlexibleSpace();
        }
    }
}