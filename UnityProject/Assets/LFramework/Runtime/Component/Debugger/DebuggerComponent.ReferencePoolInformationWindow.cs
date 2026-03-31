using System;
using System.Collections.Generic;
using UnityEngine;

namespace LFramework
{
    public sealed partial class DebuggerComponent
    {
        private sealed class ReferencePoolInformationWindow : ScrollableDebuggerWindowBase
        {
            private readonly Dictionary<string, List<ReferencePoolInfo>> _referencePoolInfos =
                new Dictionary<string, List<ReferencePoolInfo>>(StringComparer.Ordinal);

            private readonly Comparison<ReferencePoolInfo> _normalClassNameComparer = NormalClassNameComparer;
            private readonly Comparison<ReferencePoolInfo> _fullClassNameComparer = FullClassNameComparer;
            private bool _showFullClassName = false;

            public override void Initialize(params object[] args)
            {
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Reference Pool Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Enable Strict Check", ReferencePool.EnableStrictCheck.ToString());
                    DrawItem("Reference Pool Count", ReferencePool.Count.ToString());
                }
                GUILayout.EndVertical();

                _showFullClassName = GUILayout.Toggle(_showFullClassName, "Show Full Class Name");
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
                    GUILayout.Label(Utility.Text.Format("<b>Assembly: {0}</b>", assemblyReferencePoolInfo.Key));
                    GUILayout.BeginVertical("box");
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(_showFullClassName ? "<b>Full Class Name</b>" : "<b>Class Name</b>");
                            GUILayout.Label("<b>Unused</b>", GUILayout.Width(60f));
                            GUILayout.Label("<b>Using</b>", GUILayout.Width(60f));
                            GUILayout.Label("<b>Acquire</b>", GUILayout.Width(60f));
                            GUILayout.Label("<b>Release</b>", GUILayout.Width(60f));
                            GUILayout.Label("<b>Add</b>", GUILayout.Width(60f));
                            GUILayout.Label("<b>Remove</b>", GUILayout.Width(60f));
                        }
                        GUILayout.EndHorizontal();

                        if (assemblyReferencePoolInfo.Value.Count > 0)
                        {
                            assemblyReferencePoolInfo.Value.Sort(_showFullClassName
                                ? _fullClassNameComparer
                                : _normalClassNameComparer);
                            foreach (ReferencePoolInfo referencePoolInfo in assemblyReferencePoolInfo.Value)
                            {
                                DrawReferencePoolInfo(referencePoolInfo);
                            }
                        }
                        else
                        {
                            GUILayout.Label("<i>Reference Pool is Empty ...</i>");
                        }
                    }
                    GUILayout.EndVertical();
                }
            }

            private void DrawReferencePoolInfo(ReferencePoolInfo referencePoolInfo)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(_showFullClassName ? referencePoolInfo.Type.FullName : referencePoolInfo.Type.Name);
                    GUILayout.Label(referencePoolInfo.UnusedReferenceCount.ToString(), GUILayout.Width(60f));
                    GUILayout.Label(referencePoolInfo.UsingReferenceCount.ToString(), GUILayout.Width(60f));
                    GUILayout.Label(referencePoolInfo.AcquireReferenceCount.ToString(), GUILayout.Width(60f));
                    GUILayout.Label(referencePoolInfo.ReleaseReferenceCount.ToString(), GUILayout.Width(60f));
                    GUILayout.Label(referencePoolInfo.AddReferenceCount.ToString(), GUILayout.Width(60f));
                    GUILayout.Label(referencePoolInfo.RemoveReferenceCount.ToString(), GUILayout.Width(60f));
                }
                GUILayout.EndHorizontal();
            }

            private static int NormalClassNameComparer(ReferencePoolInfo a, ReferencePoolInfo b)
            {
                return a.Type.Name.CompareTo(b.Type.Name);
            }

            private static int FullClassNameComparer(ReferencePoolInfo a, ReferencePoolInfo b)
            {
                return a.Type.FullName.CompareTo(b.Type.FullName);
            }
        }
    }
}