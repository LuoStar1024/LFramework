using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.Serialization;

namespace GameEditor
{
    /// <summary>
    /// 事件参数类代码生成器
    /// </summary>
    public class EventArgsCodeGenerator : EditorWindow
    {
        /// <summary>
        /// 事件参数数据
        /// </summary>
        [Serializable]
        private class EventArgsData
        {
            public string type;
            public string name;
            public EventArgType typeEnum;
            public EventArgsData()
            {

            }

            public EventArgsData(string type, string name)
            {
                this.type = type;
                this.name = name;
            }
        }

        private enum EventArgType
        {
            Object,
            Int,
            Float,
            Bool,
            Char,
            String,

            UnityObject,
            GameObject,
            Transform,
            Vector2,
            Vector3,
            Quaternion,

            Other,
        }

        [MenuItem("LFramework/CodeGenerator/EventArgsCodeGenerator")]
        public static void OpenAutoGenWindow()
        {
            EventArgsCodeGenerator window = GetWindow<EventArgsCodeGenerator>(true, "EventArgsCodeGenerator");
            window.minSize = new Vector2(600f, 600f);
        }

        /// <summary>
        /// 事件参数数据列表
        /// </summary>
        [SerializeField]
        private List<EventArgsData> eventArgsDataList = new List<EventArgsData>();

        /// <summary>
        /// 是否是热更新层事件
        /// </summary>
        private bool _isHotfixEvent = false;

        /// <summary>
        /// 事件参数类名
        /// </summary>
        private string _className;

        // 事件代码生成后的路径
        private const string EventCodePath = "Assets/Launcher/Scripts/EventArgs/EventArgsClass";
        private const string HotfixEventCodePath = "Assets/GameScripts/Hotfix/EventArgs";
        private void OnEnable()
        {
            eventArgsDataList.Clear();
            _className = "EventArgs";
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("事件参数类名：", GUILayout.Width(140f));
            _className = EditorGUILayout.TextField(_className);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("热更新层事件：", GUILayout.Width(140f));
            _isHotfixEvent = EditorGUILayout.Toggle(_isHotfixEvent);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("自动生成的代码路径：", GUILayout.Width(140f));
            EditorGUILayout.LabelField(_isHotfixEvent ? HotfixEventCodePath : EventCodePath);
            EditorGUILayout.EndHorizontal();

            // 绘制事件参数相关按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("添加事件参数", GUILayout.Width(140f)))
            {
                eventArgsDataList.Add(new EventArgsData(null, null));
            }
            if (GUILayout.Button("删除所有事件参数", GUILayout.Width(140f)))
            {
                eventArgsDataList.Clear();
            }
            if (GUILayout.Button("删除空事件参数", GUILayout.Width(140f)))
            {
                for (int i = eventArgsDataList.Count - 1; i >= 0; i--)
                {
                    EventArgsData data = eventArgsDataList[i];
                    if (string.IsNullOrWhiteSpace(data.name))
                    {
                        eventArgsDataList.RemoveAt(i);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // 绘制事件参数数据
            for (int i = 0; i < eventArgsDataList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EventArgsData data = eventArgsDataList[i];
                EditorGUILayout.LabelField("参数类型：", GUILayout.Width(70f));
                data.typeEnum = (EventArgType)EditorGUILayout.EnumPopup(data.typeEnum, GUILayout.Width(100f));
                switch (data.typeEnum)
                {
                    case EventArgType.Object:
                    case EventArgType.Int:
                    case EventArgType.Float:
                    case EventArgType.Bool:
                    case EventArgType.Char:
                    case EventArgType.String:
                        data.type = data.typeEnum.ToString().ToLower();
                        break;

                    case EventArgType.UnityObject:
                        data.type = "UnityEngine.Object";
                        break;

                    case EventArgType.Other:
                        data.type = EditorGUILayout.TextField(data.type, GUILayout.Width(140f));
                        break;

                    default:
                        data.type = data.typeEnum.ToString();
                        break;
                }
                EditorGUILayout.LabelField("参数字段名：", GUILayout.Width(70f));
                data.name = EditorGUILayout.TextField(data.name, GUILayout.Width(140f));
                EditorGUILayout.EndHorizontal();
            }

            // 生成事件参数类代码
            if (GUILayout.Button("生成事件参数类代码", GUILayout.Width(210f)))
            {
                GenEventCode();
                AssetDatabase.Refresh();
            }
        }

        private void GenEventCode()
        {
            // 根据是否为热更新层事件来决定一些参数
            string codePath = _isHotfixEvent ? HotfixEventCodePath : EventCodePath;
            string nameSpace = _isHotfixEvent ? "LFramework.Hotfix" : "Launcher";
            string baseClass = _isHotfixEvent ? "HotfixGameEventArgs" : "GameEventArgs";

            if (!Directory.Exists($"{codePath}/"))
            {
                Directory.CreateDirectory($"{codePath}/");
            }

            using (StreamWriter sw = new StreamWriter($"{codePath}/{_className}.cs"))
            {
                sw.WriteLine("// 自动生成于：" + DateTime.Now);
                sw.WriteLine("");
                
                // sw.WriteLine("using UnityEngine;");
                sw.WriteLine("using LFramework;");
                sw.WriteLine("");

                // 命名空间
                sw.WriteLine("namespace " + nameSpace);
                sw.WriteLine("{");
                sw.WriteLine("");

                // 类名
                sw.WriteLine($"\tpublic class {_className} : {baseClass}");
                sw.WriteLine("\t{");

                // 事件编号
                sw.WriteLine($"\t\tpublic static readonly int EventId = typeof({_className}).GetHashCode();");
                sw.WriteLine("");
                
                // 构造函数
                sw.WriteLine($"\t\tpublic {_className} ()");
                sw.WriteLine("\t\t{");
                // 参数初始化
                for (int i = 0; i < eventArgsDataList.Count; i++)
                {
                    EventArgsData data = eventArgsDataList[i];
                    sw.WriteLine($"\t\t\t{data.name[0].ToString().ToUpper() + data.name.Substring(1)} = default({data.type});");
                }
                sw.WriteLine("\t\t}");
                sw.WriteLine("");
                
                sw.WriteLine("\t\tpublic override int Id");
                sw.WriteLine("\t\t{");
                sw.WriteLine("\t\t\tget");
                sw.WriteLine("\t\t\t{");
                sw.WriteLine("\t\t\t\treturn EventId;");
                sw.WriteLine("\t\t\t}");
                sw.WriteLine("\t\t}");
                sw.WriteLine("");

                // 事件参数
                for (int i = 0; i < eventArgsDataList.Count; i++)
                {
                    EventArgsData data = eventArgsDataList[i];
                    data.name = data.name[0].ToString().ToUpper() + data.name.Substring(1);
                    sw.WriteLine($"\t\tpublic {data.type} {data.name}");
                    sw.WriteLine("\t\t{");
                    sw.WriteLine("\t\t\tget;");
                    sw.WriteLine("\t\t\tprivate set;");
                    sw.WriteLine("\t\t}");
                    sw.WriteLine("");
                }
                
                // 分配引用
                sw.Write($"\t\tpublic static {_className} Create(");
                for (int i = 0; i < eventArgsDataList.Count; i++)
                {
                    EventArgsData data = eventArgsDataList[i];
                    sw.Write($"{data.type} {data.name[0].ToString().ToLower() + data.name.Substring(1)}");
                    if (i != eventArgsDataList.Count - 1)
                    {
                        sw.Write(", ");
                    }
                }
                sw.WriteLine(")");
                sw.WriteLine("\t\t{");
                sw.WriteLine($"\t\t\t{_className} {_className[0].ToString().ToLower() + _className.Substring(1)} = ReferencePool.Acquire<{_className}>();");
                for (int i = 0; i < eventArgsDataList.Count; i++)
                {
                    EventArgsData data = eventArgsDataList[i];
                    sw.WriteLine($"\t\t\t{_className[0].ToString().ToLower() + _className.Substring(1)}.{data.name} = {data.name[0].ToString().ToLower() + data.name.Substring(1)};");
                }
                sw.WriteLine($"\t\t\treturn {_className[0].ToString().ToLower() + _className.Substring(1)};");
                sw.WriteLine("\t\t}");

                //清空参数数据方法
                sw.WriteLine($"\t\tpublic override void Clear()");
                sw.WriteLine("\t\t{");
                for (int i = 0; i < eventArgsDataList.Count; i++)
                {
                    EventArgsData data = eventArgsDataList[i];
                    sw.WriteLine($"\t\t\t{data.name} = default({data.type});");
                }
                sw.WriteLine("\t\t}");
                
                sw.WriteLine("\t}");
                sw.WriteLine("}");
            }
        }
    }
}