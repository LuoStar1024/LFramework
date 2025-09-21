using UnityEngine;

namespace LFramework
{
    public sealed partial class SoundComponent
    {
        private sealed class PlaySoundInfo : IReference
        {
            private int _serialId;
            private SoundGroup _soundGroup;
            private PlaySoundParams _playSoundParams;
            private Transform _bindingTrans;
            private Vector3 _worldPosition;
            private object _userData;

            public PlaySoundInfo()
            {
                _serialId = 0;
                _soundGroup = null;
                _playSoundParams = null;
                _userData = null;
            }

            public int SerialId
            {
                get { return _serialId; }
            }

            public SoundGroup SoundGroup
            {
                get { return _soundGroup; }
            }

            public PlaySoundParams PlaySoundParams
            {
                get { return _playSoundParams; }
            }
            
            public Transform BindingTrans
            {
                get { return _bindingTrans; }
            }
            
            public Vector3 WorldPosition
            {
                get { return _worldPosition; }
            }

            public object UserData
            {
                get { return _userData; }
            }

            public static PlaySoundInfo Create(int serialId, SoundGroup soundGroup, PlaySoundParams playSoundParams,
                Transform bindingTrans, Vector3 worldPosition, object userData)
            {
                PlaySoundInfo playSoundInfo = ReferencePool.Acquire<PlaySoundInfo>();
                playSoundInfo._serialId = serialId;
                playSoundInfo._soundGroup = soundGroup;
                playSoundInfo._playSoundParams = playSoundParams;
                playSoundInfo._bindingTrans = bindingTrans;
                playSoundInfo._worldPosition = worldPosition;
                playSoundInfo._userData = userData;
                return playSoundInfo;
            }

            public void Clear()
            {
                _serialId = 0;
                _soundGroup = null;
                _playSoundParams = null;
                _userData = null;
            }
        }
    }
}