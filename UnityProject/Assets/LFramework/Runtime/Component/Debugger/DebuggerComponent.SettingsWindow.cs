using UnityEngine;

namespace LFramework
{
    public sealed partial class DebuggerComponent
    {
        private sealed class SettingsWindow : ScrollableDebuggerWindowBase
        {
            private IDebuggerManager _debuggerComponent = null;
            private ISettingManager _settingComponent = null;
            private float _lastIconX = 0f;
            private float _lastIconY = 0f;
            private float _lastWindowX = 0f;
            private float _lastWindowY = 0f;
            private float _lastWindowWidth = 0f;
            private float _lastWindowHeight = 0f;
            private float _lastWindowScale = 0f;

            public override void Initialize(params object[] args)
            {
                _debuggerComponent = LFrameworkEntry.GetModule<IDebuggerManager>();
                if (_debuggerComponent == null)
                {
                    Log.Fatal("Debugger component is invalid.");
                    return;
                }

                _settingComponent = LFrameworkEntry.GetModule<ISettingManager>();
                if (_settingComponent == null)
                {
                    Log.Fatal("Setting component is invalid.");
                    return;
                }

                _lastIconX = _settingComponent.GetFloat("Debugger.Icon.X", DefaultIconRect.x);
                _lastIconY = _settingComponent.GetFloat("Debugger.Icon.Y", DefaultIconRect.y);
                _lastWindowX = _settingComponent.GetFloat("Debugger.Window.X", DefaultWindowRect.x);
                _lastWindowY = _settingComponent.GetFloat("Debugger.Window.Y", DefaultWindowRect.y);
                _lastWindowWidth = _settingComponent.GetFloat("Debugger.Window.Width", DefaultWindowRect.width);
                _lastWindowHeight = _settingComponent.GetFloat("Debugger.Window.Height", DefaultWindowRect.height);
                _debuggerComponent.WindowScale = _lastWindowScale =
                    _settingComponent.GetFloat("Debugger.Window.Scale", DefaultWindowScale);
                _debuggerComponent.IconRect =
                    new Rect(_lastIconX, _lastIconY, DefaultIconRect.width, DefaultIconRect.height);
                _debuggerComponent.WindowRect =
                    new Rect(_lastWindowX, _lastWindowY, _lastWindowWidth, _lastWindowHeight);
            }

            public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                if (_lastIconX != _debuggerComponent.IconRect.x)
                {
                    _lastIconX = _debuggerComponent.IconRect.x;
                    _settingComponent.SetFloat("Debugger.Icon.X", _debuggerComponent.IconRect.x);
                }

                if (_lastIconY != _debuggerComponent.IconRect.y)
                {
                    _lastIconY = _debuggerComponent.IconRect.y;
                    _settingComponent.SetFloat("Debugger.Icon.Y", _debuggerComponent.IconRect.y);
                }

                if (_lastWindowX != _debuggerComponent.WindowRect.x)
                {
                    _lastWindowX = _debuggerComponent.WindowRect.x;
                    _settingComponent.SetFloat("Debugger.Window.X", _debuggerComponent.WindowRect.x);
                }

                if (_lastWindowY != _debuggerComponent.WindowRect.y)
                {
                    _lastWindowY = _debuggerComponent.WindowRect.y;
                    _settingComponent.SetFloat("Debugger.Window.Y", _debuggerComponent.WindowRect.y);
                }

                if (_lastWindowWidth != _debuggerComponent.WindowRect.width)
                {
                    _lastWindowWidth = _debuggerComponent.WindowRect.width;
                    _settingComponent.SetFloat("Debugger.Window.Width", _debuggerComponent.WindowRect.width);
                }

                if (_lastWindowHeight != _debuggerComponent.WindowRect.height)
                {
                    _lastWindowHeight = _debuggerComponent.WindowRect.height;
                    _settingComponent.SetFloat("Debugger.Window.Height", _debuggerComponent.WindowRect.height);
                }

                if (_lastWindowScale != _debuggerComponent.WindowScale)
                {
                    _lastWindowScale = _debuggerComponent.WindowScale;
                    _settingComponent.SetFloat("Debugger.Window.Scale", _debuggerComponent.WindowScale);
                }
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Window Settings</b>");
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Position:", GUILayout.Width(60f));
                        GUILayout.Label("Drag window caption to move position.");
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float width = _debuggerComponent.WindowRect.width;
                        GUILayout.Label("Width:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            width--;
                        }

                        width = GUILayout.HorizontalSlider(width, 100f, Screen.width - 20f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            width++;
                        }

                        width = Mathf.Clamp(width, 100f, Screen.width - 20f);
                        if (width != _debuggerComponent.WindowRect.width)
                        {
                            _debuggerComponent.WindowRect = new Rect(_debuggerComponent.WindowRect.x,
                                _debuggerComponent.WindowRect.y, width, _debuggerComponent.WindowRect.height);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float height = _debuggerComponent.WindowRect.height;
                        GUILayout.Label("Height:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            height--;
                        }

                        height = GUILayout.HorizontalSlider(height, 100f, Screen.height - 20f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            height++;
                        }

                        height = Mathf.Clamp(height, 100f, Screen.height - 20f);
                        if (height != _debuggerComponent.WindowRect.height)
                        {
                            _debuggerComponent.WindowRect = new Rect(_debuggerComponent.WindowRect.x,
                                _debuggerComponent.WindowRect.y, _debuggerComponent.WindowRect.width, height);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float scale = _debuggerComponent.WindowScale;
                        GUILayout.Label("Scale:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            scale -= 0.01f;
                        }

                        scale = GUILayout.HorizontalSlider(scale, 0.5f, 4f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            scale += 0.01f;
                        }

                        scale = Mathf.Clamp(scale, 0.5f, 4f);
                        if (scale != _debuggerComponent.WindowScale)
                        {
                            _debuggerComponent.WindowScale = scale;
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("0.5x", GUILayout.Height(60f)))
                        {
                            _debuggerComponent.WindowScale = 0.5f;
                        }

                        if (GUILayout.Button("1.0x", GUILayout.Height(60f)))
                        {
                            _debuggerComponent.WindowScale = 1f;
                        }

                        if (GUILayout.Button("1.5x", GUILayout.Height(60f)))
                        {
                            _debuggerComponent.WindowScale = 1.5f;
                        }

                        if (GUILayout.Button("2.0x", GUILayout.Height(60f)))
                        {
                            _debuggerComponent.WindowScale = 2f;
                        }

                        if (GUILayout.Button("2.5x", GUILayout.Height(60f)))
                        {
                            _debuggerComponent.WindowScale = 2.5f;
                        }

                        if (GUILayout.Button("3.0x", GUILayout.Height(60f)))
                        {
                            _debuggerComponent.WindowScale = 3f;
                        }

                        if (GUILayout.Button("3.5x", GUILayout.Height(60f)))
                        {
                            _debuggerComponent.WindowScale = 3.5f;
                        }

                        if (GUILayout.Button("4.0x", GUILayout.Height(60f)))
                        {
                            _debuggerComponent.WindowScale = 4f;
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Reset Layout", GUILayout.Height(30f)))
                    {
                        _debuggerComponent.ResetLayout();
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}