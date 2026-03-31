using UnityEngine;

namespace LFramework
{
    public sealed partial class DebuggerComponent
    {
        private sealed class OperationsWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Operations</b>");
                GUILayout.BeginVertical("box");
                {
                    IObjectPoolManager objectPoolComponent = LFrameworkEntry.GetModule<IObjectPoolManager>();
                    if (objectPoolComponent != null)
                    {
                        if (GUILayout.Button("Object Pool Release", GUILayout.Height(30f)))
                        {
                            objectPoolComponent.Release();
                        }

                        if (GUILayout.Button("Object Pool Release All Unused", GUILayout.Height(30f)))
                        {
                            objectPoolComponent.ReleaseAllUnused();
                        }
                    }

                    IResourceManager resourceCompoent = LFrameworkEntry.GetModule<IResourceManager>();
                    if (resourceCompoent != null)
                    {
                        if (GUILayout.Button("Unload Unused Assets", GUILayout.Height(30f)))
                        {
                            resourceCompoent.ForceUnloadUnusedAssets(false);
                        }

                        if (GUILayout.Button("Unload Unused Assets and Garbage Collect", GUILayout.Height(30f)))
                        {
                            resourceCompoent.ForceUnloadUnusedAssets(true);
                        }
                    }

                    if (GUILayout.Button("Shutdown LFramework (None)", GUILayout.Height(30f)))
                    {
                        RootComponent.Instance.Shutdown(ShutdownType.None);
                    }

                    if (GUILayout.Button("Shutdown LFramework (Restart)", GUILayout.Height(30f)))
                    {
                        RootComponent.Instance.Shutdown(ShutdownType.Restart);
                    }

                    if (GUILayout.Button("Shutdown LFramework (Quit)", GUILayout.Height(30f)))
                    {
                        RootComponent.Instance.Shutdown(ShutdownType.Quit);
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}