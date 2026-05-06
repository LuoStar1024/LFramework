using UnityEngine;

namespace LFramework
{
    public sealed partial class AudioComponent
    {
        private sealed class PlayAudioInfo : IReference
        {
            private int _serialId;
            private AudioGroup _audioGroup;
            private PlayAudioParams _playAudioParams;
            private Transform _bindingTrans;
            private Vector3 _worldPosition;
            private object _userData;

            public PlayAudioInfo()
            {
                _serialId = 0;
                _audioGroup = null;
                _playAudioParams = null;
                _userData = null;
            }

            public int SerialId
            {
                get { return _serialId; }
            }

            public AudioGroup AudioGroup
            {
                get { return _audioGroup; }
            }

            public PlayAudioParams PlayAudioParams
            {
                get { return _playAudioParams; }
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

            public static PlayAudioInfo Create(int serialId, AudioGroup audioGroup, PlayAudioParams playAudioParams,
                Transform bindingTrans, Vector3 worldPosition, object userData)
            {
                PlayAudioInfo playAudioInfo = ReferencePool.Acquire<PlayAudioInfo>();
                playAudioInfo._serialId = serialId;
                playAudioInfo._audioGroup = audioGroup;
                playAudioInfo._playAudioParams = playAudioParams;
                playAudioInfo._bindingTrans = bindingTrans;
                playAudioInfo._worldPosition = worldPosition;
                playAudioInfo._userData = userData;
                return playAudioInfo;
            }

            public void Clear()
            {
                _serialId = 0;
                _audioGroup = null;
                _playAudioParams = null;
                _bindingTrans = null;
                _worldPosition = Vector3.zero;
                _userData = null;
            }
        }
    }
}