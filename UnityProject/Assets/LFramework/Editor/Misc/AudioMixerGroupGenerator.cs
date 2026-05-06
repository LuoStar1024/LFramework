using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace LFramework.Editor
{
    internal static class AudioMixerGroupGenerator
    {
        private const string ConstantSettingScriptPath =
            "Assets/GameScripts/GameLogic/Definition/Constant/Constant.Setting.cs";

        private const string LauncherProcedureScriptPath =
            "Assets/Launcher/Scripts/Procedure/ProcedureLaunch.cs";

        private static readonly Regex ConstStringRegex =
            new Regex(@"public\s+const\s+string\s+(?<name>\w+)\s*=\s*""(?<value>[^""]+)""\s*;",
                RegexOptions.Compiled);

        private static readonly Regex DictionaryEntryRegex =
            new Regex(@"\{\s*(?<name>[^,\r\n\{]+?)\s*,\s*(?<count>\d+)\s*\}", RegexOptions.Compiled);

        private static readonly Regex AddAudioGroupRegex =
            new Regex(@"AddAudioGroup\s*\(\s*(?<name>[^,\r\n]+?)\s*,\s*(?<count>\d+)",
                RegexOptions.Compiled);

        private static readonly Regex AudioMixerGroupBlockRegex =
            new Regex(@"^--- !u!243 &(?<id>-?\d+)$", RegexOptions.Compiled);

        private static readonly Regex AudioMixerControllerBlockRegex =
            new Regex(@"^--- !u!241 &(?<id>-?\d+)$", RegexOptions.Compiled);

        private static readonly Regex FileIdRegex =
            new Regex(@"\{fileID:\s*(?<id>-?\d+)\}", RegexOptions.Compiled);

        // 从 .mixer 资源文本中提取出来的最小分组树信息，用于和运行时对象建立路径映射。
        private sealed class AudioMixerGroupAssetInfo
        {
            public string Name;
            public readonly List<long> Children = new List<long>();
        }

        [MenuItem("LFramework/Audio/Generate Audio Mixer Groups", false, 80)]
        private static void GenerateSelectedAudioMixerGroups()
        {
            AudioMixer audioMixer = GetSelectedAudioMixer();
            if (audioMixer == null)
            {
                Debug.LogWarning("Please select an AudioMixer asset or a GameObject with AudioComponent.");
                return;
            }

            Generate(audioMixer);
        }

        [MenuItem("LFramework/Audio/Generate Audio Mixer Groups", true)]
        private static bool ValidateGenerateSelectedAudioMixerGroups()
        {
            return GetSelectedAudioMixer() != null;
        }

        public static bool Generate(AudioMixer audioMixer)
        {
            if (audioMixer == null)
            {
                throw new LFrameworkException("Audio mixer is invalid.");
            }

            Dictionary<string, int> audioGroupInfos = CollectAudioGroupInfos();
            if (audioGroupInfos.Count <= 0)
            {
                Debug.LogWarning("No audio group definitions found from configured scripts.");
                return false;
            }

            bool modified = false;
            Dictionary<string, object> groupControllerMap = BuildGroupControllerPathMap(audioMixer);
            HashSet<string> expectedGroupPaths = BuildExpectedGroupPaths(audioGroupInfos);
            modified |= RemoveRedundantAudioMixerGroups(audioMixer, groupControllerMap, expectedGroupPaths);
            if (modified)
            {
                groupControllerMap = BuildGroupControllerPathMap(audioMixer);
            }

            foreach (KeyValuePair<string, int> audioGroupInfo in audioGroupInfos)
            {
                object audioGroup = EnsureAudioMixerGroup(audioMixer, groupControllerMap, "Master", audioGroupInfo.Key,
                    ref modified);
                if (audioGroup == null)
                {
                    continue;
                }

                string parentPath = Utility.Text.Format("Master/{0}", audioGroupInfo.Key);
                for (int i = 0; i < audioGroupInfo.Value; i++)
                {
                    EnsureAudioMixerGroup(audioMixer, groupControllerMap, parentPath, i.ToString(), ref modified);
                }
            }

            if (modified)
            {
                EditorUtility.SetDirty(audioMixer);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(audioMixer));
                Debug.Log($"Generate audio mixer groups success : {audioMixer.name}");
            }
            else
            {
                Debug.LogWarning($"Audio mixer groups already up to date : {audioMixer.name}");
            }

            return modified;
        }

        // 生成器只维护 Master 下的一级业务分组和其数字子分组，用路径集合作为同步基准。
        private static HashSet<string> BuildExpectedGroupPaths(Dictionary<string, int> audioGroupInfos)
        {
            HashSet<string> results = new HashSet<string>(StringComparer.Ordinal)
            {
                "Master"
            };

            foreach (KeyValuePair<string, int> audioGroupInfo in audioGroupInfos)
            {
                string topLevelGroupPath = Utility.Text.Format("Master/{0}", audioGroupInfo.Key);
                results.Add(topLevelGroupPath);
                for (int i = 0; i < audioGroupInfo.Value; i++)
                {
                    results.Add(Utility.Text.Format("{0}/{1}", topLevelGroupPath, i));
                }
            }

            return results;
        }

        private static Dictionary<string, int> CollectAudioGroupInfos()
        {
            Dictionary<string, string> constStringMap = ParseConstStringMap(ConstantSettingScriptPath);
            Dictionary<string, int> results = new Dictionary<string, int>(StringComparer.Ordinal);

            MergeAudioGroupInfos(results, ParseAudioGroupDict(ConstantSettingScriptPath, constStringMap));
            MergeAudioGroupInfos(results, ParseLaunchDefaultGroups(LauncherProcedureScriptPath, constStringMap));

            return results;
        }

        private static Dictionary<string, string> ParseConstStringMap(string assetPath)
        {
            string scriptContent = ReadScriptContent(assetPath);
            Dictionary<string, string> results = new Dictionary<string, string>(StringComparer.Ordinal);
            MatchCollection matches = ConstStringRegex.Matches(scriptContent);
            foreach (Match match in matches)
            {
                results[match.Groups["name"].Value] = match.Groups["value"].Value;
            }

            return results;
        }

        private static Dictionary<string, int> ParseAudioGroupDict(string assetPath,
            Dictionary<string, string> constStringMap)
        {
            return ParseAudioGroupInfos(assetPath, DictionaryEntryRegex, constStringMap);
        }

        private static Dictionary<string, int> ParseLaunchDefaultGroups(string assetPath,
            Dictionary<string, string> constStringMap)
        {
            return ParseAudioGroupInfos(assetPath, AddAudioGroupRegex, constStringMap);
        }

        private static Dictionary<string, int> ParseAudioGroupInfos(string assetPath, Regex regex,
            Dictionary<string, string> constStringMap)
        {
            string scriptContent = ReadScriptContent(assetPath);
            Dictionary<string, int> results = new Dictionary<string, int>(StringComparer.Ordinal);
            MatchCollection matches = regex.Matches(scriptContent);
            foreach (Match match in matches)
            {
                string audioGroupName = ResolveGroupName(match.Groups["name"].Value, constStringMap);
                if (string.IsNullOrEmpty(audioGroupName))
                {
                    continue;
                }

                int audioAgentCount = int.Parse(match.Groups["count"].Value);
                if (results.TryGetValue(audioGroupName, out int currentCount))
                {
                    results[audioGroupName] = Math.Max(currentCount, audioAgentCount);
                }
                else
                {
                    results.Add(audioGroupName, audioAgentCount);
                }
            }

            return results;
        }

        private static string ResolveGroupName(string expression, Dictionary<string, string> constStringMap)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return null;
            }

            string trimmedExpression = expression.Trim();
            if (trimmedExpression.StartsWith("\"") && trimmedExpression.EndsWith("\""))
            {
                return trimmedExpression.Substring(1, trimmedExpression.Length - 2);
            }

            string[] tokens = trimmedExpression.Split('.');
            string token = tokens[tokens.Length - 1].Trim();
            return constStringMap.TryGetValue(token, out string value) ? value : null;
        }

        private static void MergeAudioGroupInfos(Dictionary<string, int> target, Dictionary<string, int> source)
        {
            foreach (KeyValuePair<string, int> audioGroupInfo in source)
            {
                if (target.TryGetValue(audioGroupInfo.Key, out int currentCount))
                {
                    target[audioGroupInfo.Key] = Math.Max(currentCount, audioGroupInfo.Value);
                }
                else
                {
                    target.Add(audioGroupInfo.Key, audioGroupInfo.Value);
                }
            }
        }

        private static string ReadScriptContent(string assetPath)
        {
            string fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath));
            if (!File.Exists(fullPath))
            {
                throw new LFrameworkException($"Script file does not exist : {assetPath}");
            }

            return File.ReadAllText(fullPath);
        }

        private static object EnsureAudioMixerGroup(AudioMixer audioMixer,
            Dictionary<string, object> groupControllerMap,
            string parentPath, string groupName, ref bool modified)
        {
            string groupPath = Utility.Text.Format("{0}/{1}", parentPath, groupName);
            if (groupControllerMap.TryGetValue(groupPath, out object existingGroup))
            {
                return existingGroup;
            }

            if (!groupControllerMap.TryGetValue(parentPath, out object parentGroup))
            {
                Debug.LogWarning($"Can not find audio mixer parent group '{parentPath}'.");
                return null;
            }

            MethodInfo createNewGroupMethod = GetAudioMixerMethod(audioMixer, "CreateNewGroup");
            MethodInfo addChildToParentMethod = GetAudioMixerMethod(audioMixer, "AddChildToParent");
            if (createNewGroupMethod == null || addChildToParentMethod == null)
            {
                Debug.LogWarning($"Can not create audio mixer group '{groupPath}', required editor api not found.");
                return null;
            }

            object createdGroup = createNewGroupMethod.Invoke(audioMixer, new object[] { groupName, true });
            addChildToParentMethod.Invoke(audioMixer, new[] { createdGroup, parentGroup });
            modified = true;
            groupControllerMap[groupPath] = createdGroup;

            return createdGroup;
        }

        // 先删掉多余分组，再补缺失分组，保证脚本配置减少数量时也能同步到 AudioMixer。
        private static bool RemoveRedundantAudioMixerGroups(AudioMixer audioMixer,
            Dictionary<string, object> groupControllerMap,
            HashSet<string> expectedGroupPaths)
        {
            List<string> redundantPaths = CollectRedundantGroupPaths(groupControllerMap, expectedGroupPaths);
            if (redundantPaths.Count <= 0)
            {
                return false;
            }

            MethodInfo deleteGroupsMethod = GetAudioMixerMethod(audioMixer, "DeleteGroups");
            if (deleteGroupsMethod == null)
            {
                Debug.LogWarning("Can not remove redundant audio mixer groups, required editor api not found.");
                return false;
            }

            ParameterInfo[] deleteGroupParameters = deleteGroupsMethod.GetParameters();
            if (deleteGroupParameters.Length != 1 || !deleteGroupParameters[0].ParameterType.IsArray)
            {
                Debug.LogWarning("Can not remove redundant audio mixer groups, delete api signature is invalid.");
                return false;
            }

            System.Type groupControllerType = deleteGroupParameters[0].ParameterType.GetElementType();
            Array groupsToDelete = Array.CreateInstance(groupControllerType, redundantPaths.Count);
            for (int i = 0; i < redundantPaths.Count; i++)
            {
                groupsToDelete.SetValue(groupControllerMap[redundantPaths[i]], i);
            }

            deleteGroupsMethod.Invoke(audioMixer, new object[] { groupsToDelete });
            foreach (string redundantPath in redundantPaths)
            {
                groupControllerMap.Remove(redundantPath);
            }

            return true;
        }

        private static List<string> CollectRedundantGroupPaths(Dictionary<string, object> groupControllerMap,
            HashSet<string> expectedGroupPaths)
        {
            List<string> managedTopLevelGroupPaths = new List<string>();
            foreach (string groupPath in groupControllerMap.Keys)
            {
                if (GetGroupPathDepth(groupPath) != 2)
                {
                    continue;
                }

                if (expectedGroupPaths.Contains(groupPath) ||
                    IsGeneratedTopLevelGroup(groupPath, groupControllerMap.Keys))
                {
                    managedTopLevelGroupPaths.Add(groupPath);
                }
            }

            HashSet<string> managedTopLevelGroupPathSet =
                new HashSet<string>(managedTopLevelGroupPaths, StringComparer.Ordinal);
            List<string> redundantPaths = new List<string>();
            foreach (string groupPath in groupControllerMap.Keys)
            {
                if (string.Equals(groupPath, "Master", StringComparison.Ordinal) ||
                    expectedGroupPaths.Contains(groupPath))
                {
                    continue;
                }

                int depth = GetGroupPathDepth(groupPath);
                if (depth == 2)
                {
                    if (managedTopLevelGroupPathSet.Contains(groupPath))
                    {
                        redundantPaths.Add(groupPath);
                    }

                    continue;
                }

                if (depth != 3)
                {
                    continue;
                }

                string topLevelGroupPath = GetParentGroupPath(groupPath);
                if (managedTopLevelGroupPathSet.Contains(topLevelGroupPath))
                {
                    redundantPaths.Add(groupPath);
                }
            }

            redundantPaths.Sort((left, right) => GetGroupPathDepth(right).CompareTo(GetGroupPathDepth(left)));
            List<string> results = new List<string>();
            foreach (string redundantPath in redundantPaths)
            {
                bool hasRedundantAncestor = false;
                foreach (string existingPath in results)
                {
                    if (IsChildPath(existingPath, redundantPath))
                    {
                        hasRedundantAncestor = true;
                        break;
                    }
                }

                if (!hasRedundantAncestor)
                {
                    results.Add(redundantPath);
                }
            }

            return results;
        }

        // 直接解析 .mixer 文本里的父子关系，避免单纯依赖反射时路径识别不稳定导致重复生成。
        private static Dictionary<string, object> BuildGroupControllerPathMap(AudioMixer audioMixer)
        {
            Dictionary<string, object> results = new Dictionary<string, object>(StringComparer.Ordinal);
            MethodInfo getAllGroupsMethod = GetAudioMixerMethod(audioMixer, "GetAllAudioGroupsSlow");
            if (getAllGroupsMethod == null)
            {
                return results;
            }

            string mixerAssetPath = AssetDatabase.GetAssetPath(audioMixer);
            if (string.IsNullOrEmpty(mixerAssetPath))
            {
                return results;
            }

            Dictionary<long, string> groupPathMap = ParseAudioMixerGroupPaths(mixerAssetPath);
            if (groupPathMap.Count <= 0)
            {
                return results;
            }

            object groupCollection = getAllGroupsMethod.Invoke(audioMixer, null);
            if (!(groupCollection is System.Collections.IEnumerable groups))
            {
                return results;
            }

            foreach (object group in groups)
            {
                if (!(group is UnityEngine.Object unityObject))
                {
                    continue;
                }

                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(unityObject, out string _, out long localId))
                {
                    continue;
                }

                if (!groupPathMap.TryGetValue(localId, out string groupPath) || string.IsNullOrEmpty(groupPath))
                {
                    continue;
                }

                if (!results.ContainsKey(groupPath))
                {
                    results.Add(groupPath, group);
                }
            }

            return results;
        }

        private static Dictionary<long, string> ParseAudioMixerGroupPaths(string mixerAssetPath)
        {
            Dictionary<long, AudioMixerGroupAssetInfo> groupInfos = new Dictionary<long, AudioMixerGroupAssetInfo>();
            long masterGroupId = 0L;
            string mixerFullPath =
                Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.dataPath), mixerAssetPath));
            if (!File.Exists(mixerFullPath))
            {
                return new Dictionary<long, string>();
            }

            string[] lines = File.ReadAllLines(mixerFullPath);
            for (int i = 0; i < lines.Length; i++)
            {
                Match controllerMatch = AudioMixerControllerBlockRegex.Match(lines[i]);
                if (controllerMatch.Success)
                {
                    for (int j = i + 1; j < lines.Length && !lines[j].StartsWith("--- !u!"); j++)
                    {
                        if (TryParseFileIdFromLine(lines[j].Trim(), "m_MasterGroup:", out masterGroupId))
                        {
                            break;
                        }
                    }

                    continue;
                }

                Match groupMatch = AudioMixerGroupBlockRegex.Match(lines[i]);
                if (!groupMatch.Success)
                {
                    continue;
                }

                long groupId = long.Parse(groupMatch.Groups["id"].Value);
                AudioMixerGroupAssetInfo groupInfo = new AudioMixerGroupAssetInfo();
                for (int j = i + 1; j < lines.Length && !lines[j].StartsWith("--- !u!"); j++)
                {
                    string trimmedLine = lines[j].Trim();
                    if (trimmedLine.StartsWith("m_Name: "))
                    {
                        groupInfo.Name = trimmedLine.Substring("m_Name: ".Length);
                        continue;
                    }

                    if (!trimmedLine.StartsWith("m_Children:") || trimmedLine.EndsWith("[]"))
                    {
                        continue;
                    }

                    while (j + 1 < lines.Length)
                    {
                        string childLine = lines[j + 1].Trim();
                        if (!childLine.StartsWith("- {fileID:"))
                        {
                            break;
                        }

                        j++;
                        if (TryParseFileId(childLine, out long childId))
                        {
                            groupInfo.Children.Add(childId);
                        }
                    }
                }

                groupInfos[groupId] = groupInfo;
            }

            Dictionary<long, string> results = new Dictionary<long, string>();
            if (masterGroupId == 0L)
            {
                return results;
            }

            CollectAudioMixerGroupPaths(masterGroupId, null, groupInfos, results, new HashSet<long>());
            return results;
        }

        private static void CollectAudioMixerGroupPaths(long groupId, string parentPath,
            Dictionary<long, AudioMixerGroupAssetInfo> groupInfos, Dictionary<long, string> results,
            HashSet<long> visited)
        {
            if (!visited.Add(groupId))
            {
                return;
            }

            if (!groupInfos.TryGetValue(groupId, out AudioMixerGroupAssetInfo groupInfo) ||
                string.IsNullOrEmpty(groupInfo.Name))
            {
                return;
            }

            string groupPath = string.IsNullOrEmpty(parentPath)
                ? groupInfo.Name
                : Utility.Text.Format("{0}/{1}", parentPath, groupInfo.Name);
            results[groupId] = groupPath;

            foreach (long childId in groupInfo.Children)
            {
                CollectAudioMixerGroupPaths(childId, groupPath, groupInfos, results, visited);
            }
        }

        private static bool TryParseFileIdFromLine(string line, string prefix, out long fileId)
        {
            fileId = 0L;
            if (!line.StartsWith(prefix))
            {
                return false;
            }

            return TryParseFileId(line, out fileId);
        }

        private static bool TryParseFileId(string text, out long fileId)
        {
            fileId = 0L;
            Match match = FileIdRegex.Match(text);
            return match.Success && long.TryParse(match.Groups["id"].Value, out fileId);
        }

        private static bool IsGeneratedTopLevelGroup(string groupPath, ICollection<string> allGroupPaths)
        {
            string groupPrefix = Utility.Text.Format("{0}/", groupPath);
            bool hasChild = false;
            foreach (string currentGroupPath in allGroupPaths)
            {
                if (!currentGroupPath.StartsWith(groupPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                hasChild = true;
                if (GetGroupPathDepth(currentGroupPath) != 3)
                {
                    return false;
                }

                string childGroupName = GetGroupNameFromPath(currentGroupPath);
                if (!int.TryParse(childGroupName, out int _))
                {
                    return false;
                }
            }

            return hasChild;
        }

        private static int GetGroupPathDepth(string groupPath)
        {
            if (string.IsNullOrEmpty(groupPath))
            {
                return 0;
            }

            return groupPath.Split('/').Length;
        }

        private static string GetParentGroupPath(string groupPath)
        {
            if (string.IsNullOrEmpty(groupPath))
            {
                return null;
            }

            int index = groupPath.LastIndexOf('/');
            return index > 0 ? groupPath.Substring(0, index) : null;
        }

        private static string GetGroupNameFromPath(string groupPath)
        {
            if (string.IsNullOrEmpty(groupPath))
            {
                return null;
            }

            int index = groupPath.LastIndexOf('/');
            return index >= 0 ? groupPath.Substring(index + 1) : groupPath;
        }

        private static bool IsChildPath(string parentPath, string childPath)
        {
            if (string.IsNullOrEmpty(parentPath) || string.IsNullOrEmpty(childPath))
            {
                return false;
            }

            return childPath.Length > parentPath.Length
                   && childPath.StartsWith(parentPath, StringComparison.Ordinal)
                   && childPath[parentPath.Length] == '/';
        }

        private static MethodInfo GetAudioMixerMethod(AudioMixer audioMixer, string methodName)
        {
            return audioMixer.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static AudioMixer GetSelectedAudioMixer()
        {
            if (Selection.activeObject is AudioMixer audioMixer)
            {
                return audioMixer;
            }

            if (Selection.activeGameObject == null)
            {
                return null;
            }

            AudioComponent audioComponent = Selection.activeGameObject.GetComponent<AudioComponent>();
            return audioComponent != null ? audioComponent.AudioMixer : null;
        }
    }
}