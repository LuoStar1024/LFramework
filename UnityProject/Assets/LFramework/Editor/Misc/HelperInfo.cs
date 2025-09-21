using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LFramework.Editor
{
    internal sealed class HelperInfo<T> where T : MonoBehaviour
    {
        private const string CustomOptionName = "<Custom>";

        private readonly string _name;

        private SerializedProperty _helperTypeName;
        private SerializedProperty _customHelper;
        private string[] _helperTypeNames;
        private int _helperTypeNameIndex;

        public HelperInfo(string name)
        {
            _name = name;

            _helperTypeName = null;
            _customHelper = null;
            _helperTypeNames = null;
            _helperTypeNameIndex = 0;
        }

        public void Init(SerializedObject serializedObject)
        {
            // UI特殊，它就是一个整体，进行单独处理
            if (_name.Substring(0, 2) == "UI")
            {
                _helperTypeName = serializedObject.FindProperty(Utility.Text.Format("{0}HelperTypeName",
                    _name[..2].ToLower() + _name[2..]));
            }
            else
            {
                _helperTypeName = serializedObject.FindProperty(Utility.Text.Format("{0}HelperTypeName",
                    char.ToLower(_name[0]) + _name.Substring(1)));
            }
            _customHelper = serializedObject.FindProperty(Utility.Text.Format("custom{0}Helper", _name));
        }

        public void Draw()
        {
            string displayName = FieldNameForDisplay(_name);
            int selectedIndex = EditorGUILayout.Popup(Utility.Text.Format("{0} Helper", displayName),
                _helperTypeNameIndex, _helperTypeNames);
            if (selectedIndex != _helperTypeNameIndex)
            {
                _helperTypeNameIndex = selectedIndex;
                _helperTypeName.stringValue = selectedIndex <= 0 ? null : _helperTypeNames[selectedIndex];
            }

            if (_helperTypeNameIndex <= 0)
            {
                EditorGUILayout.PropertyField(_customHelper);
                if (_customHelper.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(Utility.Text.Format("You must set Custom {0} Helper.", displayName),
                        MessageType.Error);
                }
            }
        }

        public void Refresh()
        {
            List<string> helperTypeNameList = new List<string>
            {
                CustomOptionName
            };

            helperTypeNameList.AddRange(Type.GetRuntimeTypeNames(typeof(T)));
            _helperTypeNames = helperTypeNameList.ToArray();

            _helperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(_helperTypeName.stringValue))
            {
                _helperTypeNameIndex = helperTypeNameList.IndexOf(_helperTypeName.stringValue);
                if (_helperTypeNameIndex <= 0)
                {
                    _helperTypeNameIndex = 0;
                    _helperTypeName.stringValue = null;
                }
            }
        }

        private string FieldNameForDisplay(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            string str = Regex.Replace(fieldName, @"^m_", string.Empty);
            str = Regex.Replace(str, @"((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", @" $1").TrimStart();
            return str;
        }
    }
}