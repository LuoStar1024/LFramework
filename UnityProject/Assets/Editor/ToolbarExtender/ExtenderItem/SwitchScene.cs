using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameEditor
{
    public partial class ToolbarExtender
    {
        private static GUIContent _switchSceneBtContent;
        
        private static List<string> _switchSceneAssetList;
        
        private const string SwitchSceneLauncherPath = "Assets/Launcher/Scenes";
        private const string SwitchSceneOtherPath = "Assets/GameResRaw/Scenes";
        
        static void InitSwitchScene()
        {
            _switchSceneAssetList = new List<string>();
            var curOpenSceneName = SceneManager.GetActiveScene().name;
            _switchSceneBtContent = EditorGUIUtility.TrTextContentWithIcon(
                string.IsNullOrEmpty(curOpenSceneName) ? "Switch Scene" : "当前场景: " + curOpenSceneName, "切换场景",
                "UnityLogo");
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        static void ToolbarGUISwitchScene()
        {
            if (EditorGUILayout.DropdownButton(_switchSceneBtContent, FocusType.Passive, EditorStyles.toolbarPopup, GUILayout.MaxWidth(150)))
            {
                DrawSwitchSceneDropdownMenus();
            }
        }
        
        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            _switchSceneBtContent.text = "当前场景: " + scene.name;
        }
        
        static void DrawSwitchSceneDropdownMenus()
        {
            GenericMenu popMenu = new GenericMenu();
            popMenu.allowDuplicateNames = true;
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new string[] { SwitchSceneLauncherPath, SwitchSceneOtherPath });
            var sceneGuidList = sceneGuids.ToList();
            var allGuids = AssetDatabase.FindAssets("t:Scene", null);
            for (int i = 0; i < allGuids.Length; i++)
            {
                if (!sceneGuidList.Contains(allGuids[i]))
                {
                    sceneGuidList.Add(allGuids[i]);
                }
            }
            _switchSceneAssetList.Clear();
            for (int i = 0; i < sceneGuidList.Count; i++)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuidList[i]);
                _switchSceneAssetList.Add(scenePath);
                // string fileDir = System.IO.Path.GetDirectoryName(scenePath);
                // bool isInRootDir = Utility.Path.GetRegularPath(ConstEditor.ScenePath).TrimEnd('/') == Utility.Path.GetRegularPath(fileDir).TrimEnd('/');
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                string displayName = sceneName;
                // if (!isInRootDir)
                // {
                //     var sceneDir = System.IO.Path.GetRelativePath(ConstEditor.ScenePath, fileDir);
                //     displayName = $"{sceneDir}/{sceneName}";
                // }

                popMenu.AddItem(new GUIContent(displayName), false, menuIdx => { SwitchScene((int)menuIdx); }, i);
            }
            popMenu.ShowAsContext();
        }
        
        private static void SwitchScene(int menuIdx)
        {
            if (menuIdx >= 0 && menuIdx < _switchSceneAssetList.Count)
            {
                var scenePath = _switchSceneAssetList[menuIdx];
                var curScene = SceneManager.GetActiveScene();
                if (curScene.isDirty)
                {
                    int opIndex = EditorUtility.DisplayDialogComplex("警告", $"当前场景 {curScene.name} 未保存,是否保存?", "保存", "取消", "不保存");
                    switch (opIndex)
                    {
                        case 0:
                            if (!EditorSceneManager.SaveOpenScenes())
                            {
                                return;
                            }
                            break;
                        case 1:
                            return;
                    }
                }
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
        }
    }
}