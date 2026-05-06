using UnityEditor;

namespace LFramework.Editor
{
    [CustomEditor(typeof(AudioComponent))]
    internal sealed class AudioComponentInspector : LFrameworkInspector
    {
        private SerializedProperty _audioMixer = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            AudioComponent t = (AudioComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                using (new EditorGUI.DisabledScope(t.AudioMixer == null))
                {
                    if (UnityEngine.GUILayout.Button("Generate Audio Mixer Groups"))
                    {
                        AudioMixerGroupGenerator.Generate(t.AudioMixer);
                    }
                }

                EditorGUILayout.PropertyField(_audioMixer);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Audio Group Count", t.AudioGroupCount.ToString());
                EditorGUILayout.LabelField("Loading Audio Count", t.LoadingAudioCount.ToString());

                int[] loadingAudioSerialIds = t.GetAllLoadingAudioSerialIds();
                EditorGUILayout.LabelField("Loading Audio Serial Ids",
                    loadingAudioSerialIds.Length > 0 ? string.Join(", ", loadingAudioSerialIds) : "None");

                foreach (IAudioGroup audioGroup in t.GetAllAudioGroups())
                {
                    AudioGroup runtimeAudioGroup = audioGroup as AudioGroup;
                    if (runtimeAudioGroup == null)
                    {
                        continue;
                    }

                    EditorGUILayout.BeginVertical("box");
                    {
                        EditorGUILayout.LabelField("Group Name", runtimeAudioGroup.AudioGroupName);
                        EditorGUILayout.LabelField("Mute", runtimeAudioGroup.Mute.ToString());
                        EditorGUILayout.LabelField("Volume", runtimeAudioGroup.Volume.ToString("F2"));
                        EditorGUILayout.LabelField("Playing / Total",
                            string.Format("{0} / {1}", runtimeAudioGroup.PlayingAudioAgentCount,
                                runtimeAudioGroup.AudioAgentCount));
                        EditorGUILayout.LabelField("Free Agent Count",
                            runtimeAudioGroup.FreeAudioAgentCount.ToString());

                        AudioAgent[] audioAgents = runtimeAudioGroup.GetAllAudioAgents();
                        for (int i = 0; i < audioAgents.Length; i++)
                        {
                            AudioAgent audioAgent = audioAgents[i];
                            string state = audioAgent.IsPlaying
                                ? (audioAgent.IsPaused ? "Paused" : "Playing")
                                : "Idle";
                            string bindingState = audioAgent.IsFollowingBindingEntity ? "Binding" : "World";
                            string audioName = string.IsNullOrEmpty(audioAgent.AudioName)
                                ? "None"
                                : audioAgent.AudioName;
                            EditorGUILayout.LabelField(string.Format("Agent {0}", i),
                                string.Format("State:{0}, Serial:{1}, Priority:{2}, Audio:{3}, Mode:{4}", state,
                                    audioAgent.SerialId, audioAgent.Priority, audioName, bindingState));
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            _audioMixer = serializedObject.FindProperty("audioMixer");

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}