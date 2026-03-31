using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameEditor
{
    public partial class ToolbarExtender
    {
        private static GUIContent _launcherSceneBtContent;
        
        private const string PreviousSceneKey = "LFPreviousScenePath"; // 用于存储之前场景路径的键
        private const string IsLauncherBtn = "LFIsLauncher"; // 用于存储之前是否按下launcher
        private const string LauncherSceneName = "Launcher";
        // private const string LauncherScenePath = "Assets/Launcher/Scenes/Launcher.unity";
        
        static void InitLauncherScene()
        {
            _launcherSceneBtContent = EditorGUIUtility.TrTextContentWithIcon($"启动: {LauncherSceneName}", "启动默认初始场景", "PlayButton");
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnEditorQuit;
        }

        static void ToolbarGUILauncherScene()
        {
            if (GUILayout.Button(_launcherSceneBtContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(120)))
            {
                StartLauncherScene();
            }
        }

        static void StartLauncherScene()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }
            
            // 记录当前场景路径到 EditorPrefs
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.isLoaded && activeScene.name != LauncherSceneName)
            {
                EditorPrefs.SetString(PreviousSceneKey, activeScene.path);
                EditorPrefs.SetBool(IsLauncherBtn, true);
            }
            
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                var sceneToOpen = LauncherSceneName;
                string[] guids = AssetDatabase.FindAssets("t:scene " + sceneToOpen, null);
                if (guids.Length == 0)
                {
                    Debug.LogWarning("Couldn't find scene file");
                }
                else
                {
                    string scenePath = null;
                    // 优先打开完全匹配_sceneToOpen的场景
                    for (var i = 0; i < guids.Length; i++)
                    {
                        scenePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                        if (scenePath.EndsWith("/" + sceneToOpen + ".unity"))
                        {
                            break;
                        }
                    }

                    // 如果没有完全匹配的场景，默认显示找到的第一个场景
                    if (string.IsNullOrEmpty(scenePath))
                    {
                        scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    }

                    EditorSceneManager.OpenScene(scenePath);
                    EditorApplication.isPlaying = true;
                }
            }
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // 从 EditorPrefs 读取之前的场景路径
                var previousScenePath = EditorPrefs.GetString(PreviousSceneKey, string.Empty);
                if (!string.IsNullOrEmpty(previousScenePath) && EditorPrefs.GetBool(IsLauncherBtn))
                {
                    EditorApplication.delayCall += () =>
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(previousScenePath);
                        }
                    };
                }

                EditorPrefs.SetBool(IsLauncherBtn, false);
            }
        }

        private static void OnEditorQuit()
        {
            EditorPrefs.SetString(PreviousSceneKey, "");
            EditorPrefs.SetBool(IsLauncherBtn, false);
        }
    }
}