using System;
using System.Collections.Generic;
using UnityEngine;

namespace LFramework
{
    public sealed partial class DebuggerComponent
    {
        [Serializable]
        private sealed class ConsoleWindow : IDebuggerWindow
        {
            [SerializeField] private bool lockScroll = true;

            [SerializeField] private int maxLine = 100;

            [SerializeField] private bool infoFilter = true;

            [SerializeField] private bool warningFilter = true;

            [SerializeField] private bool errorFilter = true;

            [SerializeField] private bool fatalFilter = true;

            [SerializeField] private Color32 infoColor = Color.white;

            [SerializeField] private Color32 warningColor = Color.yellow;

            [SerializeField] private Color32 errorColor = Color.red;

            [SerializeField] private Color32 fatalColor = new Color(0.7f, 0.2f, 0.2f);

            private readonly Queue<LogNode> _logNodes = new Queue<LogNode>();

            private ISettingManager _settingComponent = null;
            private Vector2 _logScrollPosition = Vector2.zero;
            private Vector2 _stackScrollPosition = Vector2.zero;
            private int _infoCount = 0;
            private int _warningCount = 0;
            private int _errorCount = 0;
            private int _fatalCount = 0;
            private LogNode _selectedNode = null;
            private bool _lastLockScroll = true;
            private bool _lastInfoFilter = true;
            private bool _lastWarningFilter = true;
            private bool _lastErrorFilter = true;
            private bool _lastFatalFilter = true;

            public bool LockScroll
            {
                get { return lockScroll; }
                set { lockScroll = value; }
            }

            public int MaxLine
            {
                get { return maxLine; }
                set { maxLine = value; }
            }

            public bool InfoFilter
            {
                get { return infoFilter; }
                set { infoFilter = value; }
            }

            public bool WarningFilter
            {
                get { return warningFilter; }
                set { warningFilter = value; }
            }

            public bool ErrorFilter
            {
                get { return errorFilter; }
                set { errorFilter = value; }
            }

            public bool FatalFilter
            {
                get { return fatalFilter; }
                set { fatalFilter = value; }
            }

            public int InfoCount
            {
                get { return _infoCount; }
            }

            public int WarningCount
            {
                get { return _warningCount; }
            }

            public int ErrorCount
            {
                get { return _errorCount; }
            }

            public int FatalCount
            {
                get { return _fatalCount; }
            }

            public Color32 InfoColor
            {
                get { return infoColor; }
                set { infoColor = value; }
            }

            public Color32 WarningColor
            {
                get { return warningColor; }
                set { warningColor = value; }
            }

            public Color32 ErrorColor
            {
                get { return errorColor; }
                set { errorColor = value; }
            }

            public Color32 FatalColor
            {
                get { return fatalColor; }
                set { fatalColor = value; }
            }

            public void Initialize(params object[] args)
            {
                _settingComponent = LFrameworkEntry.GetModule<ISettingManager>();
                if (_settingComponent == null)
                {
                    Log.Fatal("Setting component is invalid.");
                    return;
                }

                Application.logMessageReceived += OnLogMessageReceived;
                lockScroll = _lastLockScroll = _settingComponent.GetBool("Debugger.Console.LockScroll", true);
                infoFilter = _lastInfoFilter = _settingComponent.GetBool("Debugger.Console.InfoFilter", true);
                warningFilter = _lastWarningFilter = _settingComponent.GetBool("Debugger.Console.WarningFilter", true);
                errorFilter = _lastErrorFilter = _settingComponent.GetBool("Debugger.Console.ErrorFilter", true);
                fatalFilter = _lastFatalFilter = _settingComponent.GetBool("Debugger.Console.FatalFilter", true);
            }

            public void Shutdown()
            {
                Application.logMessageReceived -= OnLogMessageReceived;
                Clear();
            }

            public void OnEnter()
            {
            }

            public void OnLeave()
            {
            }

            public void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                if (_lastLockScroll != lockScroll)
                {
                    _lastLockScroll = lockScroll;
                    _settingComponent.SetBool("Debugger.Console.LockScroll", lockScroll);
                }

                if (_lastInfoFilter != infoFilter)
                {
                    _lastInfoFilter = infoFilter;
                    _settingComponent.SetBool("Debugger.Console.InfoFilter", infoFilter);
                }

                if (_lastWarningFilter != warningFilter)
                {
                    _lastWarningFilter = warningFilter;
                    _settingComponent.SetBool("Debugger.Console.WarningFilter", warningFilter);
                }

                if (_lastErrorFilter != errorFilter)
                {
                    _lastErrorFilter = errorFilter;
                    _settingComponent.SetBool("Debugger.Console.ErrorFilter", errorFilter);
                }

                if (_lastFatalFilter != fatalFilter)
                {
                    _lastFatalFilter = fatalFilter;
                    _settingComponent.SetBool("Debugger.Console.FatalFilter", fatalFilter);
                }
            }

            public void OnDraw()
            {
                RefreshCount();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Clear All", GUILayout.Width(100f)))
                    {
                        Clear();
                    }

                    lockScroll = GUILayout.Toggle(lockScroll, "Lock Scroll", GUILayout.Width(90f));
                    GUILayout.FlexibleSpace();
                    infoFilter = GUILayout.Toggle(infoFilter, Utility.Text.Format("Info ({0})", _infoCount),
                        GUILayout.Width(90f));
                    warningFilter = GUILayout.Toggle(warningFilter, Utility.Text.Format("Warning ({0})", _warningCount),
                        GUILayout.Width(90f));
                    errorFilter = GUILayout.Toggle(errorFilter, Utility.Text.Format("Error ({0})", _errorCount),
                        GUILayout.Width(90f));
                    fatalFilter = GUILayout.Toggle(fatalFilter, Utility.Text.Format("Fatal ({0})", _fatalCount),
                        GUILayout.Width(90f));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("box");
                {
                    if (lockScroll)
                    {
                        _logScrollPosition.y = float.MaxValue;
                    }

                    _logScrollPosition = GUILayout.BeginScrollView(_logScrollPosition);
                    {
                        bool selected = false;
                        foreach (LogNode logNode in _logNodes)
                        {
                            switch (logNode.LogType)
                            {
                                case LogType.Log:
                                    if (!infoFilter)
                                    {
                                        continue;
                                    }

                                    break;

                                case LogType.Warning:
                                    if (!warningFilter)
                                    {
                                        continue;
                                    }

                                    break;

                                case LogType.Error:
                                    if (!errorFilter)
                                    {
                                        continue;
                                    }

                                    break;

                                case LogType.Exception:
                                    if (!fatalFilter)
                                    {
                                        continue;
                                    }

                                    break;
                            }

                            if (GUILayout.Toggle(_selectedNode == logNode, GetLogString(logNode)))
                            {
                                selected = true;
                                if (_selectedNode != logNode)
                                {
                                    _selectedNode = logNode;
                                    _stackScrollPosition = Vector2.zero;
                                }
                            }
                        }

                        if (!selected)
                        {
                            _selectedNode = null;
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                {
                    _stackScrollPosition = GUILayout.BeginScrollView(_stackScrollPosition, GUILayout.Height(100f));
                    {
                        if (_selectedNode != null)
                        {
                            Color32 color = GetLogStringColor(_selectedNode.LogType);
                            if (GUILayout.Button(
                                    Utility.Text.Format("<color=#{0:x2}{1:x2}{2:x2}{3:x2}><b>{4}</b></color>{6}{6}{5}",
                                        color.r, color.g, color.b, color.a, _selectedNode.LogMessage,
                                        _selectedNode.StackTrack, Environment.NewLine), "label"))
                            {
                                CopyToClipboard(Utility.Text.Format("{0}{2}{2}{1}", _selectedNode.LogMessage,
                                    _selectedNode.StackTrack, Environment.NewLine));
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }

            private void Clear()
            {
                _logNodes.Clear();
            }

            public void RefreshCount()
            {
                _infoCount = 0;
                _warningCount = 0;
                _errorCount = 0;
                _fatalCount = 0;
                foreach (LogNode logNode in _logNodes)
                {
                    switch (logNode.LogType)
                    {
                        case LogType.Log:
                            _infoCount++;
                            break;

                        case LogType.Warning:
                            _warningCount++;
                            break;

                        case LogType.Error:
                            _errorCount++;
                            break;

                        case LogType.Exception:
                            _fatalCount++;
                            break;
                    }
                }
            }

            public void GetRecentLogs(List<LogNode> results)
            {
                if (results == null)
                {
                    Log.Error("Results is invalid.");
                    return;
                }

                results.Clear();
                foreach (LogNode logNode in _logNodes)
                {
                    results.Add(logNode);
                }
            }

            public void GetRecentLogs(List<LogNode> results, int count)
            {
                if (results == null)
                {
                    Log.Error("Results is invalid.");
                    return;
                }

                if (count <= 0)
                {
                    Log.Error("Count is invalid.");
                    return;
                }

                int position = _logNodes.Count - count;
                if (position < 0)
                {
                    position = 0;
                }

                int index = 0;
                results.Clear();
                foreach (LogNode logNode in _logNodes)
                {
                    if (index++ < position)
                    {
                        continue;
                    }

                    results.Add(logNode);
                }
            }

            private void OnLogMessageReceived(string logMessage, string stackTrace, LogType logType)
            {
                if (logType == LogType.Assert)
                {
                    logType = LogType.Error;
                }

                _logNodes.Enqueue(LogNode.Create(logType, logMessage, stackTrace));
                while (_logNodes.Count > maxLine)
                {
                    ReferencePool.Release(_logNodes.Dequeue());
                }
            }

            private string GetLogString(LogNode logNode)
            {
                Color32 color = GetLogStringColor(logNode.LogType);
                return Utility.Text.Format("<color=#{0:x2}{1:x2}{2:x2}{3:x2}>[{4:HH:mm:ss.fff}][{5}] {6}</color>",
                    color.r, color.g, color.b, color.a, logNode.LogTime.ToLocalTime(), logNode.LogFrameCount,
                    logNode.LogMessage);
            }

            internal Color32 GetLogStringColor(LogType logType)
            {
                Color32 color = Color.white;
                switch (logType)
                {
                    case LogType.Log:
                        color = infoColor;
                        break;

                    case LogType.Warning:
                        color = warningColor;
                        break;

                    case LogType.Error:
                        color = errorColor;
                        break;

                    case LogType.Exception:
                        color = fatalColor;
                        break;
                }

                return color;
            }
        }
    }
}