using UnityEditor;
using UnityEngine;

namespace LFramework.Editor
{
    [CustomEditor(typeof(TimerComponent))]
    internal sealed class TimerComponentInspector : LFrameworkInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            TimerComponent t = (TimerComponent)target;

            if (IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Timer Count", t.TimerCount.ToString());
                TimerInfo[] timers = t.GetTimersInfo();
                foreach (var timer in timers)
                {
                    DrawTimer(timer);
                }
                
                EditorGUILayout.LabelField("");
                
                EditorGUILayout.LabelField("Unscaled Timer Count", t.UnscaledTimerCount.ToString());
                TimerInfo[] unscaledTimers = t.GetUnscaledTimersInfo();
                foreach (var timer in unscaledTimers)
                {
                    DrawTimer(timer);
                }
            }

            Repaint();
        }

        private void OnEnable()
        {
        }
        
        private void DrawTimer(TimerInfo timer)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Utility.Text.Format("{0}.{1}.{2}", timer.Id, timer.ClassName, timer.MethodName),
                GUILayout.Width(Screen.width * 0.5f));
            EditorGUILayout.LabelField(Utility.Text.Format("time:{0:F1}, rep:{1}, curT:{2:F1}", timer.Time, timer.RepeatCount, timer.CurTime),
                GUILayout.Width(Screen.width * 0.5f));
            EditorGUILayout.EndHorizontal();
        }
    }
}
