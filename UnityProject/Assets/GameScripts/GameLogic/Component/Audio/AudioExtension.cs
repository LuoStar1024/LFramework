using LFramework;
using UnityEngine;

namespace GameLogic
{
    public static partial class AudioExtension
    {
        private const float FadeVolumeDuration = 1f;
        private static int? _musicSerialId = null;

        public static int? PlayBgm(this IAudioManager audioComponent, int musicId, object userData = null)
        {
            audioComponent.StopBgm();

            var cfgAudio = GameEntry.DataTable.TbSound.Get(musicId);
            if (cfgAudio == null)
            {
                Log.Warning("Can not load music '{0}' from data table.", musicId.ToString());
                return null;
            }

            PlayAudioParams playAudioParams = PlayAudioParams.Create();
            playAudioParams.Priority = 64;
            playAudioParams.Loop = true;
            playAudioParams.VolumeInAudioGroup = 1f;
            playAudioParams.FadeInSeconds = FadeVolumeDuration;
            playAudioParams.SpatialBlend = 0f;
            _musicSerialId = audioComponent.PlayAudio(AssetUtility.GetAudioBgmAsset(cfgAudio.AssetName),
                Constant.Setting.AudioGroupBgm, Constant.AssetPriority.BgmAsset, playAudioParams, null, userData);
            return _musicSerialId;
        }

        public static void StopBgm(this IAudioManager audioComponent)
        {
            if (!_musicSerialId.HasValue)
            {
                return;
            }

            audioComponent.StopAudio(_musicSerialId.Value, FadeVolumeDuration);
            _musicSerialId = null;
        }

        public static int? PlaySound(this IAudioManager audioComponent, int audioId, Transform bindingTrans = null,
            object userData = null)
        {
            var cfgAudio = GameEntry.DataTable.TbSound.Get(audioId);
            if (cfgAudio == null)
            {
                Log.Warning("Can not load audio '{0}' from data table.", audioId.ToString());
                return null;
            }

            PlayAudioParams playAudioParams = PlayAudioParams.Create();
            playAudioParams.Priority = cfgAudio.Priority;
            playAudioParams.Loop = cfgAudio.Loop;
            playAudioParams.VolumeInAudioGroup = cfgAudio.Volume;
            playAudioParams.SpatialBlend = cfgAudio.SpatialBlend;
            return audioComponent.PlayAudio(AssetUtility.GetAudioSoundAsset(cfgAudio.AssetName),
                Constant.Setting.AudioGroupSound, Constant.AssetPriority.SoundAsset, playAudioParams,
                bindingTrans != null ? bindingTrans : null, userData);
        }

        public static int? PlayUISound(this IAudioManager audioComponent, int uiAudioId, object userData = null)
        {
            var cfgAudio = GameEntry.DataTable.TbSound.Get(uiAudioId);
            if (cfgAudio == null)
            {
                Log.Warning("Can not load UI audio '{0}' from data table.", uiAudioId.ToString());
                return null;
            }

            PlayAudioParams playAudioParams = PlayAudioParams.Create();
            playAudioParams.Priority = cfgAudio.Priority;
            playAudioParams.Loop = false;
            playAudioParams.VolumeInAudioGroup = cfgAudio.Volume;
            playAudioParams.SpatialBlend = 0f;
            return audioComponent.PlayAudio(AssetUtility.GetAudioUISoundAsset(cfgAudio.AssetName),
                Constant.Setting.AudioGroupUISound, Constant.AssetPriority.UISoundAsset, playAudioParams, userData);
        }

        public static bool IsMuted(this IAudioManager audioComponent, string audioGroupName)
        {
            if (string.IsNullOrEmpty(audioGroupName))
            {
                Log.Warning("Audio group is invalid.");
                return true;
            }

            IAudioGroup audioGroup = audioComponent.GetAudioGroup(audioGroupName);
            if (audioGroup == null)
            {
                Log.Warning("Audio group '{0}' is invalid.", audioGroupName);
                return true;
            }

            return audioGroup.Mute;
        }

        public static void Mute(this IAudioManager audioComponent, string audioGroupName, bool mute)
        {
            if (string.IsNullOrEmpty(audioGroupName))
            {
                Log.Warning("Audio group is invalid.");
                return;
            }

            IAudioGroup audioGroup = audioComponent.GetAudioGroup(audioGroupName);
            if (audioGroup == null)
            {
                Log.Warning("Audio group '{0}' is invalid.", audioGroupName);
                return;
            }

            audioGroup.Mute = mute;

            GameEntry.Setting.SetBool(Utility.Text.Format(Constant.Setting.AudioGroupMuted, audioGroupName), mute);
            GameEntry.Setting.Save();
        }

        public static float GetVolume(this IAudioManager audioComponent, string audioGroupName)
        {
            if (string.IsNullOrEmpty(audioGroupName))
            {
                Log.Warning("Audio group is invalid.");
                return 0f;
            }

            IAudioGroup audioGroup = audioComponent.GetAudioGroup(audioGroupName);
            if (audioGroup == null)
            {
                Log.Warning("Audio group '{0}' is invalid.", audioGroupName);
                return 0f;
            }

            return audioGroup.Volume;
        }

        public static void SetVolume(this IAudioManager audioComponent, string audioGroupName, float volume)
        {
            if (string.IsNullOrEmpty(audioGroupName))
            {
                Log.Warning("Audio group is invalid.");
                return;
            }

            IAudioGroup audioGroup = audioComponent.GetAudioGroup(audioGroupName);
            if (audioGroup == null)
            {
                Log.Warning("Audio group '{0}' is invalid.", audioGroupName);
                return;
            }

            audioGroup.Volume = volume;

            GameEntry.Setting.SetFloat(Utility.Text.Format(Constant.Setting.AudioGroupVolume, audioGroupName), volume);
            GameEntry.Setting.Save();
        }
    }
}